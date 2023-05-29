using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using CoreHook.IPC.Handlers;

namespace CoreHook.IPC.NamedPipes;

/// <summary>
/// Creates a pipe client for communication with a pipe server.
/// </summary>
public class NamedPipeClient : INamedPipe
{
    /// <inheritdoc />
    public IConnection Connection { get; private set; }
    /// <inheritdoc />
    public IMessageHandler MessageHandler { get; private set; }

    private NamedPipeClientStream _pipeStream;

    private bool _connectionStopped;

    private readonly string _pipeName;

    /// <summary>
    /// Initialize a new instance of the <see cref="NamedPipeClient"/> class.
    /// </summary>
    /// <param name="pipeName">The name of the pipe server to connect to.</param>
    public NamedPipeClient(string pipeName)
    {
        _pipeName = pipeName;
        _connectionStopped = false;
    }

    /// <summary>
    /// Initialize a pipe connection to a pipe by name.
    /// </summary>
    /// <param name="pipeName">The name of the pipe to connect to.</param>
    /// <returns>The stream communication channel for the pipe client.</returns>
    public static PipeStream CreatePipeStream(string pipeName)
    {
        var client = new NamedPipeClient(pipeName);
        return client.Connect() ? client._pipeStream : null;
    }

    /// <inheritdoc />
    public bool Connect()
    {
        if (_pipeStream != null)
        {
            throw new InvalidPipeOperationException("Client pipe already connected");
        }
        if (_pipeName == null)
        {
            throw new InvalidPipeOperationException("Client pipe name was not set");
        }

        try
        {
            _pipeStream = new NamedPipeClientStream(
                ".",
                _pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous,
                TokenImpersonationLevel.Impersonation);

            _pipeStream.Connect();
        }
        catch (IOException e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }

        Connection = new PipeConnection(_pipeStream, () => _connectionStopped);
        MessageHandler = new MessageHandler(Connection);

        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _connectionStopped = true;
        _pipeStream?.Dispose();
        _pipeStream = null;
    }
}
