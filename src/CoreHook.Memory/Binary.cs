using System.Runtime.InteropServices;

namespace CoreHook.Memory
{
    public class Binary
    {
        public static byte[] StructToByteArray(object obj, int? length = null)
        {
            var objectLength = Marshal.SizeOf(obj);
            var arr = new byte[length ?? objectLength];

            var ptr = Marshal.AllocHGlobal(objectLength);

            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, arr, 0, objectLength);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}