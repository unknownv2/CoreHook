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
    public class InjectionHelperTests
    {
        private const string InjectionHelperPipeName = "InjectionHelperPipeTest";

        private int TargetProcessId = Process.GetCurrentProcess().Id;

        [Fact]
        public void InjectionHelperCompleted()
        {
            bool injectionComplete = false;

            InjectionHelper.BeginInjection(TargetProcessId);
            using (var pipeServer = InjectionHelper.CreateServer(InjectionHelperPipeName, new PipePlatform()))
            {
                try
                {
                    new Thread(delegate () {
                        SendInjectionComplete(InjectionHelperPipeName, TargetProcessId);
                    }).Start();

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

    public class PipePlatform : IPipePlatform
    {
        public NamedPipeServerStream CreatePipeByName(string pipeName)
        {
            return new NamedPipeServerStream(
             pipeName,
             PipeDirection.InOut,
             NamedPipeServerStream.MaxAllowedServerInstances,
             PipeTransmissionMode.Byte,
             PipeOptions.Asynchronous,
             65536,
             65536
             );
        }
    }
}
