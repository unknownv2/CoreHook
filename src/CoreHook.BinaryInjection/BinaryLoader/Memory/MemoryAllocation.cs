using System;
using System.Diagnostics;

namespace CoreHook.BinaryInjection
{
    public class MemoryAllocation
    {
        public Process Process;
        public IntPtr Address;
        public uint Size;
        public bool IsFree;
    }
}
