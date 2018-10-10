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
