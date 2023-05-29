using System.Runtime.InteropServices;

namespace CoreHook.BinaryInjection.PortableExecutable;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal unsafe struct OptionalHeader
{
    [MarshalAs(UnmanagedType.U2)]
    [FieldOffset(0)]
    internal ImageMagic ImageMagic;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    [FieldOffset(0x60)]
    private fixed byte DataDirectory32Bytes[16 * 8];

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    [FieldOffset(0x70)]
    private fixed byte DataDirectory64Bytes[16 * 8];

    internal unsafe DataDirectory DataDirectory(ImageDirectoryEntry imageDirectoryEntry)
    {
        fixed (byte* x = DataDirectory32Bytes)
        fixed (byte* y = DataDirectory64Bytes)
        {
            return ((DataDirectory*)(ImageMagic == ImageMagic.Magic32 ? x : y))[(ushort)imageDirectoryEntry];
        }
    }
}
