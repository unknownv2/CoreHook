using CoreHook.BinaryInjection;
using CoreHook.BinaryInjection.IPC;
using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

using System;
using System.Threading.Tasks;

using Xunit;

namespace CoreHook.Tests;

public class InjectionHelperTest
{
    private readonly int _targetProcessId = Environment.ProcessId;

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
        using var pipeClient = CreateClient(pipeName);

        try
        {
            pipeClient.Connect();
            return SendPipeMessage(pipeClient, new InjectionCompleteMessage(pid, true));
        }
        catch
        {
            return false;
        }
    }

    private static INamedPipe CreateClient(string pipeName)
    {
        return new NamedPipeClient(pipeName);
    }

    private static IPipePlatform GetPipePlatform()
    {
        return new PipePlatformBase();
    }

    private static bool SendPipeMessage(INamedPipe pipe, CustomMessage message)
    {
        return pipe.TryWrite(message).Result;
    }
}
