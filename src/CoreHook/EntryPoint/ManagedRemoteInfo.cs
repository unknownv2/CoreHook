using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CoreHook.EntryPoint;

[StructLayout(LayoutKind.Sequential)]
public struct ManagedRemoteInfo
{
    public int RemoteProcessId;

    public string ChannelName;

    public string UserLibrary;

    public string UserLibraryName;

    public object?[] UserParams;

    public string?[]? UserParamsTypeNames;

    //TODO: use a typed userParams object to avoid losing null object types?
    public ManagedRemoteInfo(int remoteProcessId, string channelName, string userLibrary, params object?[] userParams)
    {
        ChannelName = channelName;
        UserLibrary = userLibrary;
        UserLibraryName = AssemblyName.GetAssemblyName(userLibrary).FullName;
        RemoteProcessId = remoteProcessId;
        UserParams = userParams;
        UserParamsTypeNames = userParams?.Select(param => param?.GetType().AssemblyQualifiedName).ToArray();
    }
}
