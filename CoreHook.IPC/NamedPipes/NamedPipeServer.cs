﻿using System;
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
                throw new PipeNameLengthException(string.Format("The pipe name ({0}) exceeds the max length allowed({1})", pipeName, MaxPipeNameLength));
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
                if (this.listeningPipe != null)
                {
                    throw new InvalidOperationException("There is already a pipe listening for a connection");
                }
                this.listeningPipe = this.platform.CreatePipeByName(this.pipeName);
                this.listeningPipe.BeginWaitForConnection(this.OnNewConnection, this.listeningPipe);
            }
            catch (Exception e)
            {
                this.LogErrorAndExit("OpenListeningPipe caught unhandled exception, exiting process", e);
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
                new Thread(() => this.OnNewConnection(ar, createNewThreadIfSynchronous: false)).Start();
                return;
            }
            this.listeningPipe = null;
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
                    if (!this.isStopping)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    this.LogErrorAndExit("OnNewConnection caught unhandled exception, exiting process", e);
                }
                if (!this.isStopping)
                {
                    this.OpenListeningPipe();
                    if (!connectionBroken)
                    {
                        try
                        {
                            this.handleConnection(new Connection(pipe, () => this.isStopping));
                        }
                        catch (Exception e)
                        {
                            this.LogErrorAndExit("Unhandled exception in connection handler", e);
                        }
                    }
                }
            }
            finally
            {
                pipe.Dispose();
            }
        }

        private void LogErrorAndExit(string message, Exception e)
        {
   
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
                this.reader = new StreamReader(this.serverStream);
                this.writer = new StreamWriter(this.serverStream);
            }
            public bool IsConnected
            {
                get { return !this.isStopping() && this.serverStream.IsConnected; }
            }
            public NamedPipeMessages.Message ReadMessage()
            {
                return NamedPipeMessages.Message.FromString(this.ReadRequest());
            }
            public string ReadRequest()
            {
                try
                {
                    return this.reader.ReadLine();
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
                    this.writer.WriteLine(message);
                    this.writer.Flush();
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
            }
            public bool TrySendResponse(NamedPipeMessages.Message message)
            {
                return this.TrySendResponse(message.ToString());
            }
        }
    }
}