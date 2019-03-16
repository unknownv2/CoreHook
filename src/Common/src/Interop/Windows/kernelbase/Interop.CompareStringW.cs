using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class KernelBase
    {
        [DllImport(Interop.Libraries.KernelBase, CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern int CompareStringW(uint locale, uint dwCmpFlags, string lpString1, int cchCount1, string lpString2, int cchCount2);        
    }
}