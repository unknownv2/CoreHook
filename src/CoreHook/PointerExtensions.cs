using System;
using System.Runtime.InteropServices;

namespace CoreHook
{
    /// <summary>
    /// IntPtr extensions for creating delegates from function pointers.
    /// </summary>
    public static class PointerExtensions
    {
        /// <summary>
        /// Convert a function address to a callable delegate method.
        /// </summary>
        /// <typeparam name="T">The delegate type to cast the function to.</typeparam>
        /// <param name="function">A function address.</param>
        /// <returns>The callable delegate method at <paramref name="function"/>.</returns>
        public static T ToFunction<T>(this IntPtr function) where T : class
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(function, typeof(T));
        }
    }
}
