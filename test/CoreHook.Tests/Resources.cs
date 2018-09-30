using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using CoreHook.ManagedHook.ProcessUtils;

namespace CoreHook.Tests
{
    internal static class Resources
    {
        private const string TestModuleDir = "Test";
        private static Process _testProcess;

        internal static Process TestProcess
        {
            get
            {
                if(_testProcess == null)
                {          
                    _testProcess = new Process();

                    _testProcess.StartInfo.FileName = Path.Combine(
                            Environment.ExpandEnvironmentVariables("%Windir%"),
                            "System32",
                            "notepad.exe"
                            );

                    _testProcess.StartInfo.UseShellExecute = false;
                    _testProcess.StartInfo.RedirectStandardInput = true;
                    _testProcess.StartInfo.RedirectStandardOutput = true;
                    _testProcess.Start();
                }
                return _testProcess;
            }            
        }

        internal static void EndTestProcess()
        {
            _testProcess?.Kill();
            _testProcess = null;
        }

        private const string TargetAppName = "CoreHook.Tests.TargetApp.dll";
        private static Process _targetApp;

        internal static Process TargetProcess
        {
            get
            {                
                if (_targetApp == null)
                {
                    _targetApp = new Process();

                    _targetApp.StartInfo.FileName = "dotnet";
                    _targetApp.StartInfo.Arguments = $"{Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Test", TargetAppName)}";

                    _targetApp.StartInfo.UseShellExecute = false;
                    _targetApp.StartInfo.RedirectStandardInput = true;
                    _targetApp.StartInfo.RedirectStandardOutput = true;
                    _targetApp.Start();
                }

                return _targetApp;
            }
        }

        internal static void EndTargetAppProcess()
        {
            _targetApp?.Kill();
            _targetApp = null;
        }

        internal static void SendToProcess(Process target, string message)
        {
            using (StreamWriter sw = target.StandardInput)
            {
                sw.WriteLine(message);
            }
        }

        internal static string ReadFromProcess(Process target)
        {
            using (StreamReader sr = target.StandardOutput)
            {
                return sr.ReadLine();
            }
        }

        // info on these environment variables: 
        // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
        internal static string GetCoreLibrariesPath()
        {
            return !ProcessHelper.IsArchitectureArm() ?
             (
                 Environment.Is64BitProcess ?
                 Environment.GetEnvironmentVariable("CORE_LIBRARIES_64") :
                 Environment.GetEnvironmentVariable("CORE_LIBRARIES_32")
             )
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetCoreRootPath()
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
        /// <param name="corehookPath">Native corehook module that implements the functions required to detour functions.</param>
        /// <returns>Returns wether all required paths and modules have been found.</returns>
        internal static bool GetCoreLoadPaths(out string coreRunPath, out string coreLibsPath, out string coreRootPath, out string coreLoadPath, out string corehookPath)
        {
            coreRunPath = string.Empty;
            coreLoadPath = string.Empty;
            corehookPath = string.Empty;

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

            corehookPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                 Environment.Is64BitProcess ? "corehook64.dll" : "corehook32.dll");

            if (!File.Exists(coreLoadPath))
            {
                Console.WriteLine($"Cannot find {Path.GetFileName(corehookPath)}");
                return false;
            }
            return true;
        }
        internal static string GetTestDllPath(string dllName)
        {
            return Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                TestModuleDir,
                dllName
                );
        }
    }
}
