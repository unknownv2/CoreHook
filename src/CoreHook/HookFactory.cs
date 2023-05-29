using System;

namespace CoreHook;

/// <summary>
/// Factory for creating managed and unmanaged hooks.
/// </summary>
public static class HookFactory
{
    /// <summary>
    /// Create a managed hook.
    /// </summary>
    /// <param name="targetFunction">The target function address that will be detoured.</param>
    /// <param name="detourFunction">The detour function that will be called instead of the function at <paramref name="targetFunction"/>.</param>
    /// <param name="callback">A context object that can be accessed as <see cref="HookRuntimeInfo.Callback"/> by the detour function.</param>
    /// <returns>The handle to the function hook.</returns>
    public static IHook CreateHook(IntPtr targetFunction, Delegate detourFunction, object callback)
    {
        return LocalHook.Create(targetFunction, detourFunction, callback);
    }

    /// <summary>
    /// Create a managed hook.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type representing the detoured function signature.</typeparam>
    /// <param name="targetFunction">The target function address that will be detoured.</param>
    /// <param name="detourFunction">The detour function that will be called instead of the function at <paramref name="targetFunction"/>.</param>
    /// <param name="callback">A context object that can be accessed as <see cref="HookRuntimeInfo.Callback"/> by the detour function.</param>
    /// <returns>The handle to the function hook.</returns>
    public static IHook<TDelegate> CreateHook<TDelegate>(IntPtr targetFunction, TDelegate detourFunction, object callback = null) where TDelegate : class
    {
        return LocalHook<TDelegate>.Create(targetFunction, detourFunction as Delegate, callback);
    }

    /// <summary>
    /// Create an unmanaged hook.
    /// </summary>
    /// <param name="targetFunction">The target function address that will be detoured.</param>
    /// <param name="detourFunction">The detour function that will be called instead of the function at <paramref name="targetFunction"/>.</param>
    /// <param name="callback">An object that is associated with the hook and can be accessed by the detour function.</param>
    /// <returns>The handle to the function hook.</returns>
    public static IHook CreateHook(IntPtr targetFunction, IntPtr detourFunction, IntPtr? callback = null)
    {
        return LocalHook.CreateUnmanaged(targetFunction, detourFunction, callback ?? IntPtr.Zero);
    }

    /// <summary>
    /// Create an unmanaged hook.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type representing the detoured function signature.</typeparam>
    /// <param name="targetFunction">The target function address that will be detoured.</param>
    /// <param name="detourFunction">The detour function that will be called instead of the function at <paramref name="targetFunction"/>.</param>
    /// <param name="callback">An object that is associated with the hook and can be accessed by the detour function.</param>
    /// <returns>The handle to the function hook.</returns>
    public static IHook<TDelegate> CreateHook<TDelegate>(IntPtr targetFunction, IntPtr detourFunction, IntPtr? callback = null) where TDelegate : class
    {
        return LocalHook<TDelegate>.CreateUnmanaged(targetFunction, detourFunction, callback ?? IntPtr.Zero);
    }
}
