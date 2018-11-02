using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public class MemoryRegion : MemoryPointer, IEquatable<MemoryRegion>
    {
        public bool Equals(MemoryRegion other)
        {
            throw new NotImplementedException();
        }

        public MemoryRegion(IProcess process, IntPtr address) : base(process, address)
        {

        }

        public void Release()
        {
            MemoryHelper.Free(Process.SafeHandle, Address);
            Address = IntPtr.Zero;
        }
    }
}
