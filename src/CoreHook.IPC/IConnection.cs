using System.IO.Pipes;

namespace CoreHook.IPC
{
    public interface IConnection
    {
        NamedPipeServerStream ServerStream { get; }
        bool IsConnected { get; }

        string ReadRequest();
        bool TrySendResponse(string message);
    }
}
