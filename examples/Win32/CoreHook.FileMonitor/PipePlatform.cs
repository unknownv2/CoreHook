using System.IO.Pipes;
using CoreHook.IPC.Platform;

namespace CoreHook.FileMonitor
{
    public class PipePlatform : IPipePlatform
    {
        public NamedPipeServerStream CreatePipeByName(string pipeName)
        {
            return new NamedPipeServerStream(
             pipeName,
             PipeDirection.InOut,
             NamedPipeServerStream.MaxAllowedServerInstances,
             PipeTransmissionMode.Byte,
             PipeOptions.Asynchronous,
             65536,
             65536
             );
        }
    }
}
