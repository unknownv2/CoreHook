using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Extensions.DependencyModel;

namespace CoreHook.DependencyModel.Resolution
{
    class PackageCompilationAssemblyResolver : ICompilationAssemblyResolver
    {
        public static void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        private readonly IFileSystem _fileSystem;
        private readonly string[] _nugetPackageDirectories;

        public PackageCompilationAssemblyResolver()
            : this(EnvironmentWrapper.Default, FileSystemWrapper.Default)
        {
        }

        public PackageCompilationAssemblyResolver(string nugetPackageDirectory)
            : this(FileSystemWrapper.Default, new string[] { nugetPackageDirectory })
        {
        }

        internal PackageCompilationAssemblyResolver(IEnvironment environment,
            IFileSystem fileSystem)
            : this(fileSystem, GetDefaultProbeDirectories(environment))
        {
        }

        internal PackageCompilationAssemblyResolver(IFileSystem fileSystem, string[] nugetPackageDirectories)
        {
            _fileSystem = fileSystem;
            _nugetPackageDirectories = nugetPackageDirectories;
        }
        private static string[] GetDefaultProbeDirectories(IEnvironment environment) =>
                GetDefaultProbeDirectories(RuntimeEnvironment.OperatingSystemPlatform, environment);

        internal static string[] GetDefaultProbeDirectories(Platform osPlatform, IEnvironment environment)
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
                Log($"List of file directories: {listOfDirectories}");

                return listOfDirectories.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            }
#endif

            var packageDirectory = environment.GetEnvironmentVariable("NUGET_PACKAGES");

            if (!string.IsNullOrEmpty(packageDirectory))
            {
                return new string[] { packageDirectory };
            }

            string basePath;
            if (osPlatform == Platform.Windows)
            {
                basePath = environment.GetEnvironmentVariable("USERPROFILE");
            }
            else
            {
                basePath = environment.GetEnvironmentVariable("HOME");
            }

            if (string.IsNullOrEmpty(basePath))
            {
                return new string[] { string.Empty };
            }

            return new string[] { Path.Combine(basePath, ".nuget", "packages") };

        }

        public bool TryResolveAssemblyPaths(CompilationLibrary library, List<string> assemblies)
        {
            if (_nugetPackageDirectories == null || _nugetPackageDirectories.Length == 0 ||
                !string.Equals(library.Type, "package", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            foreach (var directory in _nugetPackageDirectories)
            {
                string packagePath;
                Log($"TryResolveAssemblyPaths: {directory}");

                if (ResolverUtils.TryResolvePackagePath(_fileSystem, library, directory, out packagePath))
                {
                    Log($"TryResolveAssemblyPaths packagePath: {packagePath}");

                    IEnumerable<string> fullPathsFromPackage;
                    if (TryResolveFromPackagePath(_fileSystem, library, packagePath, out fullPathsFromPackage))
                    {
                        assemblies.AddRange(fullPathsFromPackage);
                        return true;
                    }
                }
            }
            return false;
        }


        private static bool TryResolveFromPackagePath(IFileSystem fileSystem, CompilationLibrary library, string basePath, out IEnumerable<string> results)
        {
            var paths = new List<string>();

            foreach (var assembly in library.Assemblies)
            {
                string fullName;
                if (!ResolverUtils.TryResolveAssemblyFile(fileSystem, basePath, assembly, out fullName))
                {
                    Log($"TryResolveAssemblyFile fullName: {fullName} failed before UWP_Acccess");

                    //PipeHelper.SendPipeMsg(fullName);
                    if (!ResolverUtils.TryResolveAssemblyFile(fileSystem, basePath, assembly, out fullName))
                    {
                        Log($"TryResolveAssemblyFile fullName: {fullName} failed before UWP_Acccess");

                        // if one of the files can't be found, skip this package path completely.
                        // there are package paths that don't include all of the "ref" assemblies 
                        // (ex. ones created by 'dotnet store')
                        results = null;
                        return false;
                    }
                }
                Log($"TryResolveFromPackagePath fullName: {fullName}");

                paths.Add(fullName);
            }

            Log($"TryResolveFromPackagePath success");

            results = paths;
            return true;
        }
    }
}
