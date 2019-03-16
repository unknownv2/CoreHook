using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Kernel32
    {
        [DllImport(Interop.Libraries.Kernel32, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern ushort DeleteAtom(ushort nAtom);
    }
}