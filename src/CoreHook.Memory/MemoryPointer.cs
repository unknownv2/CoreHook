using System;

namespace CoreHook.Memory
{
    public class MemoryPointer : IPointer
    {
        public IntPtr Address { get; protected set; }
        public IProcess Process { get; protected set; }

        public MemoryPointer(IProcess process, IntPtr address)
        {
            Process = process;
            Address = address;
        }
    }
}
