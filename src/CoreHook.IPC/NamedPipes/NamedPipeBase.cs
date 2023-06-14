using CoreHook.IPC.Messages;

using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace CoreHook.IPC.NamedPipes;
public abstract class NamedPipeBase : INamedPipe
{
    protected string _pipeName;

    public virtual PipeStream? Stream
    {
        get => _pipeStream;
        protected set
        {
            _pipeStream = value;
            _reader = new StreamReader(_pipeStream);
            _writer = new StreamWriter(_pipeStream);
        }
    }

    private PipeStream? _pipeStream;

    private StreamReader? _reader;
    private StreamWriter? _writer;

    public async Task<bool> TryWrite(CustomMessage message)
    {
        if (!Stream.IsConnected)
        {
            throw new IOException("Pipe connection is closed. Unable to write.");
        }

        try
        {
            await _writer.WriteLineAsync(message.Serialize());
            await _writer.FlushAsync();

            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }


    /// <inheritdoc />
    public async Task<CustomMessage?> Read()
    {
        if (!_pipeStream.IsConnected)
        {
            throw new IOException("Pipe connection is closed. Unable to read.");
        }

        try
        {
            var message = await _reader.ReadLineAsync();
            if (message is not null)
            {
                return CustomMessage.Deserialize(message);
            }
        }
        catch (IOException) { }

        return null;
    }

    public abstract void Connect();


    /// <inheritdoc />
    public virtual void Dispose()
    {
        Interlocked.Exchange(ref _pipeStream, null)?.Dispose();
    }
}
