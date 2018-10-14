using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreHook.BinaryInjection
{
    public class MemoryManager : IMemoryManager
    {
        private List<MemoryAllocation> _allocatedAddresses = new List<MemoryAllocation>();

        public Func<Process, IntPtr, uint, bool> FreeMemory { get; set; }

        public MemoryManager()
        {

        }

        public IntPtr Add(Process process, IntPtr address, bool isFree, uint size = 0)
        {
            _allocatedAddresses.Add(new MemoryAllocation()
            {
                Process = process,
                Address = address,
                Size = size,
                IsFree = isFree
            });
            return address;
        }
 
        public void FreeAllocations()
        {
            if (FreeMemory != null)
            {
                foreach (var memAlloc in _allocatedAddresses)
                {
                    if (!memAlloc.IsFree)
                    {
                        if (!FreeMemory(memAlloc.Process, memAlloc.Address, memAlloc.Size))
                        {
                            throw new MemoryOperationException("free");
                        }
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;
   
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    FreeAllocations();

                    _allocatedAddresses.Clear();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
