using System;
using System.ComponentModel;

using Windows.Win32.System.Memory;

namespace CoreHook.BinaryInjection.Memory;

public partial class MemoryAllocation
{
    private unsafe IntPtr Allocate(int size, PAGE_PROTECTION_FLAGS protection)
    //,MemoryAllocationType allocation //VIRTUAL_ALLOCATION_TYPE allocation = VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE)
    {
        IntPtr allocationAddress = (IntPtr)NativeMethods.VirtualAllocEx(_processHandle, IntPtr.Zero.ToPointer(), new UIntPtr((uint)size), VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE, protection);

        if (allocationAddress == IntPtr.Zero)
        {
            throw new Win32Exception($"Failed to allocated a memory region of {size} bytes.");
        }

        return allocationAddress;
    }

    public int WriteBytes(byte[] byteArray)
    {
        //nuint bytesWritten; // Probably have to allocate this first - weird that it's not handled by CsWin32?
        //NativeMethods.WriteProcessMemory(_processHandle, Address.ToPointer(), &byteArray, (nuint)byteArray.Length, &bytesWritten))
        if (!Interop.Kernel32.WriteProcessMemory(_processHandle, Address, byteArray, byteArray.Length, out IntPtr bytesWritten))
        {
            throw new Win32Exception("Failed to write to process memory");
        }
        var expectedWriteCount = byteArray.Length;
        var bytesWrittenCount = bytesWritten.ToInt32();
        if (byteArray.Length != bytesWrittenCount)
        {
            throw new Win32Exception($"Failed to write all data to process ({bytesWrittenCount} != {expectedWriteCount}).");
        }

        return bytesWrittenCount;
    }

    private unsafe void Free()
    {
        if (!NativeMethods.VirtualFreeEx(_processHandle, Address.ToPointer(), new UIntPtr(0), VIRTUAL_FREE_TYPE.MEM_RELEASE))
        {
            throw new Win32Exception($"Failed to free the memory region at {Address.ToInt64():X16}.");
        }
    }
}
