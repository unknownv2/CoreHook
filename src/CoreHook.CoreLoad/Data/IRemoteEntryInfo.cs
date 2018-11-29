using System;

namespace CoreHook.CoreLoad.Data
{
    interface IRemoteEntryInfo : IContext
    {
        int HostProcessId { get; }
        IntPtr UserData { get; }
        int UserDataSize { get; }
    }
}
