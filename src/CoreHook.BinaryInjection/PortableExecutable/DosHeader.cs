using System.Runtime.InteropServices;

namespace CoreHook.BinaryInjection.PortableExecutable;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct DosHeader
{
    internal readonly ushort e_magic;
    internal readonly ushort e_cblp;
    internal readonly ushort e_cp;
    internal readonly ushort e_crlc;
    internal readonly ushort e_cparhdr;
    internal readonly ushort e_minalloc;
    internal readonly ushort e_maxalloc;
    internal readonly ushort e_ss;
    internal readonly ushort e_sp;
    internal readonly ushort e_csum;
    internal readonly ushort e_ip;
    internal readonly ushort e_cs;
    internal readonly ushort e_lfarlc;
    internal readonly ushort e_ovno;

    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    internal fixed ushort e_res1[4];

    internal readonly ushort e_oemid;
    internal readonly ushort e_oeminfo;

    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    internal fixed ushort e_res2[10];

    internal readonly uint e_lfanew;
}
