using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace CoreHook.CoreLoad;

internal class PackageCompilationAssemblyResolver : ICompilationAssemblyResolver
{
    private readonly string[] _nugetPackageDirectories;

    public PackageCompilationAssemblyResolver(params string[] nugetPackageDirectories)
    {
        _nugetPackageDirectories = nugetPackageDirectories;
    }

    private static string[] GetDefaultProbeDirectories()
    {
#if !NETSTANDARD1_3            
#if NETSTANDARD1_6
        var probeDirectories = AppContext.GetData("PROBING_DIRECTORIES");
#else
        var probeDirectories = AppDomain.CurrentDomain.GetData("PROBING_DIRECTORIES");
#endif

        var listOfDirectories = probeDirectories as string;

        if (!string.IsNullOrEmpty(listOfDirectories))
        {
            return listOfDirectories.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
        }
#endif

        var packageDirectory = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

        if (!string.IsNullOrEmpty(packageDirectory))
        {
            return new string[] { packageDirectory };
        }

        string basePath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            basePath = Environment.GetEnvironmentVariable("USERPROFILE");
        }
        else
        {
            basePath = Environment.GetEnvironmentVariable("HOME");
        }

        if (string.IsNullOrEmpty(basePath))
        {
            return new string[] { string.Empty };
        }

        return new string[] { Path.Combine(basePath, ".nuget", "packages") };

    }

    public bool TryResolveAssemblyPaths(CompilationLibrary library, List<string> assemblies)
    {
        if (_nugetPackageDirectories is null || _nugetPackageDirectories.Length == 0 ||
            !string.Equals(library.Type, "package", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (var directory in _nugetPackageDirectories)
        {
            string packagePath;

            if (ResolverUtils.TryResolvePackagePath(library, directory, out packagePath))
            {

                IEnumerable<string> fullPathsFromPackage;
                if (TryResolveFromPackagePath(library, packagePath, out fullPathsFromPackage))
                {
                    assemblies.AddRange(fullPathsFromPackage);
                    return true;
                }
            }
        }
        return false;
    }

    private static bool TryResolveFromPackagePath(CompilationLibrary library, string basePath, out IEnumerable<string> results)
    {
        var paths = new List<string>();

        foreach (var assembly in library.Assemblies)
        {
            string fullName;
            if (!ResolverUtils.TryResolveAssemblyFile(basePath, assembly, out fullName))
            {
                // If one of the files can't be found, skip this package path completely.
                // there are package paths that don't include all of the "ref" assemblies 
                // (ex. ones created by 'dotnet store')
                results = null;
                return false;
            }

            paths.Add(fullName);
        }

        results = paths;
        return true;
    }
}
