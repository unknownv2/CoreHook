using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using CoreHook.BinaryInjection.RemoteInjection;
using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC;
using CoreHook.IPC.Platform;

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
            using (InjectionHelper.CreateServer(InjectionHelperPipeName, GetPipePlatform()))
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
            using (InjectionHelper.CreateServer(InjectionHelperPipeName, GetPipePlatform()))
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
            using (INamedPipeClient pipeClient = new NamedPipeClient(pipeName))
            {
                if (pipeClient.Connect(2000))
                {
                    return SendPipeMessage(pipeClient.MessageHandler,
                        InjectionCompleteNotification.CreateMessage(pid, true));
                }
            }
            return false;
        }

        private static IPipePlatform GetPipePlatform()
        {
            return new PipePlatformBase();
        }

        private static bool SendPipeMessage(IMessageWriter writer, IMessage message)
        {
            return writer.TryWrite(message);
        }
    }
}
