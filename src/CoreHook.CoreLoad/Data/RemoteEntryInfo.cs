using System;
using System.Runtime.InteropServices;

namespace CoreHook.CoreLoad.Data
{
    [StructLayout(LayoutKind.Sequential)]
    public class RemoteEntryInfo : IRemoteEntryInfo
    {
        public int HostProcessId { get; set; }
        public IntPtr UserData { get; set; }
        public int UserDataSize { get; set; }
    }
}
