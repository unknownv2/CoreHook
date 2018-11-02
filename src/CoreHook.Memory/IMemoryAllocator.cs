using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public interface IMemoryAllocator
    {
        IEnumerable<IMemoryAllocation> Allocations { get; }
        IMemoryAllocation Allocate(
            int size,
            MemoryProtectionType protection,
            bool mustBeDisposed = true);
        void Deallocate(IMemoryAllocation allocation);
    }
}
