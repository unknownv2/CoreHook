using System.IO;

namespace CoreHook.IPC
{
    public interface IConnection
    {
        Stream Stream { get; }
        bool IsConnected { get; }
    }
}
