using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public interface IMemoryWriter
    {
        void WriteMemory(long address, byte[] data);
    }
}
