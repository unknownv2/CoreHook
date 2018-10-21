using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    interface IMemoryAllocation : IPointer
    {
        bool IsFree { get; }
        int Size { get; }
    }
}
