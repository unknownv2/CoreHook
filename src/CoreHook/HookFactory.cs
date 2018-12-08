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
        /// <param name="targetFunction">The target function address that will be detoured.</param>
        /// <param name="detourFunction">The detour function that will be called instead of the function at <paramref name="targetFunction"/>.</param>
        /// <param name="callback">A context object that can be accessed as <see cref="HookRuntimeInfo.Callback"/>.</param>
        /// <returns>The handle to the function hook.</returns>
        public static IHook CreateHook(IntPtr targetFunction, Delegate detourFunction, object callback)
        {
            return LocalHook.Create(targetFunction, detourFunction, callback);
        }

        /// <summary>
        ///  Create an unmanaged hook.
        /// </summary>
        /// <param name="targetFunction">The target function address that will be detoured.</param>
        /// <param name="detourFunction">The detour function that will be called instead of the function at <paramref name="targetFunction"/>.</param>
        /// <returns>The handle to the function hook.</returns>
        public static IHook CreateHook(IntPtr targetFunction, IntPtr detourFunction)
        {
            return LocalHook.CreateUnmanaged(targetFunction, detourFunction, IntPtr.Zero);
        }

        /// <summary>
        /// Create an unmanaged hook
        /// </summary>
        /// <typeparam name="T">The delegate type representing the detoured function signature.</typeparam>
        /// <param name="targetFunction">The target function address that will be detoured.</param>
        /// <param name="detourFunction">The detour function that will be called instead of the function at <paramref name="targetFunction"/>.</param>
        /// <returns>The handle to the function hook.</returns>
        public static IHook<T> CreateHook<T>(IntPtr targetFunction, IntPtr detourFunction) where T : class
        {
            return LocalHook<T>.CreateUnmanaged(targetFunction, detourFunction, IntPtr.Zero);
        }

        /// <summary>
        /// Create a managed hook.
        /// </summary>
        /// <typeparam name="T">The delegate type representing the detoured function signature.</typeparam>
        /// <param name="targetFunction">The target function address that will be detoured.</param>
        /// <param name="detourFunction">The detour function that will be called instead of the function at <paramref name="targetFunction"/>.</param>
        /// <param name="callback">A context object that can be accessed as <see cref="HookRuntimeInfo.Callback"/>.</param>
        /// <returns>The handle to the function hook.</returns>
        public static IHook<T> CreateHook<T>(IntPtr targetFunction, T detourFunction, object callback = null) where T : class
        {
            return LocalHook<T>.Create(targetFunction, detourFunction as Delegate, callback);
        }
    }
}
