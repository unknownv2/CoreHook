using System;
using System.Runtime.InteropServices;

using Windows.Win32.System.Memory;

namespace CoreHook.Memory;

public partial class MemoryAllocation : IDisposable
{
    public IntPtr Address { get; private set; }
    public int Size { get; init; }
    public bool IsFree => IsDisposed;
    public bool IsDisposed { get; private set; }
    public bool MustBeDisposed { get; init; }

    private readonly SafeHandle _processHandle;

    private MemoryAllocation() { }

    internal static MemoryAllocation Wrap(IntPtr address, int size)
    {
        return new MemoryAllocation() { Address = address, MustBeDisposed = false, Size = size };
    }

    internal MemoryAllocation(SafeHandle process, int size, MemoryProtectionType protection, bool mustBeDisposed = true)
    {
        _processHandle = process;

        Address = Allocate(size, (PAGE_PROTECTION_FLAGS)(uint)protection);
        Size = size;
        MustBeDisposed = mustBeDisposed;
        IsDisposed = false;
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            IsDisposed = true;
            Free();
            Address = IntPtr.Zero;
        }
    }

    ~MemoryAllocation()
    {
        if (MustBeDisposed)
        {
            Dispose();
        }
    }
}
