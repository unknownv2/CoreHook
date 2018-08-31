using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using CoreHook.FileMonitor.Service.Pipe;
using CoreHook.FileMonitor.Service.Log;
using CoreHook.FileMonitor.Service.Stats;
using CoreHook.IPC.Platform;

namespace CoreHook.FileMonitor.Pipe
{
    public class NpListener
    {
        private bool running;

        private EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private readonly int _maxConnections = 254;

        private readonly ILog _log = new NullLogger();

        private readonly IStats _stats = new NullStats();

        private NamedPipeServerStream _previousStream = null;

        public readonly string _pipeName;

        public event EventHandler<PipeClientConnectionEventArgs> RequestRetrieved;

        private readonly IPipePlatform _pipePlatform;

        public NpListener(string pipeName, IPipePlatform pipePlatform, int maxConnections = 254, ILog log = null, IStats stats = null)
        {
            _log = log ?? _log;
            _stats = stats ?? _stats;

            if(maxConnections < _maxConnections)
            {
                _maxConnections = maxConnections;
            }

            _pipeName = pipeName;

            _pipePlatform = pipePlatform;
        }

        internal NamedPipeServerStream CreatePipe(string pipeName)
        {
            return _pipePlatform.CreatePipeByName(pipeName);
        }

        public void Start()
        {
            running = true;

            Task.Factory.StartNew(() => ServerLoop(), TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            if (running)
            {
                running = false;

                try
                {
                    using (var client = new NamedPipeClientStream(_pipeName))
                    {
                        client.Connect(50);
                    }
                }
                catch (Exception e)
                {
                    _log.Error($"Stop error: {e.ToString()}");
                }
                terminateHandle.WaitOne();
            }
        }

        private void ServerLoop()
        {
            try
            {
                while (running)
                {
                    ProcessNextClient();
                }
            }
            catch (Exception e)
            {
                _log.Fatal($"ServerLoop fatal error: {e.ToString()}");
            }
            finally
            {
                terminateHandle.Set();
            }
        }

        private void ProcessClientThread(NamedPipeServerStream pipeStream)
        {
            try
            {
                RequestRetrieved?.Invoke(this, new PipeClientConnectionEventArgs(pipeStream));
            }
            catch (Exception e)
            {
                _log.Error($"ProcessClientThread error: {e.ToString()}");
            }
            finally
            {
                if (pipeStream.IsConnected) pipeStream.Close();

                pipeStream.Dispose();
            }
        }

        public void ProcessNextClient()
        {
            try
            {
                if (_previousStream != null)
                {
                    while (_previousStream.IsConnected)
                    {
                        Thread.Sleep(500);
                    }
                }

                var pipeStream = CreatePipe(_pipeName);
                
                try
                {
                    pipeStream.WaitForConnection();
                }
                catch
                {
                    pipeStream.Disconnect();
                }

                Console.WriteLine($"Connection received from pipe {_pipeName}");

                _previousStream = pipeStream;

                Task.Factory.StartNew(() => ProcessClientThread(pipeStream));
            }
            catch (Exception e)
            {
                //If there are no more avail connections (254 is in use already) then just keep looping until one is avail
                _log.Error($"ProcessNextClient error: {e.ToString()}");
            }
        }
    }
}
