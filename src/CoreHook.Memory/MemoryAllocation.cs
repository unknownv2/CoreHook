
namespace CoreHook.Memory
{
    public class MemoryAllocation : MemoryRegion, IMemoryAllocation
    {
        public int Size { get; set; }
        public bool IsFree => IsDisposed;

        internal MemoryAllocation(IProcess process, int size,
            uint protection, bool mustBeDisposed = true)
            : base(process, MemoryHelper.Allocate(process.SafeHandle, size, protection))
        {
            Size = size;

            MusBeDisposed = mustBeDisposed;
            IsDisposed = false;
        }

        public bool IsDisposed { get; private set; }
        public bool MusBeDisposed { get; set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Release();
            }
        }

        ~MemoryAllocation()
        {
            if (MusBeDisposed)
            {
                Dispose();
            }
        }
    }

}
