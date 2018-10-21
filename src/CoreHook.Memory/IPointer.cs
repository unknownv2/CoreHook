using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    interface IPointer
    {
        IntPtr Address { get; }
    }
}
