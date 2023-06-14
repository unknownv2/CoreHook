using CoreHook.IPC.Messages;

using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace CoreHook.IPC.NamedPipes;

/// <summary>
/// Interface defining a named pipe communication channel.
/// </summary>
public interface INamedPipe : IDisposable
{
    public PipeStream Stream { get; }

    /// <summary>
    /// Initialize the pipe connection.
    /// </summary>
    /// <returns>True if initialization completed successfully.</returns>
    void Connect();

    Task<CustomMessage?> Read();

    Task<bool> TryWrite(CustomMessage message);
}
