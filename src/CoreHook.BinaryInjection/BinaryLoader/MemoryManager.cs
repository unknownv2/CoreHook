using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CoreHook.BinaryInjection
{
    public class MemoryOperationException : Exception
    {
        public MemoryOperationException(string operation)
                    : base($"Memory operation '{operation}' failed.")
        {
        }
    }

    public class MemoryManager : IMemoryManager
    {
        private List<MemoryAllocation> _allocatedAddresses = new List<MemoryAllocation>();

        public Func<Process, IntPtr, uint, bool> FreeMemory { get; set; }

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
        private bool disposedValue = false; // To detect redundant calls
   
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

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MemoryManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
