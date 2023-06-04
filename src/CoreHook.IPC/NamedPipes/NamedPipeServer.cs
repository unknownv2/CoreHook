using CoreHook.IPC.Handlers;
using CoreHook.IPC.Messages;
using CoreHook.IPC.Platform;

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace CoreHook.IPC.NamedPipes;

/// <summary>
/// Creates a pipe server and allows custom handling of messages from clients.
/// </summary>
public class NamedPipeServer : INamedPipe
{
    /// <inheritdoc />
    public IMessageHandler MessageHandler { get; private set; }

    /// <inheritdoc />
    public PipeStream Stream => _pipe;

    private const int MaxPipeNameLength = 250;

    private readonly Action<INamedPipe> _handleTransportConnection;
    private readonly string _pipeName;
    private readonly IPipePlatform _platform;

    private bool _connectionStopped;
    private NamedPipeServerStream _pipe;

    /// <summary>
    /// Initialize a new instance of the <see cref="NamedPipeServer"/> class.
    /// </summary>
    /// <param name="pipeName">The name of the pipe server.</param>
    /// <param name="platform">Method for initializing a new pipe-based server.</param>
    /// <param name="handleTransportConnection">Event handler called when receiving a new connection.</param>
    private NamedPipeServer(string pipeName, IPipePlatform platform, Action<INamedPipe> handleTransportConnection)
    {
        _pipeName = pipeName;
        _platform = platform;
        _handleTransportConnection = handleTransportConnection;
        _connectionStopped = false;
    }

    /// <summary>
    /// Initialize a new pipe server.
    /// </summary>
    /// <param name="pipeName">The name of the pipe server.</param>
    /// <param name="platform">Method for initializing a new pipe-based server.</param>
    /// <param name="handleRequest">Event handler called when receiving a new message from a client.</param>
    /// <returns>An instance of the new pipe server.</returns>
    public static NamedPipeServer StartNew(string pipeName, IPipePlatform platform, Action<IStringMessage, INamedPipe> handleRequest)
    {
        return StartNewInternal(pipeName, platform, connection => HandleTransportConnection(connection, handleRequest));
    }

    /// <summary>
    /// Initialize a new pipe server.
    /// </summary>
    /// <param name="pipeName">The name of the pipe server.</param>
    /// <param name="platform">Method for initializing a new pipe-based server.</param>
    /// <param name="handleRequest">Event handler called when receiving a new connection.</param>
    /// <returns>An instance of the new pipe server.</returns>
    public static NamedPipeServer StartNew(string pipeName, IPipePlatform platform, Action<INamedPipe> handleRequest)
    {
        return StartNewInternal(pipeName, platform, handleRequest);
    }

    /// <summary>
    /// Initialize a new pipe server.
    /// </summary>
    /// <param name="pipeName">The name of the pipe server.</param>
    /// <param name="platform">Method for initializing a new pipe-based server.</param>
    /// <param name="handleRequest">Event handler called when receiving a new connection.</param>
    /// <returns>An instance of the new pipe server.</returns>
    private static NamedPipeServer StartNewInternal(string pipeName, IPipePlatform platform, Action<INamedPipe> handleRequest)
    {
        if (pipeName.Length > MaxPipeNameLength)
        {
            throw new PipeMessageLengthException(pipeName, MaxPipeNameLength);
        }
        var pipeServer = new NamedPipeServer(pipeName, platform, handleRequest);
        pipeServer.Connect();
        return pipeServer;
    }

    private static void HandleTransportConnection(INamedPipe channel, Action<IStringMessage, INamedPipe> handleRequest)
    {
        var connection = channel.Stream;

        while (connection?.IsConnected ?? false)
        {
            var message = channel.MessageHandler.Read();
            if (message is null ||
                (message.Header is null && message.Body is null) ||
                !connection.IsConnected)
            {
                break;
            }
            handleRequest(message, channel);
        }
    }

    /// <inheritdoc />
    public async void Connect()
    {
        try
        {
            if (_pipe is not null)
            {
                throw new InvalidOperationException("Pipe server already started");
            }

            _pipe = _platform.CreatePipeByName(_pipeName);

            await _pipe.WaitForConnectionAsync();// BeginWaitForConnection(OnConnection, _pipe);

            if (!_connectionStopped)
            {
                MessageHandler = new MessageHandler(_pipe);

                _handleTransportConnection?.Invoke(this);
            }
        }
        catch (IOException e)
        {
            Log($"Pipe {_pipeName} broken with: ", e);
        }
        catch (Exception e)
        {
            Log("Unhandled exception during server start", e);
        }
    }

    private static void Log(string message, Exception e)
    {
        Console.WriteLine(message);
        Console.WriteLine(e);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _connectionStopped = true;

        Interlocked.Exchange(ref _pipe, null)?.Dispose();
    }
}
