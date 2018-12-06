using System;
using CoreHook.IPC.Transport;

namespace CoreHook.IPC.NamedPipes
{
    public interface INamedPipeClient : ITransportChannel, IDisposable
    {
        bool Connect(int timeOutMilliseconds = 10000);
    }
}
