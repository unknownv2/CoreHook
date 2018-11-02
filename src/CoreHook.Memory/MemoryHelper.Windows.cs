using System;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Memory
{
    internal static partial class MemoryHelper
    {
        internal static IntPtr Allocate(
            SafeProcessHandle processHandle,
            int size,
            uint protection,
            uint allocation = MemoryAllocationType.Commit | MemoryAllocationType.Reserve)
        {
            IntPtr allocationAddress = Interop.Kernel32.VirtualAllocEx(
                processHandle,
                IntPtr.Zero,
                new UIntPtr((uint) size),
                allocation,
                protection);

            if (allocationAddress == IntPtr.Zero)
            {
                throw new Win32Exception($"Failed to allocated a memory region of {size} bytes.");
            }

            return allocationAddress;
        }

        public static int WriteBytes(SafeProcessHandle processHandle, IntPtr address, byte[] byteArray)
        {
            if (!Interop.Kernel32.WriteProcessMemory(
                     processHandle,
                     address,
                     byteArray,
                     byteArray.Length,
                     out IntPtr bytesWritten))
            {
                throw new Win32Exception("Failed to write to process memory");
            }
            var expectedWriteCount = byteArray.Length;
            var bytesWrittenCount = bytesWritten.ToInt32();
            if (byteArray.Length != bytesWrittenCount)
            {
                throw new Win32Exception(
                    $"Failed to write all data to process ({bytesWrittenCount} != {expectedWriteCount}).");
            }
            return bytesWrittenCount;
        }

        public static void Free(SafeProcessHandle processHandle, IntPtr address)
        {
            if (!Interop.Kernel32.VirtualFreeEx(
                processHandle,
                address,
                new UIntPtr(0),
                Interop.Kernel32.FreeType.Release))
            {
                throw new Win32Exception($"Failed to free the memory region at {address:X16}.");
            }
        }
    }
}
