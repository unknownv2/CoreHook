using System;

namespace CoreHook
{
    /// <summary>
    /// Interface for a hooking class that manages installs and uninstalls of function detours.
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
        /// <summary>
        /// Address for calling the target function, bypassing the detour function.
        /// </summary>
        IntPtr HookBypassAddress { get; }
        /// <summary>
        /// Class for managing the list of threads that are detoured.
        /// </summary>
        IHookAccessControl ThreadACL { get; }
    }

    /// <summary>
    /// Interface for a hooking class that manages installs and uninstalls of function detours.
    /// It requires a defining a function delegate type for the function that is being detoured.
    /// </summary>
    /// <typeparam name="T">A type representing the target function's delegate or signature.</typeparam>
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
