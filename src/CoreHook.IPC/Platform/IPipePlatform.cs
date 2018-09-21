using System.IO.Pipes;

namespace CoreHook.IPC.Platform
{
    public interface IPipePlatform
    {
        NamedPipeServerStream CreatePipeByName(string pipeName);
    }
}
