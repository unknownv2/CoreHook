using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreHook.RPC
{   
    public interface RemoteClient
    {
        /// <summary>
        /// Create a client for IPC
        /// </summary>
        /// <typeparam name="T">Interface implemented by the server</typeparam>
        /// <returns>A proxy handle to interface with the server.</returns>
        T Create<T>();
    }

    public interface RemoteServer
    {
        /// <summary>
        /// Create a server for IPC
        /// </summary>
        /// <typeparam name="T">Interface implemented by the server</typeparam>
        void Create<T>();
    }
}
