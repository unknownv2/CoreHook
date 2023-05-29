using System;

namespace CoreHook.CoreLoad;

[Flags]
internal enum PluginInitializationState
{
    Failed = 0,
    Loading = 1,
    Initialized = int.MaxValue
}
