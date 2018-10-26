using Xunit;

namespace CoreHook.Tests
{
    public class RemoteInjectionTest64
    {
        [Fact]
        private void TestRemoteInject64()
        {
            const string TestHookLibrary = "CoreHook.Tests.SimpleHook1.dll";
            const string TestMessage = "Berner";
            var testProcess = Resources.TestProcess;

            Resources.InjectDllIntoTarget(testProcess,
               Resources.GetTestDllPath(
               TestHookLibrary
               ),
               TestMessage,
               "CoreHookInjection64");

            Assert.Equal(TestMessage, Resources.ReadFromProcess(testProcess));

            Resources.EndTestProcess();
        }

        //[Fact]
        private void TestTargetAppRemoteInject()
        {
            const string TestHookLibrary = "CoreHook.Tests.SimpleHook1.dll";
            const string TestMessage = "Berner";

            Resources.InjectDllIntoTarget(Resources.TargetProcess,
               Resources.GetTestDllPath(
               TestHookLibrary
               ),
               TestMessage);

            Assert.Equal(TestMessage, Resources.ReadFromProcess(Resources.TargetProcess));

            Resources.EndTargetAppProcess();
        }
    }

    public class RemoteInjectionTest32
    {
        [Fact]
        private void TestRemoteInject32()
        {
            const string TestHookLibrary = "CoreHook.Tests.SimpleHook1.dll";
            const string TestMessage = "Berner";
            var testProcess = Resources.TestProcess2;

            Resources.InjectDllIntoTarget(testProcess,
               Resources.GetTestDllPath(
               TestHookLibrary
               ),
               TestMessage,
               "CoreHookInjection32");

            Assert.Equal(TestMessage, Resources.ReadFromProcess(testProcess));

            Resources.EndTestProcess2();
        }
    }
}
