using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using CoreHook.IPC.NamedPipes;
using CoreHook.BinaryInjection.RemoteInjection;

namespace CoreHook.Tests
{
    public class InjectionHelperTest
    {
        private readonly int _targetProcessId = Process.GetCurrentProcess().Id;

        [Fact]
        public void InjectionHelperCompleted()
        {
            var injectionComplete = false;
            var InjectionHelperPipeName = "InjectionHelperPipeTest";

            InjectionHelper.BeginInjection(_targetProcessId);
            using (var pipeServer = InjectionHelper.CreateServer(InjectionHelperPipeName, new PipePlatformBase()))
            {
                try
                {
                    Task.Run(() => SendInjectionComplete(InjectionHelperPipeName, _targetProcessId));

                    InjectionHelper.WaitForInjection(_targetProcessId);
                }
                finally
                {
                    InjectionHelper.InjectionCompleted(_targetProcessId);

                    injectionComplete = true;
                }
            }
            Assert.True(injectionComplete);
        }

        [Fact]
        public void InjectionHelperDidNotComplete()
        {
            var InjectionHelperPipeName = "InjectionHelperFailedPipeTest";

            InjectionHelper.BeginInjection(_targetProcessId);
            using (var pipeServer = InjectionHelper.CreateServer(InjectionHelperPipeName, new PipePlatformBase()))
            {
                try
                {
                    Assert.Throws<TimeoutException>(() => InjectionHelper.WaitForInjection(_targetProcessId, 500));
                }
                finally
                {
                    InjectionHelper.InjectionCompleted(_targetProcessId);
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
