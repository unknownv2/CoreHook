using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using CoreHook.Tests.Plugins.Shared;

namespace CoreHook.Tests
{
    [Collection("Sequential")]
    public class RemoteInjectionTest
    {
        [Fact]
        private void TestRemoteInject64()
        {
            const string TestHookLibrary = "CoreHook.Tests.SimpleParameterTest.dll";
            const string TestMessage = "Berner";

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
               TestMessage);

            Assert.Equal(TestMessage, Resources.ReadFromProcess(testProcess));

            Resources.EndProcess(testProcess);
        }
    }

    [Collection("Sequential")]
    public class RemoteInjectionTest32
    {

        [Fact]
        private void TestRemoteInject32()
        {
            const string TestHookLibrary = "CoreHook.Tests.SimpleParameterTest.dll";
            const string TestMessage = "Berner";

            var testProcess = Resources.StartProcess(Path.Combine(
                            Environment.ExpandEnvironmentVariables("%Windir%"),
                            "SysWOW64",
                            "notepad.exe"
                        ));

            Resources.InjectDllIntoTarget(testProcess,
               Resources.GetTestDllPath(
               TestHookLibrary
               ),
               Resources.GetUniquePipeName(),
               TestMessage);

            Assert.Equal(TestMessage, Resources.ReadFromProcess(testProcess));

            Resources.EndProcess(testProcess);
        }
    }

    [Collection("Sequential")]
    public class RemoteInjectionTestComplex
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
