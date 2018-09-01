using System;

namespace CoreHook.RPC
{   
    public interface IRemoteClient
    {
        /// <summary>
        /// Create a client for RPC
        /// </summary>
        /// <typeparam name="T">Interface implemented by the server.</typeparam>
        /// <returns>A proxy handle to interface with the server.</returns>
        T Create<T>();
    }

    public interface IRemoteServer
    {
        /// <summary>
        /// Create a server for RPC
        /// </summary>
        /// <typeparam name="T">Interface implemented by the server.</typeparam>
        /// <param name="serverImp">Implementation of server interface</param>
        void Create(Type serverImp);
    }
}
