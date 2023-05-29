using System;

namespace CoreHook;

/// <summary>
/// Interface for a hooking class that manages installs and uninstalls of function detours.
/// </summary>
public interface IHook : IDisposable
{
    /// <summary>
    /// Object associated with the detour handle that can be
    /// accessed inside the detour handler, for example a class.
    /// </summary>
    object Callback { get; }
    /// <summary>
    /// Activate or de-activate the detour using the <see cref="ThreadACL"/>.
    /// </summary>
    bool Enabled { get; set; }
    /// <summary>
    /// Address of the function that is detoured.
    /// </summary>
    IntPtr TargetAddress { get; }
    /// <summary>
    /// Address for calling the target function, bypassing the detour function.
    /// </summary>
    IntPtr OriginalAddress { get; }
    /// <summary>
    /// Class for managing the list of threads that are detoured.
    /// </summary>
    IHookAccessControl ThreadACL { get; }
}
