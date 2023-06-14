using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;

namespace CoreHook.IPC.NamedPipes;

/// <summary>
/// Creates a pipe client for communication with a pipe server.
/// </summary>
public class NamedPipeClient : NamedPipeBase
{
    public NamedPipeClient(string pipeName, bool connect = false) 
    {
        _pipeName = pipeName;

        if (connect)
        {
            Connect();
        }
    }

    /// <inheritdoc />
    public override void Connect()
    {
        if (Stream is not null)
        {
            throw new IOException("Client pipe already connected");
        }

        ArgumentNullException.ThrowIfNull(_pipeName);

        var pipeStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation);

        Stream = pipeStream;

        pipeStream.Connect();
    }
}
