using System;
using System.Runtime.InteropServices;

namespace CoreHook
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHook : IDisposable
    {
        /// <summary>
        /// Object associated with the detour handle that can be
        /// accessed inside the detour handler, for example a class.
        /// </summary>
        object Callback { get; }
        /// <summary>
        /// Activate or de-activate the detour using the <see cref="ThreadACL"/>.
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// Address of the function that is detoured.
        /// </summary>
        IntPtr TargetAddress { get; }
        /// <summary>
        /// Address for calling the target function, bypassing the detour function.
        /// </summary>
        IntPtr OriginalAddress { get; }
        IntPtr HookBypassAddress { get; }
        /// <summary>
        /// Class for managing the list of threads that are detoured.
        /// </summary>
        IHookAccessControl ThreadACL { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHook<T> : IHook where T : class
    { 
        /// <summary>
        /// The original function address, called to bypass the detour.
        /// </summary>
        T Original { get; }
        /// <summary>
        /// The target address where the detour is installed to.
        /// </summary>
        T Target { get; }
    }
}
