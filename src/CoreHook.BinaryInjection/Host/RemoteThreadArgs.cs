using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection.Host
{
    public struct RemoteThreadArgs
    {
        public int Status;

        public uint ProcFlags;

        public long Result;

        public uint ThreadAttributes;

        public uint CreationFlags;

        public long StackSize;

        public IntPtr StartAddress;

        public IntPtr Params;
    }
}
