using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreHook.Memory
{
    public class MemoryManager : IMemoryManager
    {
        protected readonly List<IMemoryAllocation> MemoryAllocations;

        private readonly IProcess _process;

        public IEnumerable<IMemoryAllocation> Allocations => MemoryAllocations.AsReadOnly();

          public MemoryManager(IProcess process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
            MemoryAllocations = new List<IMemoryAllocation>();
        }

        public IMemoryAllocation Allocate(
            int size,
            uint protection,
            bool mustBeDisposed = true)
        {
            var memory = new MemoryAllocation(_process, size, protection, mustBeDisposed);
            MemoryAllocations.Add(memory);
            return memory;
        }

        public void Deallocate(IMemoryAllocation allocation)
        {
            if (!allocation.IsFree)
            {
                allocation.Dispose();
            }

            if (MemoryAllocations.Contains(allocation))
            {
                MemoryAllocations.Remove(allocation);
            }
        }

        public byte[] ReadMemory(long address)
        {
            throw new NotImplementedException();
        }

        public void WriteMemory(long address, byte[] data)
        {
            MemoryHelper.WriteBytes(
                _process.SafeHandle,
                new IntPtr(address),
                data);
        }

        public virtual void Dispose()
        {
            foreach(var memoryAllocation in MemoryAllocations.Where(m => m.MusBeDisposed).ToArray())
            {
                memoryAllocation.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~MemoryManager()
        {
            Dispose();
        }
    }
}
