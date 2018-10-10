using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CoreHook.ManagedHook.ProcessUtils;
using CoreHook.ManagedHook.Remote;

namespace CoreHook.Examples.Common
{
    public class Utilities
    {
        // info on these environment variables: 
        // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
        public static string GetCoreLibrariesPath(bool is64BitProcess)
        {
            return !ProcessHelper.IsArchitectureArm() ?
             (
                 is64BitProcess ?
                 Environment.GetEnvironmentVariable("CORE_LIBRARIES_64") :
                 Environment.GetEnvironmentVariable("CORE_LIBRARIES_32")
             )
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetCoreRootPath(bool is64BitProcess)
        {
            return !ProcessHelper.IsArchitectureArm() ?
             (
                is64BitProcess ?
                Environment.GetEnvironmentVariable("CORE_ROOT_64") :
                Environment.GetEnvironmentVariable("CORE_ROOT_32")
             )
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        /// <summary>
        /// Retrieve the required paths for initializing the CoreCLR and executing .NET assemblies in an unmanaged process
        /// </summary>
        /// <param name="is64BitProcess">Flag for determining which native modules to load into the target process</param>
        /// <param name="corehookConfig"></param>
        /// <returns>Returns wether all required paths and modules have been found.</returns>
        public static bool GetCoreLoadPaths(bool is64BitProcess, out CoreHookNativeConfig corehookConfig)
        {
            corehookConfig = null;
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Paths to the CoreCLR dlls used to host and execute .NET assemblies 
            var coreLibsPath = GetCoreLibrariesPath(is64BitProcess);
            var coreRootPath = GetCoreRootPath(is64BitProcess);

            if (string.IsNullOrEmpty(coreRootPath) && string.IsNullOrEmpty(coreLibsPath))
            {
                Console.WriteLine("CoreCLR root path was not set!");
                return false;
            }

            // Module  that initializes the .NET Core runtime and executes .NET assemblies
            var coreRunPath = Path.Combine(currentDir,
                is64BitProcess ? "corerundll64.dll" : "corerundll32.dll");
            if (!File.Exists(coreRunPath))
            {
                Console.WriteLine("Cannot find the corerun dll");
                return false;
            }
            // Module that loads and executes the IEntryPoint.Run method of our hook dll.
            // It also resolves any dependencies for the hook dll
            var coreLoadPath = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

            if (!File.Exists(coreLoadPath))
            {
                Console.WriteLine("Cannot find the CoreLoad dll");
                return false;
            }

            var corehookPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                 is64BitProcess ? "corehook64.dll" : "corehook32.dll");

            if (!File.Exists(coreLoadPath))
            {
                Console.WriteLine($"Cannot find {Path.GetFileName(corehookPath)}");
                return false;
            }

            corehookConfig = new CoreHookNativeConfig()
            {
                CoreCLRLibrariesPath = coreLibsPath,
                CoreCLRPath = coreRootPath,
                HostLibrary = coreRunPath,
                DetourLibrary = corehookPath
            };

            return true;
        }
        /// <summary>
        /// Retrieve the required paths for initializing the CoreCLR and executing .NET assemblies in an unmanaged process
        /// </summary>
        /// <param name="is64BitProcess">Flag for determining which native modules to load into the target process</param>
        /// <param name="coreRunPath">The native module that we call to execute host and execute our hooking dll in the target process</param>
        /// <param name="coreLibsPath">Path to the CoreCLR dlls that implement the .NET Core runtime</param>
        /// <param name="coreRootPath">Path to the CoreCLR dlls  that implement the .NET Core runtime</param>
        /// <param name="coreLoadPath">Initial .NET module that loads and executes our hooking dll, and handles dependency resolution.</param>
        /// <param name="corehookPath">Native corehook module that implements the functions required to detour functions.</param>
        /// <returns>Returns wether all required paths and modules have been found.</returns>
        public static bool GetCoreLoadPaths(bool is64BitProcess, out string coreRunPath, out string coreLibsPath, out string coreRootPath, out string coreLoadPath, out string corehookPath)
        {
            coreRunPath = string.Empty;
            coreLoadPath = string.Empty;
            corehookPath = string.Empty;

            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Paths to the CoreCLR dlls used to host and execute .NET assemblies 
            coreLibsPath = GetCoreLibrariesPath(is64BitProcess);
            coreRootPath = GetCoreRootPath(is64BitProcess);

            if (string.IsNullOrEmpty(coreRootPath) && string.IsNullOrEmpty(coreLibsPath))
            {
                Console.WriteLine("CoreCLR root path was not set!");
                return false;
            }

            // Module  that initializes the .NET Core runtime and executes .NET assemblies
            coreRunPath = Path.Combine(currentDir,
                is64BitProcess ? "corerundll64.dll" : "corerundll32.dll");
            if (!File.Exists(coreRunPath))
            {
                Console.WriteLine("Cannot find the corerun dll");
                return false;
            }

            // Module that loads and executes the IEntryPoint.Run method of our hook dll.
            // It also resolves any dependencies for the hook dll
            coreLoadPath = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

            if (!File.Exists(coreLoadPath))
            {
                Console.WriteLine("Cannot find CoreLoad dll");
                return false;
            }

            corehookPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                 is64BitProcess ? "corehook64.dll" : "corehook32.dll");

            if (!File.Exists(coreLoadPath))
            {
                Console.WriteLine($"Cannot find {Path.GetFileName(corehookPath)}");
                return false;
            }

            return true;
        }
    }
}
