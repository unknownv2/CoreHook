using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using CoreHook.IPC.Platform;

namespace CoreHook.IPC.NamedPipes
{
    public class NamedPipeServer : IDisposable
    {
        private const int MaxPipeNameLength = 250;
        private bool isStopping;
        private string pipeName;
        private Action<Connection> handleConnection;
        private IPipePlatform platform;
        private NamedPipeServerStream listeningPipe;

        private NamedPipeServer(string pipeName, IPipePlatform platform, Action<Connection> handleConnection)
        {
            this.pipeName = pipeName;
            this.platform = platform;
            this.handleConnection = handleConnection;
            this.isStopping = false;
        }

        public static NamedPipeServer StartNewServer(string pipeName, IPipePlatform platform, Action<string, Connection> handleRequest)
        {
            if (pipeName.Length > MaxPipeNameLength)
            {
                throw new PipeMessageLengthException(pipeName, MaxPipeNameLength);
            }
            NamedPipeServer pipeServer = new NamedPipeServer(pipeName, platform, connection => HandleConnection(connection, handleRequest));
            pipeServer.OpenListeningPipe();
            return pipeServer;
        }

        public void Dispose()
        {
            this.isStopping = true;
            NamedPipeServerStream pipe = Interlocked.Exchange(ref this.listeningPipe, null);
            if (pipe != null)
            {
                pipe.Dispose();
            }
        }

        private static void HandleConnection(Connection connection, Action<string, Connection> handleRequest)
        {
            while (connection.IsConnected)
            {
                string request = connection.ReadRequest();
                if (request == null ||
                    !connection.IsConnected)
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
            this.OnNewConnection(ar, createNewThreadIfSynchronous: true);
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
                            handleConnection(new Connection(pipe, () => this.isStopping));
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

        public class Connection
        {
            private NamedPipeServerStream serverStream;
            private StreamReader reader;
            private StreamWriter writer;
            private Func<bool> isStopping;

            public Connection(NamedPipeServerStream serverStream, Func<bool> isStopping)
            {
                this.serverStream = serverStream;
                this.isStopping = isStopping;
                reader = new StreamReader(this.serverStream);
                writer = new StreamWriter(this.serverStream);
            }

            public bool IsConnected
            {
                get { return !isStopping() && serverStream.IsConnected; }
            }

            public NamedPipeMessages.Message ReadMessage()
            {
                return NamedPipeMessages.Message.FromString(ReadRequest());
            }

            public string ReadRequest()
            {
                try
                {
                    return reader.ReadLine();
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
                    writer.WriteLine(message);
                    writer.Flush();
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
