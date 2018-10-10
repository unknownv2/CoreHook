using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using CoreHook.IPC.NamedPipes;
using CoreHook.ManagedHook.Remote;

namespace CoreHook.Tests
{
    public class InjectionHelperTest
    {
        private int TargetProcessId = Process.GetCurrentProcess().Id;

        [Fact]
        public void InjectionHelperCompleted()
        {
            var injectionComplete = false;
            var InjectionHelperPipeName = "InjectionHelperPipeTest";

            InjectionHelper.BeginInjection(TargetProcessId);
            using (var pipeServer = InjectionHelper.CreateServer(InjectionHelperPipeName, new PipePlatformBase()))
            {
                try
                {
                    Task.Run(() => SendInjectionComplete(InjectionHelperPipeName, TargetProcessId));

                    InjectionHelper.WaitForInjection(TargetProcessId);
                }
                finally
                {
                    InjectionHelper.InjectionCompleted(TargetProcessId);

                    injectionComplete = true;
                }
            }
            Assert.True(injectionComplete);
        }

        [Fact]
        public void InjectionHelperDidNotComplete()
        {
            var InjectionHelperPipeName = "InjectionHelperFailedPipeTest";

            InjectionHelper.BeginInjection(TargetProcessId);
            using (var pipeServer = InjectionHelper.CreateServer(InjectionHelperPipeName, new PipePlatformBase()))
            {
                try
                {
                    Assert.Throws<TimeoutException>(() => InjectionHelper.WaitForInjection(TargetProcessId, 500));
                }
                finally
                {
                    InjectionHelper.InjectionCompleted(TargetProcessId);
                }
            }
        }

        private static bool SendInjectionComplete(string pipeName, int pid)
        {
            using (NamedPipeClient pipeClient = new NamedPipeClient(pipeName))
            {
                if (pipeClient.Connect())
                {
                    var request = new NamedPipeMessages.InjectionCompleteNotification(pid, true);
                    if (pipeClient.TrySendRequest(request.CreateMessage()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
