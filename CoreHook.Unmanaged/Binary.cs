using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.Unmanaged
{
    public class Binary
    {
        public static byte[] StructToByteArray(object obj, int length = -1)
        {
            var len = Marshal.SizeOf(obj);
            var arr = new byte[length == -1 ? len : length];

            var ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}