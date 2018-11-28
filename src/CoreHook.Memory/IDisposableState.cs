using System;

namespace CoreHook.Memory
{
    public interface IDisposableState : IDisposable
    {
        bool IsDisposed { get; }
        bool MusBeDisposed { get; }
    }
}
