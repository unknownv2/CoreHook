using System;
using System.Runtime.InteropServices;

namespace CoreHook.CoreLoad
{
    [StructLayout(LayoutKind.Sequential)]
    internal class RemoteEntryInfo : IContext
    {
        public int HostPID { get; }
        public IntPtr UserData { get; }
        public int UserDataSize { get; }
    }
}
