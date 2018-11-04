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

    public interface IHookManager : IDisposable
    {
        IHook CreateHook(IntPtr targetFunction, IntPtr detourFunction);

        IHook<T> CreateHook<T>(IntPtr targetFunction, T detourFunction, object callback) where T : class;
    }

    public class HookManager : IHookManager
    {
        /// <summary>
        /// Create an unmanaged hook.
        /// </summary>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <returns></returns>
        public IHook CreateHook(IntPtr targetFunction, IntPtr detourFunction)
        {
            return LocalHook.CreateUnmanaged(targetFunction, detourFunction, IntPtr.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IHook<T> CreateHook<T>(IntPtr targetFunction, T detourFunction, object callback = null) where T : class
        {
            return LocalHook<T>.Create(targetFunction, detourFunction as Delegate, callback);
        }

        public void Dispose()
        {

        }
    }
}
