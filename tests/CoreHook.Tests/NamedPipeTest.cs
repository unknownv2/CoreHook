using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

using System;

using Xunit;

namespace CoreHook.Tests;

public class NamedPipeTest
{
    [Fact]
    private void ShouldConnectToServer()
    {
        string namedPipe = Resources.GetUniquePipeName();
        const string testMessage = "TestMessage";
        bool receivedMessage = false;

        using (CreateServer(namedPipe, GetPipePlatform(), (request) =>
            {
                receivedMessage = true;
                SendPipeMessage(request, "RandomResponse");
            }))
        using (var pipeClient = CreateClient(namedPipe))
        {
            if (SendPipeMessage(pipeClient, testMessage))
            {
                ReadMessage(pipeClient);
            }
        }
        Assert.True(receivedMessage);
    }

    [Fact]
    private void ShouldConnectToServerAndReceiveResponse()
    {
        string namedPipe = Resources.GetUniquePipeName();
        const string testMessage = "TestMessage";

        using (CreateServer(namedPipe, GetPipePlatform(),
            async (channel) =>
            {
                Assert.Equal((await channel.Read()).ToString(), testMessage);
                channel.TryWrite(new StringMessage(testMessage));
            }))
        using (var pipeClient = CreateClient(namedPipe))
        {
            if (SendPipeMessage(pipeClient, testMessage))
            {
                Assert.Equal(ReadMessageToString(pipeClient), testMessage);
            }
        }
    }

    [Fact]
    private void ShouldConnectToServerAndReceiveMultipleResponses()
    {
        string namedPipe = Resources.GetUniquePipeName();
        const string testMessage1 = "TestMessage1";
        const string testMessage2 = "TestMessage2";
        const string testMessage3 = "TestMessage3";

        using var server = new NamedPipeServer(namedPipe, GetPipePlatform(), async pipe => SendPipeMessage(server, await pipe.Read());
        //(CreateServer(namedPipe, GetPipePlatform(), (pipe) => SendPipeMessage(namedPipe, await pipe.Read()))
        using (var pipeClient = CreateClient(namedPipe))
        {
            pipeClient.Connect();

            Assert.True(SendPipeMessage(pipeClient, testMessage1));
            Assert.True(SendPipeMessage(pipeClient, testMessage2));
            Assert.True(SendPipeMessage(pipeClient, testMessage3));

            Assert.Equal(ReadMessageToString(pipeClient), testMessage1);
            Assert.Equal(ReadMessageToString(pipeClient), testMessage2);
            Assert.Equal(ReadMessageToString(pipeClient), testMessage3);

        }
    }

    [Fact]
    private void ShouldConnectToServerAndReceiveRandomResponse()
    {
        string namedPipe = Resources.GetUniquePipeName();
        const string testMessage = "TestMessage";

        using (CreateServer(namedPipe, GetPipePlatform(),
            (message) =>
            {
                Assert.Equal(message.ToString(), testMessage);
                SendPipeMessage(message, "RandomResponse");
            }))
        using (var pipeClient = CreateClient(namedPipe))
        {
            if (SendPipeMessage(pipeClient, testMessage))
            {
                Assert.NotEqual(ReadMessageToString(pipeClient), testMessage);
            }
        }
    }

    private static IPipePlatform GetPipePlatform()
    {
        return new PipePlatformBase();
    }

    private static INamedPipe CreateClient(string pipeName)
    {
        return new NamedPipeClient(pipeName);
    }

    private static INamedPipe CreateServer(string namedPipeName, IPipePlatform pipePlatform, Action<CustomMessage> handleRequest)
    {
        return new NamedPipeServer(namedPipeName, pipePlatform, handleRequest);
    }

    private static INamedPipe CreateServer(string namedPipeName, IPipePlatform pipePlatform, Action<INamedPipe> handleConnection)
    {
        return new NamedPipeServer(namedPipeName, pipePlatform, handleConnection);
    }

    private static bool SendPipeMessage(INamedPipe pipeClient, string message)
    {
        try
        {
            pipeClient.Connect();
            return pipeClient.TryWrite(new StringMessage("", message)).Result;
        }
        catch
        {
            return false;
        }
    }

    private static bool SendPipeMessage2(INamedPipe pipe, string message)
    {
        return SendPipeMessage(pipe, new StringMessage("", message));
    }

    private static bool SendPipeMessage(INamedPipe pipe, CustomMessage message)
    {
        return pipe.TryWrite(message).Result;
    }

    private static string ReadMessageToString(INamedPipe pipe)
    {
        return pipe.Read().ToString();
    }

    private static CustomMessage ReadMessage(INamedPipe pipe)
    {
        return pipe.Read().Result;
    }

}
