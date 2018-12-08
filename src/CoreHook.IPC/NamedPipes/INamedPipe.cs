using System;
using CoreHook.IPC.Transport;

namespace CoreHook.IPC.NamedPipes
{
    public interface INamedPipe : ITransportChannel, IDisposable
    {
        bool Connect();
    }
}
