using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Memory
{
    internal static partial class MemoryHelper
    {
        internal static IntPtr Allocate(
            SafeProcessHandle processHandle,
            int size,
            MemoryProtectionType protection,
            uint allocation = MemoryAllocationType.Commit | MemoryAllocationType.Reserve)
        {
            IntPtr allocationAddress = Interop.Kernel32.VirtualAllocEx(
                processHandle,
                IntPtr.Zero,
                new UIntPtr((uint) size),
                allocation,
                ConvertToPlatforProtectionType(protection));

            if (allocationAddress == IntPtr.Zero)
            {
                throw new Win32Exception($"Failed to allocated a memory region of {size} bytes.");
            }

            return allocationAddress;
        }

        private static uint ConvertToPlatforProtectionType(MemoryProtectionType protection)
        {
            return (uint) protection;
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
                throw new Win32Exception($"Failed to free the memory region at {address.ToString("X")}.");
            }
        }
    }
}
