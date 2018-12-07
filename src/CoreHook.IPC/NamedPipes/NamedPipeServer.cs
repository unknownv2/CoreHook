using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using CoreHook.IPC.Handlers;
using CoreHook.IPC.Messages;
using CoreHook.IPC.Platform;
using CoreHook.IPC.Transport;

namespace CoreHook.IPC.NamedPipes
{
    public class NamedPipeServer : INamedPipeServer
    {
        public IConnection Connection { get; private set; }
        public IMessageHandler MessageHandler { get; private set; }

        private const int MaxPipeNameLength = 250;
     
        private readonly Action<ITransportChannel> _handleTransportConnection;
        private bool _connectionStopped;

        private readonly string _pipeName;
        private readonly IPipePlatform _platform;
        private NamedPipeServerStream _pipe;

        private NamedPipeServer(string pipeName, IPipePlatform platform, Action<ITransportChannel> handleTransportConnection)
        {
            _pipeName = pipeName;
            _platform = platform;
            _handleTransportConnection = handleTransportConnection;
            _connectionStopped = false;
        }

        public static INamedPipeServer StartNewServer(string pipeName, IPipePlatform platform, Action<IMessage, ITransportChannel> handleRequest)
        {
            if (pipeName.Length > MaxPipeNameLength)
            {
                throw new PipeMessageLengthException(pipeName, MaxPipeNameLength);
            }

            var pipeServer = new NamedPipeServer(pipeName, platform, connection => HandleTransportConnection(connection, handleRequest));
            pipeServer.Start();
            return pipeServer;
        }

        public static INamedPipeServer StartNewServer(string pipeName, IPipePlatform platform, Action<ITransportChannel> handleRequest)
        {
            if (pipeName.Length > MaxPipeNameLength)
            {
                throw new PipeMessageLengthException(pipeName, MaxPipeNameLength);
            }

            var pipeServer = new NamedPipeServer(pipeName, platform, handleRequest);
            pipeServer.Start();
            return pipeServer;
        }

        private static void HandleTransportConnection(ITransportChannel channel, Action<IMessage, ITransportChannel> handleRequest)
        {
            var connection = channel.Connection;

            while (connection.IsConnected)
            {
                var message = channel.MessageHandler.Read();
                if (message == null || !connection.IsConnected)
                {
                    break;
                }
                handleRequest(message, channel);
            }
        }

        public void Start()
        {
            try
            {
                if (_pipe != null)
                {
                    throw new InvalidOperationException("Pipe server already started");
                }

                _pipe = _platform.CreatePipeByName(_pipeName);
                _pipe.BeginWaitForConnection(OnConnection, _pipe);
            }
            catch (Exception e)
            {
                LogError("Unhandled exception during server start", e);
            }
        }

        private void OnConnection(IAsyncResult ar)
        {
            // Check if we should be accepting any new connections.
            if (_connectionStopped)
            {
                return;
            }

            _pipe = null;
            var connectionBroken = false;
            if (!(ar.AsyncState is NamedPipeServerStream pipe))
            {
                return;
            }

            try
            {
                try
                {
                    pipe.EndWaitForConnection(ar);
                }
                catch (IOException e)
                {
                    connectionBroken = true;
                    if (!_connectionStopped)
                    {
                        LogError($"Pipe {_pipeName} broken with: ", e);
                    }
                }
                catch (ObjectDisposedException)
                {
                    if (!_connectionStopped)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    LogError("Unhandled exception during connection initialization", e);
                }
                if (!_connectionStopped && !connectionBroken)
                {
                    try
                    {
                        Connection = new PipeConnection(pipe, () => _connectionStopped);
                        MessageHandler = new MessageHandler(Connection);

                        _handleTransportConnection?.Invoke(this);
                    }
                    catch (Exception e)
                    {
                        LogError("Unhandled exception during message transport initialization", e);
                    }
                }
            }
            finally
            {
                pipe.Dispose();
            }
        }

        private static void LogError(string message, Exception e)
        {
            Console.WriteLine(message);
            Console.WriteLine(e);
        }

        public void Dispose()
        {
            _connectionStopped = true;

            NamedPipeServerStream pipe = Interlocked.Exchange(ref _pipe, null);
            pipe?.Dispose();
        }
    }
}
