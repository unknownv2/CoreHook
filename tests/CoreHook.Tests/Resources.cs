using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CoreHook.BinaryInjection.RemoteInjection;
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
                    _testProcess64 = new Process
                    {
                        StartInfo =
                        {
                            FileName = Path.Combine(
                                Environment.ExpandEnvironmentVariables("%Windir%"),
                                "System32",
                                "notepad.exe"
                            ),
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true
                        }
                    };

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
                    _testProcess32 = new Process
                    {
                        StartInfo =
                        {
                            FileName = Path.Combine(
                                Environment.ExpandEnvironmentVariables("%Windir%"),
                                "SysWOW64",
                                "notepad.exe"
                            ),
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true
                        }
                    };

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
        internal static void EndProcess(Process process)
        {
            process?.Kill();
            process = null;
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
                    _targetApp = new Process
                    {
                        StartInfo =
                        {
                            FileName = "dotnet",
                            Arguments = Path.Combine(
                                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                TestModuleDir,
                                TargetAppName
                            ),
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true
                        }
                    };

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

        internal static Process StartProcess(string fileName)
        {
            var testProcess = new Process
            {
                StartInfo =
                    {
                        FileName = fileName,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true
                    }
            };

            testProcess.Start();
            
            return testProcess;
        }
        internal static void SendToProcess(Process target, string message)
        {
            target.StandardInput.WriteLine(message);
        }

        internal static string ReadFromProcess(Process target)
        {
            return target.StandardOutput.ReadLine();
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
            string injectionPipeName,
            params object[] remoteArguments
            )
        {
            if (Examples.Common.ModulesPathHelper.GetCoreLoadPaths(
                    target.Is64Bit(), out CoreHookNativeConfig nativeConfig) &&
                Examples.Common.ModulesPathHelper.GetCoreLoadModulePath(
                    out string coreLoadLibrary))
            {
                RemoteInjector.Inject(
                    target.Id,
                    new RemoteInjectorConfig(nativeConfig)
                    {
                        InjectionPipeName = injectionPipeName,
                        ClrBootstrapLibrary = coreLoadLibrary,
                        PayloadLibrary = injectionLibrary,
                        VerboseLog = false
                    },
                    new PipePlatformBase(),
                    remoteArguments);
            }
        }

        internal static string GetUniquePipeName()
        {
            return Path.GetRandomFileName();
        }
    }
}
