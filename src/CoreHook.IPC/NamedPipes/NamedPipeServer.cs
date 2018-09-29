using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using CoreHook.IPC.Platform;

namespace CoreHook.IPC.NamedPipes
{
    public class NamedPipeServer : INamedPipeServer
    {
        private const int MaxPipeNameLength = 250;
        private bool isStopping;
        private string pipeName;
        private Action<Connection> handleConnection;
        private IPipePlatform platform;
        private NamedPipeServerStream listeningPipe;

        private NamedPipeServer(string pipeName, IPipePlatform platform, Action<IConnection> handleConnection)
        {
            this.pipeName = pipeName;
            this.platform = platform;
            this.handleConnection = handleConnection;
            this.isStopping = false;
        }

        public static INamedPipeServer StartNewServer(string pipeName, IPipePlatform platform, Action<string, IConnection> handleRequest)
        {
            if (pipeName.Length > MaxPipeNameLength)
            {
                throw new PipeMessageLengthException(pipeName, MaxPipeNameLength);
            }
            var pipeServer = new NamedPipeServer(pipeName, platform, connection => HandleConnection(connection, handleRequest));
            pipeServer.OpenListeningPipe();
            return pipeServer;
        }

        public static INamedPipeServer StartNewServer(string pipeName, IPipePlatform platform, Action<IConnection> handleConnection)
        {
            if (pipeName.Length > MaxPipeNameLength)
            {
                throw new PipeMessageLengthException(pipeName, MaxPipeNameLength);
            }
            var pipeServer = new NamedPipeServer(pipeName, platform, connection => handleConnection(connection));
            pipeServer.OpenListeningPipe();
            return pipeServer;
        }

        public void Dispose()
        {
            isStopping = true;
            NamedPipeServerStream pipe = Interlocked.Exchange(ref listeningPipe, null);
            if (pipe != null)
            {
                pipe.Dispose();
            }
        }

        private static void HandleConnection(IConnection connection, Action<string, IConnection> handleRequest)
        {
            while (connection.IsConnected)
            {
                string request = connection.ReadRequest();
                if (request == null || !connection.IsConnected)
                {
                    break;
                }
                handleRequest(request, connection);
            }
        }

        private void OpenListeningPipe()
        {
            try
            {
                if (listeningPipe != null)
                {
                    throw new InvalidOperationException("There is already a pipe listening for a connection");
                }
                listeningPipe = platform.CreatePipeByName(pipeName);
                listeningPipe.BeginWaitForConnection(OnNewConnection, listeningPipe);
            }
            catch (Exception e)
            {
                LogError("OpenListeningPipe caught unhandled exception", e);
            }
        }

        private void OnNewConnection(IAsyncResult ar)
        {
            OnNewConnection(ar, createNewThreadIfSynchronous: true);
        }

        private void OnNewConnection(IAsyncResult ar, bool createNewThreadIfSynchronous)
        {
            if (createNewThreadIfSynchronous &&
               ar.CompletedSynchronously)
            {
                // if this callback got called synchronously, we must not do any blocking IO on this thread
                // or we will block the original caller. Moving to a new thread so that it will be safe
                // to call a blocking Read on the NamedPipeServerStream
                new Thread(() => OnNewConnection(ar, createNewThreadIfSynchronous: false)).Start();
                return;
            }
            listeningPipe = null;
            bool connectionBroken = false;
            NamedPipeServerStream pipe = (NamedPipeServerStream)ar.AsyncState;
            try
            {
                try
                {
                    pipe.EndWaitForConnection(ar);
                }
                catch (IOException e)
                {
                    connectionBroken = true;
                }
                catch (ObjectDisposedException)
                {
                    if (!isStopping)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    LogError("OnNewConnection caught unhandled exception", e);
                }
                if (!isStopping)
                {
                    new Thread(() => OpenListeningPipe()).Start();
                    if (!connectionBroken)
                    {
                        try
                        {
                            handleConnection(new Connection(pipe, () => isStopping));
                        }
                        catch (Exception e)
                        {
                            LogError("Unhandled exception in connection handler", e);
                        }
                    }
                }
            }
            finally
            {
                pipe.Dispose();
            }
        }

        private void LogError(string message, Exception e)
        {
            Console.WriteLine(message);
            Console.WriteLine(e);
        }

        public class Connection : IConnection
        {
            public NamedPipeServerStream ServerStream { get; }

            private readonly StreamReader _reader;
            private readonly StreamWriter _writer;
            private readonly Func<bool> _isStopping;

            public Connection(NamedPipeServerStream serverStream, Func<bool> isStopping)
            {
                ServerStream = serverStream;

                _isStopping = isStopping;
                _reader = new StreamReader(ServerStream);
                _writer = new StreamWriter(ServerStream);
            }

            public bool IsConnected
            {
                get { return !_isStopping() && ServerStream.IsConnected; }
            }

            public NamedPipeMessages.IMessage ReadMessage()
            {
                return NamedPipeMessages.Message.FromString(ReadRequest());
            }

            public string ReadRequest()
            {
                try
                {
                    return _reader.ReadLine();
                }
                catch (IOException)
                {
                    return null;
                }
            }

            public bool TrySendResponse(string message)
            {
                try
                {
                    _writer.WriteLine(message);
                    _writer.Flush();

                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
            }

            public bool TrySendResponse(NamedPipeMessages.Message message)
            {
                return TrySendResponse(message.ToString());
            }
        }
    }
}
