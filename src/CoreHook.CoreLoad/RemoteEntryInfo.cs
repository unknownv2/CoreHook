using System;
using System.Runtime.InteropServices;

namespace CoreHook.CoreLoad
{
    [StructLayout(LayoutKind.Sequential)]
    class RemoteEntryInfo
    {
        private int _hostPID;
        public IntPtr UserData;
        public int UserDataSize;

        public int HostPID
        {
            get
            {
                return _hostPID;
            }
        }
    }
}
