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
    /// <summary>
    /// Creates a pipe server and allows custom handling of messages from clients.
    /// </summary>
    public class NamedPipeServer : INamedPipe
    {
        /// <inheritdoc />
        public IConnection Connection { get; private set; }
        /// <inheritdoc />
        public IMessageHandler MessageHandler { get; private set; }

        private const int MaxPipeNameLength = 250;
     
        private readonly Action<ITransportChannel> _handleTransportConnection;
        private bool _connectionStopped;

        private readonly string _pipeName;
        private readonly IPipePlatform _platform;
        private NamedPipeServerStream _pipe;

        /// <summary>
        /// Initialize a new instance of the <see cref="NamedPipeServer"/> class.
        /// </summary>
        /// <param name="pipeName">The name of the pipe server.</param>
        /// <param name="platform">Method for initializing a new pipe-based server.</param>
        /// <param name="handleTransportConnection">Event handler called when receiving a new connection.</param>
        private NamedPipeServer(string pipeName, IPipePlatform platform, Action<ITransportChannel> handleTransportConnection)
        {
            _pipeName = pipeName;
            _platform = platform;
            _handleTransportConnection = handleTransportConnection;
            _connectionStopped = false;
        }

        /// <summary>
        /// Initialize a new pipe server.
        /// </summary>
        /// <param name="pipeName">The name of the pipe server.</param>
        /// <param name="platform">Method for initializing a new pipe-based server.</param>
        /// <param name="handleRequest">Event handler called when receiving a new message from a client.</param>
        /// <returns>An instance of the new pipe server.</returns>
        public static INamedPipe StartNewServer(string pipeName, IPipePlatform platform, Action<IStringMessage, ITransportChannel> handleRequest)
        {
            return CreateNewServer(pipeName, platform, connection => HandleTransportConnection(connection, handleRequest));
        }

        /// <summary>
        /// Initialize a new pipe server.
        /// </summary>
        /// <param name="pipeName">The name of the pipe server.</param>
        /// <param name="platform">Method for initializing a new pipe-based server.</param>
        /// <param name="handleRequest">Event handler called when receiving a new connection.</param>
        /// <returns>An instance of the new pipe server.</returns>
        public static INamedPipe StartNewServer(string pipeName, IPipePlatform platform, Action<ITransportChannel> handleRequest)
        {
            return CreateNewServer(pipeName, platform, handleRequest);
        }

        /// <summary>
        /// Initialize a new pipe server.
        /// </summary>
        /// <param name="pipeName">The name of the pipe server.</param>
        /// <param name="platform">Method for initializing a new pipe-based server.</param>
        /// <param name="handleRequest">Event handler called when receiving a new connection.</param>
        /// <returns>An instance of the new pipe server.</returns>
        private static NamedPipeServer CreateNewServer(string pipeName, IPipePlatform platform, Action<ITransportChannel> handleRequest)
        {
            if (pipeName.Length > MaxPipeNameLength)
            {
                throw new PipeMessageLengthException(pipeName, MaxPipeNameLength);
            }
            var pipeServer = new NamedPipeServer(pipeName, platform, handleRequest);
            pipeServer.Connect();
            return pipeServer;
        }

        private static void HandleTransportConnection(ITransportChannel channel, Action<IStringMessage, ITransportChannel> handleRequest)
        {
            var connection = channel.Connection;

            while (connection.IsConnected)
            {
                var message = channel.MessageHandler.Read();
                if (message == null ||
                    (message.Header == null && message.Body == null) ||
                    !connection.IsConnected)
                {
                    break;
                }
                handleRequest(message, channel);
            }
        }

        /// <inheritdoc />
        public bool Connect()
        {
            try
            {
                if (_pipe != null)
                {
                    throw new InvalidOperationException("Pipe server already started");
                }

                _pipe = _platform.CreatePipeByName(_pipeName);
                _pipe.BeginWaitForConnection(OnConnection, _pipe);

                return true;
            }
            catch (Exception e)
            {
                Log("Unhandled exception during server start", e);
            }

            return false;
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
                        Log($"Pipe {_pipeName} broken with: ", e);
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
                    Log("Unhandled exception during connection initialization", e);
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
                        Log("Unhandled exception during message transport initialization", e);
                    }
                }
            }
            finally
            {
                pipe.Dispose();
            }
        }

        private static void Log(string message, Exception e)
        {
            Console.WriteLine(message);
            Console.WriteLine(e);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _connectionStopped = true;

            NamedPipeServerStream pipe = Interlocked.Exchange(ref _pipe, null);
            pipe?.Dispose();
        }
    }
}
