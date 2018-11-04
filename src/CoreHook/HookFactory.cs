using System;

namespace CoreHook
{
    /// <summary>
    /// Class for creating managed and unmanaged hooks.
    /// </summary>
    public static class HookFactory
    {
        /// <summary>
        /// Create a managed hook.
        /// </summary>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IHook CreateHook(IntPtr targetFunction, Delegate detourFunction, object callback)
        {
            return LocalHook.Create(targetFunction, detourFunction as Delegate, callback);
        }

        /// <summary>
        ///  Create an unmanaged hook.
        /// </summary>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <returns></returns>
        public static IHook CreateHook(IntPtr targetFunction, IntPtr detourFunction)
        {
            return LocalHook.CreateUnmanaged(targetFunction, detourFunction, IntPtr.Zero);
        }

        /// <summary>
        /// Create an unmanaged hook
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <returns></returns>
        public static IHook<T> CreateHook<T>(IntPtr targetFunction, IntPtr detourFunction) where T : class
        {
            return LocalHook2<T>.CreateUnmanaged(targetFunction, detourFunction, IntPtr.Zero);
        }

        /// <summary>
        /// Create a managed hook.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IHook<T> CreateHook<T>(IntPtr targetFunction, T detourFunction, object callback = null) where T : class
        {
            return LocalHook2<T>.Create(targetFunction, detourFunction as Delegate, callback);
        }
    }
}
