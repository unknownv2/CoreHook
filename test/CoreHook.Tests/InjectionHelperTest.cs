using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Xunit;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;
using CoreHook.ManagedHook.Remote;

namespace CoreHook.Tests
{
    public class InjectionHelperTest
    {
        private const string InjectionHelperPipeName = "InjectionHelperPipeTest";

        private int TargetProcessId = Process.GetCurrentProcess().Id;

        [Fact]
        public void InjectionHelperCompleted()
        {
            bool injectionComplete = false;

            InjectionHelper.BeginInjection(TargetProcessId);
            using (var pipeServer = InjectionHelper.CreateServer(InjectionHelperPipeName, new PipePlatformBase()))
            {
                try
                {
                    new Thread(() => SendInjectionComplete(InjectionHelperPipeName, TargetProcessId)).Start();

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
