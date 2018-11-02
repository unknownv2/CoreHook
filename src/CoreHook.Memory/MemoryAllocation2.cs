using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public class MemoryAllocation2 : MemoryRegion, IMemoryAllocation
    {
        public new IntPtr Address { get; set; }
        public new IProcess Process { get; set; }
        public int Size { get; set; }
        public bool IsFree { get; set; }
   
        internal MemoryAllocation2() : base(null, IntPtr.Zero)
        {

        }

        internal MemoryAllocation2(IProcess process, int size,
            MemoryProtectionType protection, bool mustBeDisposed = true)
            : base(process, MemoryHelper.Allocate(process.SafeHandle, size, protection))
        {
            Size = size;
            IsDisposed = false;
        }


        public bool IsDisposed { get; private set; }
        public bool MusBeDisposed { get; set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
            }
        }

        ~MemoryAllocation2()
        {
            if (MusBeDisposed)
            {
                Dispose();
            }
        }
    }
}
