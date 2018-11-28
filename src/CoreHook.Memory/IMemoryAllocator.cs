using System.Collections.Generic;

namespace CoreHook.Memory
{
    public interface IMemoryAllocator
    {
        IEnumerable<IMemoryAllocation> Allocations { get; }
        IMemoryAllocation Allocate(int size, uint protection, bool mustBeDisposed = true);
        void Deallocate(IMemoryAllocation allocation);
    }
}
