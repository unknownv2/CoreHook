using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using CoreHook.FileMonitor.Service.Pipe;
using CoreHook.FileMonitor.Service.Log;
using CoreHook.FileMonitor.Service.Stats;


namespace CoreHook.UWP.FileMonitor.Pipe
{
    public class NpListener
    {
        private bool running;
        private EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private int _maxConnections = 254;
        private ILog _log = new NullLogger();
        private IStats _stats = new NullStats();

        public string PipeName { get; set; }
        public event EventHandler<PipeClientConnectionEventArgs> RequestRetrieved;

        private PipeSecurity _pipeSecurity = null;

        public NpListener(string pipeName, int maxConnections = 254, ILog log = null, IStats stats = null)
        {
            _log = log ?? _log;
            _stats = stats ?? _stats;

            if (maxConnections > 254)
            {
                maxConnections = 254;
            }
            _maxConnections = maxConnections;
            PipeName = pipeName;
            _pipeSecurity = CreateUWPPipeSecurity();
        }

        private PipeSecurity CreateUWPPipeSecurity()
        {
            const PipeAccessRights access = PipeAccessRights.ReadWrite;

            var sec = new PipeSecurity();

            using (var identity = WindowsIdentity.GetCurrent())
            {
                sec.AddAccessRule(
                    new PipeAccessRule(identity.User, access, AccessControlType.Allow)
                );

                if (identity.User != identity.Owner)
                {
                    sec.AddAccessRule(
                        new PipeAccessRule(identity.Owner, access, AccessControlType.Allow)
                    );
                }
            }
   
            // Allow all app packages to connect.
            sec.AddAccessRule(new PipeAccessRule(new SecurityIdentifier("S-1-15-2-1"), access, AccessControlType.Allow));
            return sec;
        }

        internal NamedPipeServerStream CreatePipe(string pipeName)
        {
            return new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    _maxConnections,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    65536,
                    65536,
                    _pipeSecurity
                    );
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
                    using (var client = new NamedPipeClientStream(PipeName))
                    {
                        client.Connect(50);
                    }
                }
                catch (Exception e)
                {
                    _log.Error("Stop error: {0}", e.ToString());
                }
                terminateHandle.WaitOne();
            }
        }

        private void ServerLoop()
        {
            try
            {
                ProcessNextClient();
                while (running)
                {
                    ProcessNextClient();
                }
            }
            catch (Exception e)
            {
                _log.Fatal("ServerLoop fatal error: {0}", e.ToString());
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
                if (this.RequestRetrieved != null) //has event subscribers
                {
                    var args = new PipeClientConnectionEventArgs(pipeStream);
                    RequestRetrieved(this, args);
                }
            }
            catch (Exception e)
            {
                _log.Error("ProcessClientThread error: {0}", e.ToString());
            }
            finally
            {
                if (pipeStream.IsConnected) pipeStream.Close();

                pipeStream.Dispose();
            }
        }

        private NamedPipeServerStream _previousStream = null;
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

                var pipeStream = CreatePipe(PipeName);
     
                try
                {
                    pipeStream.WaitForConnection();
                }
                catch
                {
                    pipeStream.Disconnect();
                }

                Console.WriteLine($"Connection received from pipe {PipeName}");

                _previousStream = pipeStream;

                Task.Factory.StartNew(() => ProcessClientThread(pipeStream));
            }
            catch (Exception e)
            {
                //If there are no more avail connections (254 is in use already) then just keep looping until one is avail
                _log.Error("ProcessNextClient error: {0}", e.ToString());
            }
        }
    }
}
