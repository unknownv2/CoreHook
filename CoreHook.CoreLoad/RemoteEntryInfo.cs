using System;
using System.Runtime.InteropServices;

namespace CoreHook.CoreLoad
{
    [StructLayout(LayoutKind.Sequential)]
    class RemoteEntryInfo
    {
        public int m_HostPID;
        public IntPtr UserData;
        public int UserDataSize;

        public int HostPID
        {
            get
            {
                return m_HostPID;
            }
        }
    }
}
