using System;

namespace CoreHook.CoreLoad
{
    [Serializable]
    public class ManagedRemoteInfo
    {
        public string ChannelName;
        public object[] UserParams;
        public string UserLibrary;
        public string UserLibraryName;
        public int HostPID;
        public bool RequireStrongName;
    }
}
