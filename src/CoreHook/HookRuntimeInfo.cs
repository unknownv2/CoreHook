using System;
using System.Runtime.InteropServices;

namespace CoreHook
{
    /// <summary>
    /// Class that holds context information used within hook handlers.
    /// </summary>
    public class HookRuntimeInfo
    {
        /// <summary>
        /// Determine if the current thread is within a hook handler.
        /// True if the current method was called from a detoured function.
        /// </summary>
        public static bool IsHandlerContext =>
            NativeAPI.DetourBarrierGetCallback(out IntPtr callback) == NativeAPI.STATUS_SUCCESS;

        /// <summary>
        /// The user callback parameter passed to <see cref="LocalHook{T}.Create"/>.
        /// </summary>
        public static object Callback => Handle?.Callback;

        /// <summary>
        /// The class handle initially returned from <see cref="LocalHook{T}.Create"/>.
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
