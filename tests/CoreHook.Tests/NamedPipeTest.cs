using System;
using Xunit;
using CoreHook.IPC;
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
                (IMessage request, ITransportChannel channel) =>
                {
                    receivedMessage = true;
                    SendPipeMessage(channel.MessageHandler, "RandomResponse");
                }))
            using (INamedPipeClient pipeClient = CreateClient(namedPipe))
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

            using (var server = CreateServer(namedPipe, GetPipePlatform(),
                (IMessage message, ITransportChannel channel) =>
                {
                    var receivedMessage = message.ToString();
                    Assert.Equal(receivedMessage, testMessage);
                    channel.MessageHandler.Write(Message.FromString(testMessage));
                }))
            using (INamedPipeClient pipeClient = CreateClient(namedPipe))
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
            using (INamedPipeClient pipeClient = CreateClient(namedPipe))
            {
                if (pipeClient.Connect(3000))
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
                    var receivedMessage = message.ToString();
                    Assert.Equal(receivedMessage, testMessage);
                    SendPipeMessage(channel.MessageHandler, "RandomResponse");
                }))
            using (INamedPipeClient pipeClient = CreateClient(namedPipe))
            {
                if (SendPipeMessage(pipeClient, testMessage))
                {
                    Assert.NotEqual(ReadMessageToString(pipeClient), testMessage);
                }
            }
        }

        [Fact]
        private void ShouldNotConnectToServer()
        {
            string clientNamedPipe = Resources.GetUniquePipeName();
            bool connected = false;
   
            using (INamedPipeClient pipeClient = CreateClient(clientNamedPipe))
            {
                if(pipeClient.Connect(1000))
                {
                    connected = true;
                }
            }
            
            Assert.False(connected);
        }

        private static IPipePlatform GetPipePlatform()
        {
            return new PipePlatformBase();
        }

        private static INamedPipeClient CreateClient(string pipeName)
        {
            return new NamedPipeClient(pipeName);
        }

        private static INamedPipeServer CreateServer(string namedPipeName, IPipePlatform pipePlatform, Action<IMessage, ITransportChannel> handleRequest)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, handleRequest);
        }
        
        private static bool SendPipeMessage(INamedPipeClient pipeClient, string message)
        {
            if (pipeClient.Connect(3000))
            {
                return pipeClient.MessageHandler.TryWrite(Message.FromString(message));
            }
            return false;
        }

        private static bool SendPipeMessage(IMessageHandler messageHandler, string message)
        {
            return SendPipeMessage(messageHandler, Message.FromString(message));
        }

        private static bool SendPipeMessage(IMessageHandler messageHandler, IMessage message)
        {
            return messageHandler.TryWrite(message);
        }

        private static string ReadMessageToString(IMessageHandler messageHandler)
        {
            return messageHandler.Read().ToString();
        }

        private static IMessage ReadMessage(IMessageHandler messageHandler)
        {
            return messageHandler.Read();
        }

        private static IMessage ReadMessage(INamedPipeClient pipeClient)
        {
            return ReadMessage(pipeClient.MessageHandler);
        }

        private static string ReadMessageToString(INamedPipeClient pipeClient)
        {
            return ReadMessageToString(pipeClient.MessageHandler);
        }
    }
}
