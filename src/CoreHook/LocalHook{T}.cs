using System;
using System.Runtime.InteropServices;

namespace CoreHook;

/// <summary>
/// Class for creating and managing a function hook.
/// </summary>
/// <typeparam name="T">A type representing the target function's delegate or signature.</typeparam>
public class LocalHook<T> : LocalHook, IHook<T> where T : class
{
    /// <summary>
    /// Delegate used to call the original function by bypassing the detour.
    /// </summary>
    public T Original => OriginalAddress.ToFunction<T>();

    /// <summary>
    /// Delegate used to call the target function directly,
    /// where any detour that is activated will be called as well.
    /// </summary>
    public T Target => TargetAddress.ToFunction<T>();

    /// <summary>
    /// Installs an unmanaged hook using the pointer to a hook handler.
    /// </summary>
    /// <param name="targetFunction">The target function to install the detour at.</param>
    /// <param name="detourFunction">The hook handler which intercepts the target function.</param>
    /// <param name="callback">A context object that will be available for reference inside the detour.</param>
    /// <returns>The handle to the function hook.</returns>
    public new static LocalHook<T> CreateUnmanaged(IntPtr targetFunction, IntPtr detourFunction, IntPtr callback)
    {
        var hook = new LocalHook<T>
        {
            Callback = callback,
            TargetAddress = targetFunction,
            Handle = Marshal.AllocCoTaskMem(IntPtr.Size)
        };

        hook.SelfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

        Marshal.WriteIntPtr(hook.Handle, IntPtr.Zero);

        try
        {
            NativeApi.DetourInstallHook(
                targetFunction,
                detourFunction,
                callback,
                hook.Handle);
        }
        catch (Exception e)
        {
            Marshal.FreeCoTaskMem(hook.Handle);
            hook.Handle = IntPtr.Zero;

            hook.SelfHandle.Free();

            throw e;
        }

        hook.AccessControl = new HookAccessControl(hook.Handle);

        return hook;
    }

    /// <summary>
    /// Install a managed hook with a managed delegate for the hook handler.
    /// </summary>
    /// <param name="targetFunction">The target function to install the detour at.</param>
    /// <param name="detourFunction">The hook handler which intercepts the target function.</param>
    /// <param name="callback">A context object that will be available for reference inside the detour.</param>
    /// <returns>The handle to the function hook.</returns>
    public new static LocalHook<T> Create(IntPtr targetFunction, Delegate detourFunction, object callback)
    {
        var hook = new LocalHook<T>
        {
            Callback = callback,
            TargetAddress = targetFunction,
            DetourFunction = detourFunction,
            Handle = Marshal.AllocCoTaskMem(IntPtr.Size)
        };

        hook.SelfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

        Marshal.WriteIntPtr(hook.Handle, IntPtr.Zero);

        try
        {
            NativeApi.DetourInstallHook(
                targetFunction,
                Marshal.GetFunctionPointerForDelegate(hook.DetourFunction),
                GCHandle.ToIntPtr(hook.SelfHandle),
                hook.Handle);
        }
        catch (Exception e)
        {
            Marshal.FreeCoTaskMem(hook.Handle);
            hook.Handle = IntPtr.Zero;

            hook.SelfHandle.Free();

            throw e;
        }

        hook.AccessControl = new HookAccessControl(hook.Handle);

        return hook;
    }

    /// <summary>
    /// Determine if a thread is to be intercepted by the user detour.
    /// </summary>
    /// <param name="threadId">Native thread ID to check, or zero for the current thread.</param>
    /// <returns>True if the thread is intercepted.</returns>
    public bool IsThreadIntercepted(int threadId)
    {
        if (Handle == IntPtr.Zero)
        {
            throw new ObjectDisposedException(typeof(LocalHook<T>).FullName);
        }

        NativeApi.DetourIsThreadIntercepted(Handle, threadId, out bool isThreadIntercepted);

        return isThreadIntercepted;
    }
}
