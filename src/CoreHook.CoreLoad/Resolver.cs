using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using System.Diagnostics;

namespace CoreHook.CoreLoad
{
    internal sealed class Resolver : IDisposable
    {
        private readonly ICompilationAssemblyResolver assemblyResolver;
        private readonly DependencyContext dependencyContext;
        private readonly AssemblyLoadContext loadContext;

        private const string CoreHookModuleName = "CoreHook";

        public Assembly Assembly { get; }

        public Resolver(string path)
        {
            try
            {
                Log($"Image base path is {Path.GetDirectoryName(path)}");

                Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);

                dependencyContext = DependencyContext.Load(Assembly);

                assemblyResolver = new CompositeCompilationAssemblyResolver
                                        (new ICompilationAssemblyResolver[]
                {
                    new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                    new ReferenceAssemblyPathResolver(),
                    new DependencyModel.Resolution.PackageCompilationAssemblyResolver()
                });

                loadContext = AssemblyLoadContext.GetLoadContext(Assembly);

                loadContext.Resolving += OnResolving;
            }
            catch(Exception ex)
            {
                Log($"AssemblyResolver error: {ex.ToString()}");
            }
        }

        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            bool NamesMatchOrContain(RuntimeLibrary runtime)
            {
                bool matched = string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
                // if not matched by exact name or not a default corehook module (which should be matched exactly)
                if (!matched && !runtime.Name.Contains(CoreHookModuleName)){
                    return runtime.Name.IndexOf(name.Name, StringComparison.OrdinalIgnoreCase) >= 0;
                };
                return matched;
            }     

            Log($"OnResolving: {name}");

            try
            {
                RuntimeLibrary library =
                    dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatchOrContain);

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
                    this.assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);

                    if (assemblies.Count > 0)
                    {
                        Log($"Resolved {assemblies[0]}");
                        return loadContext.LoadFromAssemblyPath(assemblies[0]);
                    }
                    else
                    {
                        Log("Failed to resolve assembly");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"OnResolving error: {ex.ToString()}");
            }
            return null;
        }

        public void Dispose()
        {
            loadContext.Resolving -= OnResolving;
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
