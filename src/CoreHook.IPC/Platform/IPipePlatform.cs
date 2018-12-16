using System.IO.Pipes;

namespace CoreHook.IPC.Platform
{
    /// <summary>
    /// Interface for implementing the creation of named pipe server.
    /// </summary>
    public interface IPipePlatform
    {
        /// <summary>
        /// Creates a named pipe server for communication with a client.
        /// </summary>
        /// <param name="pipeName">The name of the pipe.</param>
        /// <param name="serverName">The name of the remote computer, or "." to specify the local computer.</param>
        /// <returns></returns>
        NamedPipeServerStream CreatePipeByName(string pipeName, string serverName = ".");
    }
}
