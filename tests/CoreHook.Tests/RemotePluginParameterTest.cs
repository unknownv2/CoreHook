using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using CoreHook.Tests.Plugins.Shared;

namespace CoreHook.Tests
{
    public class RemotePluginParameterTest
    {
        [Fact]
        private void TestRemotePluginComplexParameter()
        {
            const string TestHookLibrary = "CoreHook.Tests.ComplexParameterTest.dll";
            const string TestMessage = "Berner";
            
            var complexParameter = new ComplexParameter
            {
                Message = TestMessage,
                HostProcessId = Process.GetCurrentProcess().Id
            };

            var testProcess = Resources.StartProcess(Path.Combine(
                            Environment.ExpandEnvironmentVariables("%Windir%"),
                            "System32",
                            "notepad.exe"
                        ));

            Resources.InjectDllIntoTarget(testProcess,
               Resources.GetTestDllPath(
               TestHookLibrary
               ),
               Resources.GetUniquePipeName(),
               complexParameter);

            Assert.Equal(complexParameter.Message, Resources.ReadFromProcess(testProcess));
            Assert.Equal(complexParameter.HostProcessId.ToString(), Resources.ReadFromProcess(testProcess));

            Resources.EndProcess(testProcess);
        }
    }
}
