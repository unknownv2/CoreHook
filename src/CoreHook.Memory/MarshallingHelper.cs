using System.Runtime.InteropServices;

namespace CoreHook.Memory
{
    public static class MarshallingHelper
    {
        public static byte[] StructToByteArray(object obj, int? length = null)
        {
            var objectLength = length ?? Marshal.SizeOf(obj);
            var arr = new byte[objectLength];

            var ptr = Marshal.AllocHGlobal(objectLength);

            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, arr, 0, objectLength);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}