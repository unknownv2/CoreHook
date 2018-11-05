using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace CoreHook
{
    /// <summary>
    /// Class for creating and managing a function hook.
    /// </summary>
    public class LocalHook : CriticalFinalizerObject, IHook
    {
        protected IntPtr _handle = IntPtr.Zero;
        /// <summary>
        /// ACL used to activate or de-activate a detour for all threads.
        /// </summary>
        protected readonly int[] _defaultThreadACL = new int[0];
        protected Delegate _detourFunction;
        protected object _threadSafe = new object();
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
                    throw new ObjectDisposedException(typeof(LocalHook).FullName);
                }

                return _threadACL;
            }
        }
        /// <summary>
        /// Context object passed in during detour creation to <see cref="LocalHook.Create"/>
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
                    throw new ObjectDisposedException(typeof(LocalHook).FullName);
                }

                NativeAPI.DetourGetHookBypassAddress(_handle, out IntPtr targetFunctionAddress);
                return targetFunctionAddress;
            }
        }

        /// <summary>
        /// Address for calling the target function, bypassing the detour function.
        /// </summary>
        public IntPtr HookBypassAddress => OriginalAddress;

        /// <summary>
        /// Address of the function that is detoured.
        /// </summary>
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

        protected LocalHook() { }

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
        /// Install a managed hook with a managed delegate for the hook handler.
        /// </summary>
        /// <param name="targetFunction">The target function to install the detour at.</param>
        /// <param name="detourFunction">The hook handler which intercepts the target function.</param>
        /// <param name="callback">A context object that will be available for reference inside the detour.</param>
        /// <returns></returns>
        public static LocalHook Create(IntPtr targetFunction, Delegate detourFunction, object callback)
        {
            var hook = new LocalHook
            {
                Callback = callback,
                TargetAddress = targetFunction,
                _detourFunction = detourFunction,
                _handle = Marshal.AllocCoTaskMem(IntPtr.Size)
            };

            hook._selfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

            Marshal.WriteIntPtr(hook._handle, IntPtr.Zero);

            try
            {
                NativeAPI.DetourInstallHook(
                    targetFunction,
                    Marshal.GetFunctionPointerForDelegate(hook._detourFunction),
                    GCHandle.ToIntPtr(hook._selfHandle),
                    hook._handle);
            }
            catch (Exception ex)
            {
                Marshal.FreeCoTaskMem(hook._handle);
                hook._handle = IntPtr.Zero;

                hook._selfHandle.Free();

                throw ex;
            }

            hook._threadACL = new HookAccessControl(hook._handle);

            return hook;
        }

        /// <summary>
        /// Install an unmanaged hook using the pointer to a hook handler.
        /// </summary>
        /// <param name="targetFunction">The target function to install the detour at.</param>
        /// <param name="detourFunction">The hook handler which intercepts the target function.</param>
        /// <param name="callback">A context object that will be available for reference inside the detour.</param>
        /// <returns></returns>
        public static LocalHook CreateUnmanaged(IntPtr targetFunction, IntPtr detourFunction, IntPtr callback)
        {
            var hook = new LocalHook
            {
                Callback = callback,
                TargetAddress = targetFunction,
                _handle = Marshal.AllocCoTaskMem(IntPtr.Size)
            };

            hook._selfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

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

            hook._threadACL = new HookAccessControl(hook._handle);

            return hook;
        }

        /// <summary>
        /// Install a hook handler to a target function using a pointer to a hook handler.
        /// </summary>
        /// <param name="hook">The hooking class that manages the detour.</param>
        /// <param name="targetFunction">The target function to install the detour at.</param>
        /// <param name="detourFunction">The hook handler which intercepts the target function.</param>
        /// <param name="callback">A context object that will be available for reference inside the detour.</param>
        protected static void InstallHook(LocalHook hook, IntPtr targetFunction, IntPtr detourFunction, IntPtr callback)
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

            hook._threadACL = new HookAccessControl(hook._handle);
        }

        private bool _disposed = false;

        /// <summary>
        /// Dispose the hook and uninstall the detour from the target function.
        /// </summary>
        public void Dispose()
        {
            lock (_threadSafe)
            {
                if (!_disposed && _handle != IntPtr.Zero)
                {
                    _disposed = true;

                    // Uninstall the detour
                    NativeAPI.DetourUninstallHook(_handle);

                    Marshal.FreeCoTaskMem(_handle);

                    _handle = IntPtr.Zero;
                    _detourFunction = null;
                    Callback = null;

                    _selfHandle.Free();
                }
            }
        }

        ~LocalHook()
        {
            Dispose();
        }
    }

    /// <summary>
    /// Class for creating and managing a function hook.
    /// </summary>
    /// <typeparam name="T">A type representing the target function's delegate or signature.</typeparam>
    public class LocalHook<T> : LocalHook, IHook<T> where T : class
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

        public bool EnableForCurrentThread => false;

        /// <summary>
        /// Installs an unmanaged hook using the pointer to a hook handler.
        /// </summary>
        /// <param name="targetFunction">The target function to install the detour at.</param>
        /// <param name="detourFunction">The hook handler which intercepts the target function.</param>
        /// <param name="callback">A context object that will be available for reference inside the detour.</param>
        /// <returns></returns>
        public new static LocalHook<T> CreateUnmanaged(IntPtr targetFunction, IntPtr detourFunction, IntPtr callback)
        {
            var hook = new LocalHook<T>
            {
                Callback = callback,
                TargetAddress = targetFunction,
                _handle = Marshal.AllocCoTaskMem(IntPtr.Size)
            };

            hook._selfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

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

            hook._threadACL = new HookAccessControl(hook._handle);

            return hook;
        }

        /// <summary>
        /// Install a managed hook with a managed delegate for the hook handler.
        /// </summary>
        /// <param name="targetFunction">The target function to install the detour at.</param>
        /// <param name="detourFunction">The hook handler which intercepts the target function.</param>
        /// <param name="callback">A context object that will be available for reference inside the detour.</param>
        /// <returns></returns>
        public new static LocalHook<T> Create(IntPtr targetFunction, Delegate detourFunction, object callback)
        {
            var hook = new LocalHook<T>
            {
                Callback = callback,
                TargetAddress = targetFunction,
                _detourFunction = detourFunction,
                _handle = Marshal.AllocCoTaskMem(IntPtr.Size)
            };

            hook._selfHandle = GCHandle.Alloc(hook, GCHandleType.Weak);

            Marshal.WriteIntPtr(hook._handle, IntPtr.Zero);

            try
            {
                NativeAPI.DetourInstallHook(
                    targetFunction,
                    Marshal.GetFunctionPointerForDelegate(hook._detourFunction),
                    GCHandle.ToIntPtr(hook._selfHandle),
                    hook._handle);
            }
            catch (Exception ex)
            {
                Marshal.FreeCoTaskMem(hook._handle);
                hook._handle = IntPtr.Zero;

                hook._selfHandle.Free();

                throw ex;
            }

            hook._threadACL = new HookAccessControl(hook._handle);

            return hook;
        }

        /// <summary>
        /// Determine if a thread is to be intercepted by the user detour.
        /// </summary>
        /// <param name="threadId">Native thread ID to check, or zero for the current thread.</param>
        /// <returns>True if the thread is intercepted.</returns>
        public bool IsThreadIntercepted(int threadId)
        {
            if (_handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException(typeof(LocalHook<T>).FullName);
            }

            NativeAPI.DetourIsThreadIntercepted(_handle, threadId, out bool isThreadIntercepted);

            return isThreadIntercepted;
        }
    }
}
