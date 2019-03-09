using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using CoreHook.Tests.Plugins.Shared;

namespace CoreHook.Tests
{
    [Collection("Remote Injection Tests")]
    public class RemoteInjectionTestSimpleParameter
    {
        [Theory]
        [InlineData("System32")]
        [InlineData("SysWOW64")]
        private void TestRemotePluginSimpleParameter(string applicationFolder)
        {
            const string testHookLibrary = "CoreHook.Tests.SimpleParameterTest.dll";
            const string remoteArgument = "Berner";

            var testProcess = Resources.StartProcess(Path.Combine(
                            Environment.ExpandEnvironmentVariables("%Windir%"),
                            applicationFolder,
                            "notepad.exe"
                        ));

            System.Threading.Thread.Sleep(500);

            Resources.InjectDllIntoTarget(testProcess,
               Resources.GetTestDllPath(
               testHookLibrary
               ),
               Resources.GetUniquePipeName(),
               remoteArgument);

            Assert.Equal(remoteArgument, Resources.ReadFromProcess(testProcess));

            Resources.EndProcess(testProcess);
        }
    }

    [Collection("Remote Injection Tests")]
    public class RemoteInjectionTestComplexParameter
    {
        [Theory]
        [InlineData("System32")]
        [InlineData("SysWOW64")]
        private void TestRemotePluginComplexParameter(string applicationFolder)
        {
            const string testHookLibrary = "CoreHook.Tests.ComplexParameterTest.dll";
            const string testMessageParameter = "Berner";

            var complexParameter = new ComplexParameter
            {
                Message = testMessageParameter,
                HostProcessId = Process.GetCurrentProcess().Id
            };

            var testProcess = Resources.StartProcess(Path.Combine(
                            Environment.ExpandEnvironmentVariables("%Windir%"),
                            applicationFolder,
                            "notepad.exe"
                        ));

            System.Threading.Thread.Sleep(500);

            Resources.InjectDllIntoTarget(testProcess,
               Resources.GetTestDllPath(
               testHookLibrary
               ),
               Resources.GetUniquePipeName(),
               complexParameter);

            Assert.Equal(complexParameter.Message, Resources.ReadFromProcess(testProcess));
            Assert.Equal(complexParameter.HostProcessId.ToString(), Resources.ReadFromProcess(testProcess));

            Resources.EndProcess(testProcess);
        }
    }
}
