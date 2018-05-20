using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.ManagedHook
{
    internal static class NativeAPI
    {
        public readonly static Boolean Is64Bit = IntPtr.Size == 8;

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentProcessId();
    }
}
