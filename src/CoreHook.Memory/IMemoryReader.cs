using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public interface IMemoryReader
    {
        byte[] ReadMemory(long address);
    }
}
