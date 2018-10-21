using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CoreHook.ManagedHook.Remote;
using CoreHook.Memory;

namespace CoreHook.Tests
{
    internal static class Resources
    {
        private const string TestModuleDir = "Test";

        private static Process _testProcess64;
        private static Process _testProcess32;

        internal static Process TestProcess
        {
            get
            {
                if(_testProcess64 == null)
                {
                    _testProcess64 = new Process();

                    _testProcess64.StartInfo.FileName = Path.Combine(
                            Environment.ExpandEnvironmentVariables("%Windir%"),
                            "System32",
                            "notepad.exe"
                            );

                    _testProcess64.StartInfo.UseShellExecute = false;
                    _testProcess64.StartInfo.RedirectStandardInput = true;
                    _testProcess64.StartInfo.RedirectStandardOutput = true;
                    _testProcess64.Start();
                }
                return _testProcess64;
            }
        }
        internal static Process TestProcess2
        {
            get
            {
                if (_testProcess32 == null)
                {
                    _testProcess32 = new Process();

                    _testProcess32.StartInfo.FileName = Path.Combine(
                            Environment.ExpandEnvironmentVariables("%Windir%"),
                            "SysWOW64",
                            "notepad.exe"
                            );

                    _testProcess32.StartInfo.UseShellExecute = false;
                    _testProcess32.StartInfo.RedirectStandardInput = true;
                    _testProcess32.StartInfo.RedirectStandardOutput = true;
                    _testProcess32.Start();
                }
                return _testProcess32;
            }
        }

        internal static void EndTestProcess()
        {
            _testProcess64?.Kill();
            _testProcess64 = null;
        }
        internal static void EndTestProcess2()
        {
            _testProcess32?.Kill();
            _testProcess32 = null;
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
                    _targetApp.StartInfo.Arguments =
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                            TestModuleDir, 
                            TargetAppName
                        );

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
 
        internal static string GetTestDllPath(string dllName)
        {
            return Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                TestModuleDir,
                dllName
                );
        }
        internal static void InjectDllIntoTarget(
            Process target,
            string injectionLibrary,
            string message,
            string pipeName = "CoreHookInjection")
        {
            if (Examples.Common.Utilities.GetCoreLoadPaths(target.Is64Bit(),
                out string coreRunDll, out string coreLibrariesPath,
                out string coreRootPath, out string coreLoadDll, out string coreHookDll))
            {
                RemoteHooking.Inject(
                    target.Id,
                    new RemoteHookingConfig()
                    {
                        HostLibrary = coreRunDll,
                        CoreCLRPath = coreRootPath,
                        CoreCLRLibrariesPath = coreLibrariesPath,
                        CLRBootstrapLibrary = coreLoadDll,
                        DetourLibrary = coreHookDll,
                        PayloadLibrary = injectionLibrary,
                        VerboseLog = false,
                        WaitForDebugger = false,
                        InjectionPipeName = pipeName
                    },
                    new PipePlatformBase(),
                    message);
            }
        }
    }
}
