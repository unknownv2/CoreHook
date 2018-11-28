using System;
using System.Runtime.InteropServices;

namespace CoreHook
{
    /// <summary>
    /// Holds context information used within detour handlers.
    /// </summary>
    public class HookRuntimeInfo
    {
        /// <summary>
        /// Determine if the current thread is within a hook handler.
        /// True if the current method was called from a detoured function.
        /// </summary>
        public static bool IsHandlerContext =>
            NativeAPI.DetourBarrierGetCallback(out IntPtr _) == NativeAPI.STATUS_SUCCESS;

        /// <summary>
        /// The user callback parameter passed to the hook class during creation.
        /// For example: a class handle.
        /// </summary>
        public static object Callback => Handle?.Callback;

        /// <summary>
        /// The class that manages the function detour.
        /// </summary>
        public static IHook Handle
        {
            get
            {
                NativeAPI.DetourBarrierGetCallback(out IntPtr callback);
                return callback == IntPtr.Zero ? null : GCHandle.FromIntPtr(callback).Target as IHook;
            }
        }
    }
}
