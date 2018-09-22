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

        private void InjectDllIntoTarget(Process target, string injectionLibrary, string message)
        {
            string coreRunDll, coreLibrariesPath, coreRootPath, coreLoadDll, coreHookDll;
            if (Resources.GetCoreLoadPaths(
                out coreRunDll, out coreLibrariesPath,
                out coreRootPath, out coreLoadDll, out coreHookDll))
            {
                RemoteHooking.Inject(
                    target.Id,
                    coreRunDll,
                    coreLoadDll,
                    coreRootPath,
                    coreLibrariesPath, 
                    injectionLibrary,
                    injectionLibrary,
                    new PipePlatformBase(),
                    new[] { coreHookDll },
                    message);
            }
        }
    }
}
