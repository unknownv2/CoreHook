using System;
using System.IO;
using System.Reflection;
using CoreHook.BinaryInjection.ProcessUtils;
using CoreHook.BinaryInjection.RemoteInjection;

namespace CoreHook.Examples.Common
{
    public class ModulesPathHelper
    {
        /// <summary>
        /// The name of the .NET Core hosting module for 64-bit processes.
        /// </summary>
        private const string CoreHostModule64 = "corerundll64.dll";
        /// <summary>
        /// The name of the .NET Core hosting module for 32-bit processes.
        /// </summary>
        private const string CoreHostModule32 = "corerundll32.dll";
        /// <summary>
        /// The name of the native detour module for 64-bit processes.
        /// </summary>
        private const string CoreHookingModule64 = "corehook64.dll";
        /// <summary>
        /// The name of the native detour module for 32-bit processes.
        /// </summary>
        private const string CoreHookingModule32 = "corehook32.dll";
        /// <summary>
        /// Module that loads and executes the IEntryPoint.Run method of our hook dll.
        /// It also resolves any dependencies for the CoreHook plugin.
        /// </summary>
        private const string CoreLoadModule = "CoreHook.CoreLoad.dll";

        // For more information o on these environment variables, see: 
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

        private static void HandleFileNotFound(string path)
        {
            Console.WriteLine($"Cannot find file {Path.GetFileName(path)}");
        }

        /// <summary>
        /// Get the path of the .NET Assembly that is first loaded by the host 
        /// and initializes the dependencies for hooking libraries.
        /// </summary>
        /// <param name="coreLoadLibrary">The path to the .NET bootstrap library</param>
        /// <returns>Whether or not the CoreLoad module file exists.</returns>
        public static bool GetCoreLoadModulePath(out string coreLoadLibrary)
        {
            coreLoadLibrary = null;

            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrWhiteSpace(currentDir))
            {
                return false;
            }

            var coreLoadPath = Path.Combine(currentDir, CoreLoadModule);

            if (!File.Exists(coreLoadPath))
            {
                HandleFileNotFound(coreLoadPath);
                return false;
            }

            coreLoadLibrary = coreLoadPath;

            return true;
        }

        /// <summary>
        /// Get the path of the .NET Assembly that is first loaded by the host 
        /// and initializes the dependencies for hooking libraries.
        /// </summary>
        /// <param name="is64BitProcess">Value to determine which native modules path to look for.</param>
        /// <param name="coreLibsPath">Path to the CoreCLR native modules.</param>
        /// <param name="coreRootPath">Path to the CoreCLR native modules.</param>
        /// <returns>Whether the CoreCLR path was found or not.</returns>
        public static bool GetCoreClrRootPath(
            bool is64BitProcess,
            out string coreLibsPath,
            out string coreRootPath)
        {
            // Paths to the CoreCLR dlls used to host and execute .NET assemblies 
            coreLibsPath = GetCoreLibrariesPath(is64BitProcess);
            coreRootPath = GetCoreRootPath(is64BitProcess);

            if (string.IsNullOrWhiteSpace(coreRootPath) && string.IsNullOrWhiteSpace(coreLibsPath))
            {
                Console.WriteLine(is64BitProcess
                    ? "CoreCLR root path was not set for 64-bit processes."
                    : "CoreCLR root path was not set for 32-bit processes.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieve the required paths for initializing the CoreCLR and executing .NET assemblies in an unmanaged process
        /// </summary>
        /// <param name="is64BitProcess">Flag for determining which native modules to load into the target process</param>
        /// <param name="corehookConfig">Configuration class containing paths to the native modules used by CoreHook.</param>
        /// <returns>Returns whether all required paths and modules have been found.</returns>
        public static bool GetCoreLoadPaths(
            bool is64BitProcess,
            out CoreHookNativeConfig corehookConfig)
        {
            corehookConfig = null;

            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!string.IsNullOrWhiteSpace(currentDir) && GetCoreClrRootPath(
                is64BitProcess,
                out string coreLibsPath,
                out string coreRootPath))
            {
                // Module that initializes the .NET Core runtime and executes .NET assemblies
                var coreRunPath = Path.Combine(
                    currentDir,
                    is64BitProcess ? CoreHostModule64 : CoreHostModule32);
                if (!File.Exists(coreRunPath))
                {
                    HandleFileNotFound(coreRunPath);
                    return false;
                }

                var corehookPath = Path.Combine(
                    currentDir,
                    is64BitProcess ? CoreHookingModule64 : CoreHookingModule32);
                if (!File.Exists(corehookPath))
                {
                    HandleFileNotFound(corehookPath);
                    return false;
                }

                corehookConfig = new CoreHookNativeConfig
                {
                    ClrLibrariesPath = coreLibsPath,
                    ClrRootPath = coreRootPath,
                    HostLibrary = coreRunPath,
                    DetourLibrary = corehookPath
                };

                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieve the required paths for initializing the CoreCLR and executing .NET assemblies in an unmanaged process
        /// </summary>
        /// <param name="is64BitProcess">Flag for determining which native modules to load into the target process</param>
        /// <param name="coreRunPath">The native module that we call to execute host and execute our hooking dll in the target process</param>
        /// <param name="coreLibsPath">Path to the CoreCLR dlls that implement the .NET Core runtime.</param>
        /// <param name="coreRootPath">Path to the CoreCLR dlls  that implement the .NET Core runtime.</param>
        /// <param name="coreLoadPath">Initial .NET module that loads and executes our hooking dll, and handles dependency resolution.</param>
        /// <param name="corehookPath">Native corehook module that implements the functions required to detour functions.</param>
        /// <returns>Returns whether all required paths and modules have been found.</returns>
        public static bool GetCoreLoadPaths(
            bool is64BitProcess,
            out string coreRunPath,
            out string coreLibsPath,
            out string coreRootPath,
            out string coreLoadPath,
            out string corehookPath)
        {
            coreRunPath = string.Empty;
            coreLoadPath = string.Empty;
            corehookPath = string.Empty;
            coreLibsPath = string.Empty;
            coreRootPath = string.Empty;

            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!string.IsNullOrWhiteSpace(currentDir) && GetCoreClrRootPath(
                is64BitProcess,
                out coreLibsPath,
                out coreRootPath))
            {
                // Module  that initializes the .NET Core runtime and executes .NET assemblies
                coreRunPath = Path.Combine(currentDir,
                    is64BitProcess ? CoreHostModule64 : CoreHostModule32);
                if (!File.Exists(coreRunPath))
                {
                    HandleFileNotFound(coreRunPath);
                    return false;
                }

                if (GetCoreLoadModulePath(out coreLoadPath))
                {
                    corehookPath = Path.Combine(currentDir,
                         is64BitProcess ? CoreHookingModule64 : CoreHookingModule32);
                    if (!File.Exists(corehookPath))
                    {
                        HandleFileNotFound(corehookPath);
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
