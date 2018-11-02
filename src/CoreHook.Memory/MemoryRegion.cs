using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public class MemoryRegion : MemoryPointer
    {
        public MemoryRegion(IProcess process, IntPtr address) : base(process, address) { }

        public void Release()
        {
            MemoryHelper.Free(Process.SafeHandle, Address);
            Address = IntPtr.Zero;
        }
    }
}
