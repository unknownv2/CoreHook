using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public interface IPointer
    {
        IntPtr Address { get; }
    }
}
