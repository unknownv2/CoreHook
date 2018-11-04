using System;
using System.Runtime.InteropServices;

namespace CoreHook.HookTest
{
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
        /// Class for managing the list of threads that are detoured.
        /// </summary>
        IHookAccessControl ThreadACL { get; }
    }

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
            return LocalHook2.CreateUnmanaged(targetFunction, detourFunction, IntPtr.Zero);
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
            return LocalHook2<T>.Create(targetFunction, detourFunction as Delegate, callback);
        }

        public void Dispose()
        {

        }
    }

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
            return LocalHook2.Create(targetFunction, detourFunction as Delegate, callback);
        }

        /// <summary>
        ///  Create an unmanaged hook.
        /// </summary>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <returns></returns>
        public static IHook CreateHook(IntPtr targetFunction, IntPtr detourFunction)
        {
            return LocalHook2.CreateUnmanaged(targetFunction, detourFunction, IntPtr.Zero);
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

    /// <summary>
    /// Class used for managing the thread ACL of a hook.
    /// </summary>
    public class HookAccessControl2 : IHookAccessControl
    {
        private static readonly int[] _defaultThreadACL = new int[0];

        public bool IsExclusive { get; private set; }

        public bool IsInclusive => !IsExclusive;

        private IntPtr _handle;

        private int[] _acl = _defaultThreadACL;

        public HookAccessControl2(IntPtr handle)
        {
            IsExclusive = handle == IntPtr.Zero;
            _handle = handle;
        }

        /// <summary>
        /// Overwrite the current ACL and set an exclusive list of threads.
        /// Exclusive means that all threads in <paramref name="acl"/> are
        /// not intercepted and all other threads are intercepted.
        /// </summary>
        /// <param name="acl">List of threads to exclude from intercepting.</param>
        public void SetExclusiveACL(int[] acl)
        {
            IsExclusive = true;

            _acl = acl == null ? _defaultThreadACL : (int[])acl.Clone();

            if(_handle == IntPtr.Zero)
            {
                NativeAPI.DetourSetGlobalExclusiveACL(_acl, _acl.Length);
            }
            else
            {
                NativeAPI.DetourSetExclusiveACL(_acl, _acl.Length, _handle);
            }
        }

        /// <summary>
        /// Overwrite the current ACL and set an inclusive list of threads.
        /// Inclusive means that all threads in <paramref name="acl"/> are
        /// intercepted and any others not present in the list are not.
        /// </summary>
        /// <param name="acl">A list of threads to that are intercepted.</param>
        public void SetInclusiveACL(int[] acl)
        {
            IsExclusive = false;

            _acl = acl == null ? _defaultThreadACL : (int[])acl.Clone();

            if(_handle == IntPtr.Zero)
            {
                NativeAPI.DetourSetGlobalInclusiveACL(_acl, _acl.Length);
            }
            else
            {
                NativeAPI.DetourSetInclusiveACL(_acl, _acl.Length, _handle);
            }
        }

        /// <summary>
        /// Get a copy of the current thread list for this ACL.
        /// The returned list can be edited without affecting the hook.
        /// </summary>
        /// <returns>A copy of the ACL's thread list.</returns>
        public int[] GetEntries()
        {
            return (int[])_acl.Clone();
        }
    }

    public class LocalHook2 : IHook
    {
        protected IntPtr _handle = IntPtr.Zero;
        protected Delegate _detourFunction;
        protected object _threadSafe = new object();
        /// <summary>
        /// ACL used to activate or de-activate a detour for all threads.
        /// </summary>
        protected readonly int[] _defaultThreadACL = new int[0];
        protected GCHandle _selfHandle;
        protected IHookAccessControl _threadACL;
        /// <summary>
        /// Get the thread ACL handle for this hook.
        /// </summary>
        public IHookAccessControl ThreadACL
        {
            get
            {
                if (_handle == IntPtr.Zero)
                {
                    throw new ObjectDisposedException(typeof(LocalHook2).FullName);
                }

                return _threadACL;
            }
        }
        /// <summary>
        /// Context object passed in during detour creation to <see cref="LocalHook2.Create"/>
        /// </summary>
        public object Callback { get; protected set; }

        /// <summary>
        /// Get the address used to call the original function,
        /// while bypassing the user detour.
        /// </summary>
        public IntPtr OriginalAddress
        {
            get
            {
                if (_handle == IntPtr.Zero)
                {
                    throw new ObjectDisposedException(typeof(LocalHook2).FullName);
                }

                NativeAPI.DetourGetHookBypassAddress(_handle, out IntPtr targetFunctionAddress);
                return targetFunctionAddress;
            }
        }

        public IntPtr TargetAddress { get; protected set; }

        private bool _enabled;

        /// <summary>
        /// Determines if a detour is active or inactive for all threads.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value)
                {
                    ThreadACL.SetExclusiveACL(_defaultThreadACL);
                }
                else
                {
                    ThreadACL.SetInclusiveACL(_defaultThreadACL);
                }
                _enabled = value;
            }
        }

        /// <summary>
        /// Get the address for a function located in a module that is loaded in the current process.
        /// </summary>
        /// <param name="module">A library name like "kernel32.dll" or a full path to the module.</param>
        /// <param name="function">The name of the function, such as "CreateFileW".</param>
        /// <returns>The address of the given function name in the process.</returns>
        public static IntPtr GetProcAddress(string module, string function)
        {
            if (module == null)
            {
                throw new ArgumentNullException(module);
            }
            if (function == null)
            {
                throw new ArgumentNullException(function);
            }

            IntPtr functionAddress = NativeAPI.DetourFindFunction(module, function);
            if (functionAddress == IntPtr.Zero)
            {
                throw new MissingMethodException($"The function {function} in module {module} was not found.");
            }
            return functionAddress;
        }

        /// <summary>
        /// Install an unmanaged hook.
        /// </summary>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static LocalHook2 CreateUnmanaged(IntPtr targetFunction, IntPtr detourFunction, IntPtr callback)
        {
            var hook = new LocalHook2
            {
                Callback = callback,
                TargetAddress = targetFunction,
                _handle = Marshal.AllocCoTaskMem(IntPtr.Size)
            };

            hook._selfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

            InstallHook(hook, targetFunction, detourFunction, callback);

            return hook;
        }

        /// <summary>
        /// Install a managed hook.
        /// </summary>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static LocalHook2 Create(IntPtr targetFunction, Delegate detourFunction, object callback)
        {
            var hook = new LocalHook2
            {
                Callback = callback,
                TargetAddress = targetFunction,
                _detourFunction = detourFunction,
                _handle = Marshal.AllocCoTaskMem(IntPtr.Size)
            };

            hook._selfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

            InstallHook(hook, targetFunction,
                   Marshal.GetFunctionPointerForDelegate(hook._detourFunction),
                   GCHandle.ToIntPtr(hook._selfHandle));

            return hook;
        }

        /// <summary>
        /// Install a hook handler to a target function.
        /// </summary>
        /// <param name="hook"></param>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <param name="callback"></param>
        protected static void InstallHook(LocalHook2 hook, IntPtr targetFunction, IntPtr detourFunction, IntPtr callback)
        {
            Marshal.WriteIntPtr(hook._handle, IntPtr.Zero);

            try
            {
                NativeAPI.DetourInstallHook(
                    targetFunction,
                    detourFunction,
                    callback,
                    hook._handle);
            }
            catch (Exception ex)
            {
                Marshal.FreeCoTaskMem(hook._handle);
                hook._handle = IntPtr.Zero;

                hook._selfHandle.Free();

                throw ex;
            }

            hook._threadACL = new HookAccessControl2(hook._handle);
        }

        /// <summary>
        /// Dispose the hook and uninstall the detour from the target function.
        /// </summary>
        public void Dispose()
        {
            lock (_threadSafe)
            {
                if (_handle != IntPtr.Zero)
                {
                    // Uninstall the detour using it's handle
                    NativeAPI.DetourUninstallHook(_handle);

                    Marshal.FreeCoTaskMem(_handle);

                    _handle = IntPtr.Zero;
                    Callback = null;
                    _detourFunction = null;

                    _selfHandle.Free();
                }
            }
        }

        ~LocalHook2()
        {
            Dispose();
        }
    }

    public class LocalHook2<T> : LocalHook2, IHook<T> where T : class
    {
        /// <summary>
        /// Delegate used to call the original function by bypassing the detour.
        /// </summary>
        public T Original => OriginalAddress.ToDelegate<T>();
        /// <summary>
        /// Delegate used to call the target function directly,
        /// where any detour that is activated will be called as well.
        /// </summary>
        public T Target => TargetAddress.ToDelegate<T>();

        public LocalHook2() { }

        public bool EnableForCurrentThread => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetFunction"></param>
        /// <param name="detourFunction"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public new static LocalHook2<T> CreateUnmanaged(IntPtr targetFunction, IntPtr detourFunction, IntPtr callback)
        {
            var hook = new LocalHook2<T>
            {
                Callback = callback,
                TargetAddress = targetFunction,
                _handle = Marshal.AllocCoTaskMem(IntPtr.Size)
            };

            hook._selfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

            InstallHook(hook, targetFunction, detourFunction, callback);

            return hook;
        }

        /// <summary>
        /// Install a managed hook.
        /// </summary>
        /// <param name="targetFunction">The target function to install the detour at.</param>
        /// <param name="detourFunction">The hook handler which intercepts the target function.</param>
        /// <param name="callback">A context object that will be available for reference inside the detour.</param>
        /// <returns></returns>
        public new static LocalHook2<T> Create(IntPtr targetFunction, Delegate detourFunction, object callback)
        {
            var hook = new LocalHook2<T>
            {
                Callback = callback,
                TargetAddress = targetFunction,
                _detourFunction = detourFunction,
                _handle = Marshal.AllocCoTaskMem(IntPtr.Size)
            };

            hook._selfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

            InstallHook(hook, targetFunction,
                Marshal.GetFunctionPointerForDelegate(hook._detourFunction),
                GCHandle.ToIntPtr(hook._selfHandle));

            return hook;
        }

        /// <summary>
        /// Determine if a thread is to be intercepted by the user detour.
        /// </summary>
        /// <param name="threadId">Native thread ID to check, or zero for the current thread.</param>
        /// <returns>True if the thread is intercepted.</returns>
        public bool IsThreadIntercepted(int threadId)
        {
            if(_handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException(typeof(LocalHook2<T>).FullName);
            }

            NativeAPI.DetourIsThreadIntercepted(_handle, threadId, out bool isThreadIntercepted);

            return isThreadIntercepted;
        }
    }

    /// <summary>
    /// Class that holds context information used within hook handlers.
    /// </summary>
    public class HookRuntimeInfo2
    {
        /// <summary>
        /// Determine if the current thread is within a hook handler.
        /// True if the current method was called from a detoured function.
        /// </summary>
        public static bool IsHandlerContext =>
            NativeAPI.DetourBarrierGetCallback(out IntPtr callback) == NativeAPI.STATUS_SUCCESS;

        /// <summary>
        /// The user callback parameter passed to <see cref="LocalHook2{T}.Create"/>.
        /// </summary>
        public static object Callback => Handle?.Callback;

        /// <summary>
        /// The class handle initially returned from <see cref="LocalHook2{T}.Create"/>.
        /// </summary>
        public static IHook Handle
        {
            get
            {
                NativeAPI.DetourBarrierGetCallback(out IntPtr callback);
                return callback == IntPtr.Zero ? null : (IHook)GCHandle.FromIntPtr(callback).Target;
            }
        }
    }

    /// <summary>
    /// IntPtr extensions for creating delegates from function pointers.
    /// </summary>
    public static class FunctionExtensions
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
