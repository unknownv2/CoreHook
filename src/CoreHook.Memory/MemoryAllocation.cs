using System;
using System.Diagnostics;

namespace CoreHook.Memory
{
    public class MemoryAllocation : IMemoryAllocation
    {
        public Process Process;
        public IntPtr Address { get; set; }
        public int Size { get; set; }
        public bool IsFree { get; set; }
    }
}
