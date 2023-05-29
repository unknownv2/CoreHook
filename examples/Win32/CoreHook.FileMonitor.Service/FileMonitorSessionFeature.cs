using System.Threading;

namespace CoreHook.FileMonitor.Service;

public class FileMonitorSessionFeature : Examples.Common.ISessionFeature
{
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> that can stop the server.
    /// </summary>
    public CancellationToken CancellationToken => _cts.Token;

    /// <summary>
    /// Stops the server.
    /// </summary>
    public void StopServer()
    {
        _cts.Cancel();
    }
}
