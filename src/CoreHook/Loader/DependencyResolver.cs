﻿using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace CoreHook.Loader;

/// <summary>
/// Resolves assembly dependencies for the plugins during initialization,
/// such as NuGet packages dependencies.
/// </summary>
internal sealed class DependencyResolver
{
    private readonly ICompilationAssemblyResolver _assemblyResolver;
    private readonly DependencyContext _dependencyContext;
    private readonly AssemblyLoadContext _loadContext;

    private const string CoreHookModuleName = "CoreHook";

    public Assembly Assembly { get; }

    public DependencyResolver(string path)
    {
        try
        {
            Log($"Image base path is {Path.GetDirectoryName(path)}");

            Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            
            _dependencyContext = DependencyContext.Load(Assembly);

            _assemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
            {
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                new ReferenceAssemblyPathResolver(),
                new Loader.PackageCompilationAssemblyResolver()
            });

            _loadContext = AssemblyLoadContext.GetLoadContext(Assembly);

            _loadContext.Resolving += OnResolving;
        }
        catch (Exception e)
        {
            Log($"AssemblyResolver error: {e}");
        }
    }

    private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
    {
        bool NamesMatchOrContain(RuntimeLibrary runtime)
        {
            bool matched = string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
            // if not matched by exact name or not a default corehook module (which should be matched exactly)
            if (!matched && !runtime.Name.Contains(CoreHookModuleName))
            {
                return runtime.Name.Contains(name.Name, StringComparison.OrdinalIgnoreCase);
            }
            return matched;
        }

        Log($"OnResolving: {name}");

        try
        {
            RuntimeLibrary library = _dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatchOrContain);

            if (library is not null)
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
                else
                {
                    Log("Failed to resolve assembly");
                }
            }
        }
        catch (Exception e)
        {
            Log($"OnResolving error: {e}");
        }
        return null;
    }

    public void Dispose()
    {
        _loadContext.Resolving -= OnResolving;
    }

    private static void Log(string message)
    {
        Debug.WriteLine(message);
    }
}
