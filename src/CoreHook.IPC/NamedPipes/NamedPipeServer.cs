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

        private readonly Action<Connection> _handleConnection;
        private readonly string _pipeName;
        private readonly IPipePlatform _platform;

        private bool _isStopping;
        private NamedPipeServerStream _listeningPipe;

        private NamedPipeServer(string pipeName, IPipePlatform platform, Action<IConnection> handleConnection)
        {
            _pipeName = pipeName;
            _platform = platform;
            _handleConnection = handleConnection;
            _isStopping = false;
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
            var pipeServer = new NamedPipeServer(pipeName, platform, handleConnection);
            pipeServer.OpenListeningPipe();
            return pipeServer;
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

        public void OpenListeningPipe()
        {
            try
            {
                if (_listeningPipe != null)
                {
                    throw new InvalidOperationException("There is already a pipe listening for a connection");
                }
                _listeningPipe = _platform.CreatePipeByName(_pipeName);
                _listeningPipe.BeginWaitForConnection(OnNewConnection, _listeningPipe);
            }
            catch (Exception e)
            {
                LogError("OpenListeningPipe caught unhandled exception", e);
            }
        }

        private void OnNewConnection(IAsyncResult ar)
        {
            OnNewConnection(ar, true);
        }

        private void OnNewConnection(IAsyncResult ar, bool createNewThreadIfSynchronous)
        {
            if (createNewThreadIfSynchronous &&
               ar.CompletedSynchronously)
            {
                // If this callback got called synchronously, we must not do any blocking IO on this thread
                // or we will block the original caller. Moving to a new thread so that it will be safe
                // to call a blocking Read on the NamedPipeServerStream
                new Thread(() => OnNewConnection(ar, false)).Start();
                return;
            }
            _listeningPipe = null;
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
                    if (!_isStopping)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    LogError("OnNewConnection caught unhandled exception", e);
                }
                if (!_isStopping)
                {
                    new Thread(OpenListeningPipe).Start();
                    if (!connectionBroken)
                    {
                        try
                        {
                            _handleConnection(new Connection(pipe, () => _isStopping));
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
        public void Dispose()
        {
            _isStopping = true;
            NamedPipeServerStream pipe = Interlocked.Exchange(ref _listeningPipe, null);
            pipe?.Dispose();
        }
    }
}
