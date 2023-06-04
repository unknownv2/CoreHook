using Microsoft.Extensions.DependencyModel;

using System.IO;

namespace CoreHook.Loader;

internal static class ResolverUtils
{
    internal static bool TryResolvePackagePath(CompilationLibrary library, string basePath, out string packagePath)
    {
        var path = library.Path;
        if (string.IsNullOrEmpty(path))
        {
            path = Path.Combine(library.Name, library.Version);
        }

        packagePath = Path.Combine(basePath, path);

        if (Directory.Exists(packagePath))
        {
            return true;
        }
        // Check all lower case for systems with case sensitive filepath
        if (Directory.Exists(packagePath.ToLower()))
        {
            return true;
        }
        return false;
    }


    internal static bool TryResolveAssemblyFile(string basePath, string assemblyPath, out string fullName)
    {
        // Several checks for determining whether a file exists since 
        // Linux filesystems have case sensitive filepaths
        // for their Nuget Package paths
        fullName = Path.Combine(basePath, assemblyPath);
        if (File.Exists(fullName))
        {
            return true;
        }
        var fullNameLowercase = fullName.ToLower();
        if (File.Exists(fullNameLowercase))
        {
            fullName = fullNameLowercase;
            return true;
        }
        var dirName = Path.GetDirectoryName(fullNameLowercase);

        if (Directory.Exists(dirName))
        {
            fullName = Path.Combine(dirName, Path.GetFileName(fullName));
            return true;
        }
        return false;
    }
}
