using System;
using Xunit;
using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;
using CoreHook.IPC.Transport;
using CoreHook.IPC.Handlers;

namespace CoreHook.Tests
{
    public class NamedPipeTest
    {
        [Fact]
        private void ShouldConnectToServer()
        {
            string namedPipe = Resources.GetUniquePipeName();
            const string testMessage = "TestMessage";
            bool receivedMessage = false;

            using (CreateServer(namedPipe, GetPipePlatform(),
                (request, channel) =>
                {
                    receivedMessage = true;
                    SendPipeMessage(channel.MessageHandler, "RandomResponse");
                }))
            using (var pipeClient = CreateClient(namedPipe))
            {
                if(SendPipeMessage(pipeClient, testMessage))
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
                (message, channel) =>
                {
                    Assert.Equal(message.ToString(), testMessage);
                    channel.MessageHandler.Write(StringMessage.FromString(testMessage));
                }))
            using (var pipeClient = CreateClient(namedPipe))
            {
                if (SendPipeMessage(pipeClient, testMessage))
                {


                    Assert.Equal(ReadMessageToString(pipeClient.MessageHandler), testMessage);
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

            using (CreateServer(namedPipe, GetPipePlatform(),
                  (request, channel) => SendPipeMessage(channel.MessageHandler, request)))
            using (var pipeClient = CreateClient(namedPipe))
            {
                if (pipeClient.Connect())
                {
                    var messageHandler = pipeClient.MessageHandler;
                    Assert.True(SendPipeMessage(messageHandler, testMessage1));
                    Assert.True(SendPipeMessage(messageHandler, testMessage2));
                    Assert.True(SendPipeMessage(messageHandler, testMessage3));

                    Assert.Equal(ReadMessageToString(messageHandler), testMessage1);
                    Assert.Equal(ReadMessageToString(messageHandler), testMessage2);
                    Assert.Equal(ReadMessageToString(messageHandler), testMessage3);
                }
            }
        }

        [Fact]
        private void ShouldConnectToServerAndReceiveRandomResponse()
        {
            string namedPipe = Resources.GetUniquePipeName();
            const string testMessage = "TestMessage";

            using (CreateServer(namedPipe, GetPipePlatform(),
                (message, channel) =>
                {
                    Assert.Equal(message.ToString(), testMessage);
                    SendPipeMessage(channel.MessageHandler, "RandomResponse");
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

        private static INamedPipe CreateServer(string namedPipeName, IPipePlatform pipePlatform, Action<IStringMessage, ITransportChannel> handleRequest)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, handleRequest);
        }
        
        private static bool SendPipeMessage(INamedPipe pipeClient, string message)
        {
            if (pipeClient.Connect())
            {
                return pipeClient.MessageHandler.TryWrite(StringMessage.FromString(message));
            }
            return false;
        }

        private static bool SendPipeMessage(IMessageHandler messageHandler, string message)
        {
            return SendPipeMessage(messageHandler, StringMessage.FromString(message));
        }

        private static bool SendPipeMessage(IMessageHandler messageHandler, IStringMessage message)
        {
            return messageHandler.TryWrite(message);
        }

        private static string ReadMessageToString(IMessageHandler messageHandler)
        {
            return messageHandler.Read().ToString();
        }

        private static IStringMessage ReadMessage(IMessageHandler messageHandler)
        {
            return messageHandler.Read();
        }

        private static IStringMessage ReadMessage(INamedPipe pipeClient)
        {
            return ReadMessage(pipeClient.MessageHandler);
        }

        private static string ReadMessageToString(INamedPipe pipeClient)
        {
            return ReadMessageToString(pipeClient.MessageHandler);
        }
    }
}
