using System;
using static CoreHook.Unmanaged.NativeMethods;
using System.ComponentModel;

namespace CoreHook.Unmanaged
{
    public class Memory
    {
        public static IntPtr Allocate(IntPtr address, int size, AllocationType allocationFlags = AllocationType.Commit, MemoryProtection protectionFlags = MemoryProtection.ExecuteReadWrite)
        {
            // Allocate a memory page
            var ret = VirtualAlloc(address, (uint)size, allocationFlags, protectionFlags);

            // Check whether the memory page is valid
            if (ret != IntPtr.Zero)
                return ret;

            // If the pointer isn't valid, throws an exception
            throw new Win32Exception(string.Format("Couldn't allocate memory of {0} byte(s).", size));
        }
        public static void Free(IntPtr address, int size = 0, FreeType freeType = FreeType.Release)
        {
            // Free the memory
            if (!VirtualFree(address, size, freeType))
            {
                // If the memory wasn't correctly freed, throws an exception
                throw new Win32Exception(string.Format("The memory page 0x{0} cannot be freed.", address.ToString("X")));
            }
        }
        public static bool ChangeProtection(IntPtr address, uint size, MemoryProtection protection, out MemoryProtection oldProtect)
        {
            if(!VirtualProtect(address, size, protection, out oldProtect))
            {
                throw new Win32Exception(string.Format("The memory page 0x{0} protection change failed.", address.ToString("X")));
            }
            return true;
        }
    }
}
