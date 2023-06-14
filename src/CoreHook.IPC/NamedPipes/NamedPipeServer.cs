using CoreHook.IPC.Messages;
using CoreHook.IPC.Platform;

using System;
using System.IO;

namespace CoreHook.IPC.NamedPipes;

/// <summary>
/// Creates a pipe server and allows custom handling of messages from clients.
/// </summary>
public class NamedPipeServer : NamedPipeBase
{
    private readonly Action<INamedPipe>? _handleConnection;

    protected string _pipeName;

    private readonly IPipePlatform _platform;

    private bool _connectionStopped;

    /// <summary>
    /// Initialize a new instance of the <see cref="NamedPipeServer"/> class.
    /// </summary>
    /// <param name="pipeName">The name of the pipe server.</param>
    /// <param name="platform">Method for initializing a new pipe-based server.</param>
    /// <param name="handleConnection">Event handler called when receiving a new connection.</param>
    public NamedPipeServer(string pipeName, IPipePlatform platform, Action<INamedPipe>? handleConnection = null)
    {
        _pipeName = pipeName;
        _platform = platform;
        _handleConnection = handleConnection;

        Connect();
    }

    /// <summary>
    /// Initialize a new pipe server.
    /// </summary>
    /// <param name="pipeName">The name of the pipe server.</param>
    /// <param name="platform">Method for initializing a new pipe-based server.</param>
    /// <param name="handleRequest">Event handler called when receiving a new message from a client.</param>
    /// <returns>An instance of the new pipe server.</returns>
    public NamedPipeServer(string pipeName, IPipePlatform platform, Action<CustomMessage> handleRequest) : this(pipeName, platform)
    {
        _handleConnection = message => HandleMessage(handleRequest);
    }

    LogMessage defaultMissingBody = new LogMessage(LogLevel.Error, "A message has been received but could not be parsed. Ignoring.");
    private async void HandleMessage(Action<CustomMessage> handleRequest)
    {
        while (Stream?.IsConnected ?? false)
        {
            var message = await Read();

            // Exit the loop if the stream was closed after reading
            if (!Stream.IsConnected)
            {
                break;
            }

            handleRequest(message ?? defaultMissingBody);
        }
    }

    /// <inheritdoc />
    public override async void Connect()
    {
        try
        {
            if (Stream is not null)
            {
                throw new InvalidOperationException("Pipe server already started");
            }

            var pipeStream = _platform.CreatePipeByName(_pipeName);

            Stream = pipeStream;

            await pipeStream.WaitForConnectionAsync();

            if (!_connectionStopped)
            {
                _handleConnection?.Invoke(this);
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
    public new void Dispose()
    {
        _connectionStopped = true;

        base.Dispose();
    }
}
