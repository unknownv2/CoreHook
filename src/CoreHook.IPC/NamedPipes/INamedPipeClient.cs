using System;
using System.IO.Pipes;

namespace CoreHook.IPC.NamedPipes
{
    public interface INamedPipeClient : IDisposable
    {
        PipeStream PipeStream { get; }
        bool Connect(int timeOutMilliseconds);
        void SendRequest(string request);
        string ReadRawResponse();
    }
}
