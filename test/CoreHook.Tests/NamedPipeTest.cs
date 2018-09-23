using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CoreHook.Tests
{
    public class NamedPipeTest
    {
        private const string NamedPipeName = "NamedPipeNameTest";

        [Fact]
        private void ShouldConnectToServer()
        {
            bool receivedMessage = false;
            const string testMessage = "TestMessage";

            using (var pipeServer = CreateServer(NamedPipeName, new PipePlatformBase(),
                (string request, NamedPipeServer.Connection connection) =>
                {
                    if (request == testMessage)
                    {
                        receivedMessage = true;
                    }
                    connection.TrySendResponse(request);
                }))
            {
                using (INamedPipeClient pipeClient = new NamedPipeClient(NamedPipeName))
                {
                    if(SendPipeMessage(pipeClient, testMessage))
                    {
                        Assert.Equal(pipeClient.ReadRawResponse(), testMessage);
                    }
                }
            }
            Assert.True(receivedMessage);
        }

        private static INamedPipeServer CreateServer(string namedPipeName, IPipePlatform pipePlatform, Action<string, NamedPipeServer.Connection> handleRequest)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, handleRequest);
        }

        private static bool SendPipeMessage(INamedPipeClient pipeClient, string message)
        {
            if (pipeClient.Connect(3000))
            {
                pipeClient.SendRequest(message);
                return true;
            }
            return false;
        }
    }
}
