using System;
using System.Runtime.InteropServices;
using System.Text;

internal partial class Interop
{
    internal partial class Kernel32
    {
        [DllImport(Interop.Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern uint GetAtomNameW(ushort nAtom, StringBuilder lpBuffer, int nSize);
    }
}