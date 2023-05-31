using CoreHook.HookDefinition;
using CoreHook.Native;

using System;
using System.Runtime.InteropServices;

namespace CoreHook.EntryPoint;

/// <summary>
/// Holds context information used within detour handlers.
/// </summary>
public class HookRuntimeInfo
{
    /// <summary>
    /// Determine if the current thread is within a hook handler.
    /// True if the current method was called from a detoured function.
    /// </summary>
    public static bool IsHandlerContext => NativeApi.DetourBarrierGetCallback(out nint _) == NativeApi.STATUS_SUCCESS;

    /// <summary>
    /// The user callback parameter passed to the hook class during creation.
    /// For example: a class handle.
    /// </summary>
    public static object? Callback => Handle?.Callback;

    /// <summary>
    /// The class that manages the function detour.
    /// </summary>
    public static IHook? Handle
    {
        get
        {
            NativeApi.DetourBarrierGetCallback(out nint callback);
            return callback == nint.Zero ? null : GCHandle.FromIntPtr(callback).Target as IHook;
        }
    }
}
