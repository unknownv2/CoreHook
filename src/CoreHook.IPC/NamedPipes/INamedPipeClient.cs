using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.IPC.NamedPipes
{
    public interface INamedPipeClient : IDisposable
    {
        bool Connect(int timeOutMilliseconds);
        void SendRequest(string request);
        string ReadRawResponse();
    }
}
