using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RemoteFunctionArgs
    {
        public IntPtr UserData;
        public uint UserDataSize;
    }
}
