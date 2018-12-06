using System.IO.Pipes;
using CoreHook.IPC.Platform;

namespace CoreHook.Tests
{
    public class PipePlatformBase : IPipePlatform
    {
        public NamedPipeServerStream CreatePipeByName(string pipeName, string serverName)
        {
            return new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                65536,
                65536
            );
        }
    }
}
