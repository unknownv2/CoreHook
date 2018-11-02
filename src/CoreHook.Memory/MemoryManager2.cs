using System;
using System.Collections.Generic;
using System.Diagnostics;
using CoreHook.Memory.Processes;

namespace CoreHook.Memory
{
    public class MemoryManager2 : IMemoryManager2
    {
        private List<MemoryAllocation2> _allocatedAddresses = new List<MemoryAllocation2>();

        public Func<Process, IntPtr, int, bool> FreeMemory { get; set; }

        public MemoryManager2()
        {

        }

        public IntPtr Add(Process process, IntPtr address, bool isFree, int size = 0)
        {
            _allocatedAddresses.Add(new MemoryAllocation2
            {
                Process = new ProcessManager2(process),
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
                        if (!FreeMemory(memAlloc.Process.ProcessHandle, memAlloc.Address, memAlloc.Size))
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
