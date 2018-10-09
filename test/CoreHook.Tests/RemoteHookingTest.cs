using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Xunit;
using CoreHook.ManagedHook.Remote;
using CoreHook.ManagedHook.ProcessUtils;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CoreHook.Tests
{
    public class RemoteHookingTest
    {
        [Fact]
        private void TestRemoteInject()
        {
            const string TestHookLibrary = "CoreHook.Tests.SimpleHook1.dll";
            const string TestMessage = "Berner";

            InjectDllIntoTarget(Resources.TestProcess,
               Resources.GetTestDllPath(
               TestHookLibrary
               ),
               TestMessage);

            Assert.Equal(TestMessage, Resources.ReadFromProcess(Resources.TestProcess));

            Resources.EndTestProcess();
        }

        //[Fact]
        private void TestTargetAppRemoteInject()
        {
            const string TestHookLibrary = "CoreHook.Tests.SimpleHook1.dll";
            const string TestMessage = "Berner";

            InjectDllIntoTarget(Resources.TargetProcess,
               Resources.GetTestDllPath(
               TestHookLibrary
               ),
               TestMessage);

            Assert.Equal(TestMessage, Resources.ReadFromProcess(Resources.TargetProcess));

            Resources.EndTargetAppProcess();
        }

        public static void InjectDllIntoTarget(Process target, string injectionLibrary, string message)
        {
            if (Resources.GetCoreLoadPaths(
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
                        StartAssembly = false
                    },
                    new PipePlatformBase(),
                    message);
            }
        }
    }
}
