using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.CoreLoad.Data
{
    interface IRemoteEntryInfo : IContext
    {
        int HostPID { get; }
        IntPtr UserData { get; }
        int UserDataSize { get; }
    }
}
