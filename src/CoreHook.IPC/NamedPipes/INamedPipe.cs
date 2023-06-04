using CoreHook.IPC.Handlers;

using System;
using System.IO;
using System.IO.Pipes;

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

    /// <summary>
    /// Manages sending and receiving messages through the <see cref="Stream"/>.
    /// </summary>
    IMessageHandler MessageHandler { get; }
}
