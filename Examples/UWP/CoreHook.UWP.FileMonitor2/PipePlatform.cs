
using System.IO.Pipes;
using CoreHook.IPC.Platform;

namespace CoreHook.UWP.FileMonitor2
{
    public class PipePlatform : IPipePlatform
    {
        private int _maxConnections = 254;

        public NamedPipeServerStream CreatePipeByName(string pipeName)
        {
            return new NamedPipeServerStream(
             pipeName,
             PipeDirection.InOut,
             _maxConnections,
             PipeTransmissionMode.Byte,
             PipeOptions.Asynchronous,
             65536,
             65536
             );
        }
    }
}
