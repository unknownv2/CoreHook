using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Extensions.DependencyModel;
using System.Diagnostics;

namespace CoreHook.DependencyModel
{
    internal sealed class AssemblyResolver : IDisposable
    {
        private readonly ICompilationAssemblyResolver _assemblyResolver;
        private readonly DependencyContext _dependencyContext;
        private readonly AssemblyLoadContext _loadContext;

        public AssemblyResolver(string path)
        {
            try
            {
                Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);

                _dependencyContext = DependencyContext.Load(Assembly);

                _assemblyResolver = new CompositeCompilationAssemblyResolver
                                        (new ICompilationAssemblyResolver[]
                {
                    new Resolution.AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                    new Resolution.PackageCompilationAssemblyResolver(),
                    new ReferenceAssemblyPathResolver()
                });

                _loadContext = AssemblyLoadContext.GetLoadContext(Assembly);

                _loadContext.Resolving += OnResolving;
            }
            catch (Exception ex)
            {
                Log($"AssemblyResolver error: {ex.ToString()}");
            }
        }

        public Assembly Assembly { get; }

        public void Dispose()
        {
            _loadContext.Resolving -= OnResolving;
        }

        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            bool NamesMatch(RuntimeLibrary runtime)
            {
                return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
            }
            bool NamesContain(RuntimeLibrary runtime)
            {
                return runtime.Name.IndexOf(name.Name, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            Log($"OnResolving: {name}");

            try
            {
                RuntimeLibrary library =
                    _dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);

                if (library == null)
                {
                    library =
                        _dependencyContext.RuntimeLibraries.FirstOrDefault(NamesContain);
                }
                if (library != null)
                {
                    var wrapper = new CompilationLibrary(
                        library.Type,
                        library.Name,
                        library.Version,
                        library.Hash,
                        library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                        library.Dependencies,
                        library.Serviceable);

                    var assemblies = new List<string>();
                    _assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);

                    if (assemblies.Count > 0)
                    {
                        Log($"Resolved {assemblies[0]}");
                        return _loadContext.LoadFromAssemblyPath(assemblies[0]);
                    }

                    Log("Failed to resolve assembly");
                    
                }
            }
            catch (Exception ex)
            {
                Log($"OnResolving error: {ex}");
            }
            return null;
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
