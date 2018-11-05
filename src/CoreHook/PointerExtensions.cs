using System;
using System.Collections.Generic;
using System.Text;
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
        /// <typeparam name="TDelegate">The delegate type to cast the function to.</typeparam>
        /// <param name="function">A function address.</param>
        /// <returns>The callable delegate method at <paramref name="function"/>.</returns>
        public static TDelegate ToDelegate<TDelegate>(this IntPtr function) where TDelegate : class
        {
            return ToDelegate(function, typeof(TDelegate)) as TDelegate;
        }

        /// <summary>
        /// Convert a function address to a delegate type.
        /// </summary>
        /// <param name="functionPointer">The function address to cast to a delegate.</param>
        /// <param name="functionDelegate">The delegate type to cast the function address to.</param>
        /// <returns>The <paramref name="functionPointer"/> cast to delegate type <paramref name="functionDelegate"/>.</returns>
        public static Delegate ToDelegate(this IntPtr functionPointer, Type functionDelegate)
        {
            return Marshal.GetDelegateForFunctionPointer(functionPointer, functionDelegate);
        }
    }
}
