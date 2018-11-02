using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public interface IMemoryAllocation : IPointer, IDisposableState
    {
        bool IsFree { get; }
        int Size { get; }
    }
}
