using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.IPC
{
    public interface IConnection
    {
        bool IsConnected { get; }
        string ReadRequest();
        bool TrySendResponse(string message);
    }
}
