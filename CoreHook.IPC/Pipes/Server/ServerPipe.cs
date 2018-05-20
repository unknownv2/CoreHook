using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace CoreHook.IPC.Pipes.Server
{
    public class ServerPipe
    {
        private string _pipeName;

        private int _numOfThreads;

        private NamedPipeServerStream _pipe;

        public ServerPipe(string pipeName, int numOfThreads = 1)
        {
            _pipeName = pipeName;
            _numOfThreads = numOfThreads;
        }

        public NamedPipeServerStream Start(int numThreads)
        {
            _pipe = CreatePipe(_pipeName, _numOfThreads);
            _pipe.WaitForConnection();
            return _pipe;
        }

        private static NamedPipeServerStream CreatePipe(string pipeName, int numOfThreads)
        {
            return new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                numOfThreads);
        }
    }
}
