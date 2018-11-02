using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public interface IDisposableState : IDisposable
    {
        bool IsDisposed { get; }
        bool MusBeDisposed { get; }
    }
}
