using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyModel;

namespace CoreHook.DependencyModel.Resolution
{
    internal static class ResolverUtils
    {
        internal static bool TryResolvePackagePath(IFileSystem fileSystem, CompilationLibrary library, string basePath, out string packagePath)
        {
            var path = library.Path;
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(library.Name, library.Version);
            }

            packagePath = Path.Combine(basePath, path);

            if (fileSystem.Directory.Exists(packagePath))
            {
                return true;
            }
            // check all lower case for systems with case sensitive filepath
            if (fileSystem.Directory.Exists(packagePath.ToLower()))
            {
                return true;
            }
            return false;
        }

        internal static bool TryResolveAssemblyFile(IFileSystem fileSystem, string basePath, string assemblyPath, out string fullName)
        {
            fullName = Path.Combine(basePath, assemblyPath);
            if (fileSystem.File.Exists(fullName))
            {
                return true;
            }
            return false;
        }
    }
}
