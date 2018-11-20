using System;

namespace CoreHook.CoreLoad.Data
{
    interface IRemoteEntryInfo : IContext
    {
        int HostPID { get; }
        IntPtr UserData { get; }
        int UserDataSize { get; }
    }
}
