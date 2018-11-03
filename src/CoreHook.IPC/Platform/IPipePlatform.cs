using System.IO.Pipes;

namespace CoreHook.IPC.Platform
{
    /// <summary>
    /// Interface for implementing the creation of named pipe server by name.
    /// </summary>
    public interface IPipePlatform
    {
        /// <summary>
        /// Creates a named pipe server for communication with a client.
        /// </summary>
        /// <param name="pipeName">Name of the pipe server.</param>
        /// <returns></returns>
        NamedPipeServerStream CreatePipeByName(string pipeName);
    }
}
