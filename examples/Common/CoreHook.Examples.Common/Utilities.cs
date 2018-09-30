using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CoreHook.ManagedHook.ProcessUtils;

namespace CoreHook.Examples.Common
{
    public class Utilities
    {
        // info on these environment variables: 
        // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
        public static string GetCoreLibrariesPath()
        {
            return !ProcessHelper.IsArchitectureArm() ?
             (
                 Environment.Is64BitProcess ?
                 Environment.GetEnvironmentVariable("CORE_LIBRARIES_64") :
                 Environment.GetEnvironmentVariable("CORE_LIBRARIES_32")
             )
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetCoreRootPath()
        {
            return !ProcessHelper.IsArchitectureArm() ?
             (
                Environment.Is64BitProcess ?
                Environment.GetEnvironmentVariable("CORE_ROOT_64") :
                Environment.GetEnvironmentVariable("CORE_ROOT_32")
             )
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        /// <summary>
        /// Retrieve the required paths for initializing the CoreCLR and executing .NET assemblies in an unmanaged process
        /// </summary>
        /// <param name="coreRunPath">The native module that we call to execute host and execute our hooking dll in the target process</param>
        /// <param name="coreLibsPath">Path to the CoreCLR dlls that implement the .NET Core runtime</param>
        /// <param name="coreRootPath">Path to the CoreCLR dlls  that implement the .NET Core runtime</param>
        /// <param name="coreLoadPath">Initial .NET module that loads and executes our hooking dll, and handles dependency resolution.</param>
        /// <returns>Returns wether all required paths and modules have been found.</returns>
        public static bool GetCoreLoadPaths(out string coreRunPath, out string coreLibsPath, out string coreRootPath, out string coreLoadPath)
        {
            coreRunPath = string.Empty;
            coreLoadPath = string.Empty;

            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Paths to the CoreCLR dlls used to host and execute .NET assemblies 
            coreLibsPath = GetCoreLibrariesPath();
            coreRootPath = GetCoreRootPath();

            if (string.IsNullOrEmpty(coreRootPath) && string.IsNullOrEmpty(coreLibsPath))
            {
                Console.WriteLine("CoreCLR root path was not set!");
                return false;
            }

            // Module  that initializes the .NET Core runtime and executes .NET assemblies
            coreRunPath = Path.Combine(currentDir,
                Environment.Is64BitProcess ? "corerundll64.dll" : "corerundll32.dll");
            if (!File.Exists(coreRunPath))
            {
                coreRunPath = Environment.GetEnvironmentVariable("CORERUNDLL");
                if (!File.Exists(coreRunPath))
                {
                    Console.WriteLine("Cannot find corerun dll");
                    return false;
                }
            }

            // Module that loads and executes the IEntryPoint.Run method of our hook dll.
            // It also resolves any dependencies for the hook dll
            coreLoadPath = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

            if (!File.Exists(coreLoadPath))
            {
                Console.WriteLine("Cannot find CoreLoad dll");
                return false;
            }

            return true;
        }
    }
}
