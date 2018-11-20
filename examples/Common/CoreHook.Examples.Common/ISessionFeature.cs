using System.Threading;

namespace CoreHook.Examples.Common
{
    public interface ISessionFeature
    {
        CancellationToken CancellationToken { get; }
        void StopServer();
    }
}
