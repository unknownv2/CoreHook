using System.IO.Pipes;

namespace CoreHook.IPC.Platform;

public class DefaultPipePlatform : IPipePlatform
{
    internal static IPipePlatform Instance { get; } = new DefaultPipePlatform();

    public NamedPipeServerStream CreatePipeByName(string pipeName, string serverName)
    {
        return new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 65536, 65536);
    }
}
