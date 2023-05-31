using System;
using System.Runtime.InteropServices;

namespace CoreHook.Extensions;

/// <summary>
/// IntPtr extensions for creating delegates from function pointers.
/// </summary>
public static class PointerExtensions
{
    /// <summary>
    /// Convert a function address to a callable delegate method.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type to cast the function to.</typeparam>
    /// <param name="function">A function address.</param>
    /// <returns>The callable delegate method at <paramref name="function"/>.</returns>
    public static TDelegate ToFunction<TDelegate>(this nint function) where TDelegate : class
    {
        System.Diagnostics.Debug.Assert(typeof(Delegate).IsAssignableFrom(typeof(TDelegate)));

        return Marshal.GetDelegateForFunctionPointer<TDelegate>(function);
    }
}
