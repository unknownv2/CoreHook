using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace CoreHook
{
    /// <summary>
    /// This class will provide various static members to be used with local hooking and
    /// is also the instance class of a hook.
    /// </summary>
    public partial class LocalHook : CriticalFinalizerObject, IDisposable
    {
        private object _threadSafe = new object();
        private IntPtr _handle = IntPtr.Zero;
        private GCHandle _selfHandle;
        private Delegate _hookProc;
        private object _callback;
        private HookAccessControl _threadACL;
        private static HookAccessControl _globalThreadACL = new HookAccessControl(IntPtr.Zero);

        /// <summary>
        /// Ensures that each instance is always terminated with <see cref="Dispose"/>.
        /// </summary>
        ~LocalHook()
        {
            Dispose();
        }

        private LocalHook() { }

        /// <summary>
        /// The callback passed to <see cref="Create"/>.
        /// </summary>
        public object Callback { get { return _callback; } }

        /// <summary>
        /// Returns the thread ACL associated with this hook. Refer to <see cref="IsThreadIntercepted"/>
        /// for more information about access negotiation.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// The underlying hook is already disposed.
        /// </exception>
        public HookAccessControl ThreadACL
        {
            get
            {
                if (IntPtr.Zero == _handle)
                    throw new ObjectDisposedException(typeof(LocalHook).FullName);

                return _threadACL;
            }
        }

        /// <summary>
        /// Returns the trampoline bypass address associated with this hook.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// The underlying hook has been disposed.
        /// </exception>
        public IntPtr HookBypassAddress
        {
            get
            {
                if (IntPtr.Zero == _handle)
                {
                    throw new ObjectDisposedException(typeof(LocalHook).FullName);
                }

                IntPtr address = IntPtr.Zero;
                NativeAPI.DetourGetHookBypassAddress(_handle, out address);
                return address;
            }
        }

        /// <summary>
        /// Checks whether a given thread ID will be intercepted by the underlying hook.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method provides an interface to the internal negotiation algorithm.
        /// You may use it to check whether your ACL provides expected results.
        /// </para><para>
        /// The following is a pseudo code of how this method is implemented:
        /// <code>
        /// if(InThreadID == 0)
        ///     InThreadID = GetCurrentThreadId();
        /// 
        /// if(GlobalACL.Contains(InThreadID))
        /// {
        ///     if(LocalACL.Contains(InThreadID))
        /// 	{
        /// 		if(LocalACL.IsExclusive)
        /// 			return false;
        /// 	}
        /// 	else
        /// 	{
        /// 		if(GlobalACL.IsExclusive)
        /// 			return false;
        /// 
        /// 		if(!LocalACL.IsExclusive)
        /// 			return false;
        /// 	}
        /// }
        /// else
        /// {
        /// 	if(LocalACL.Contains(InThreadID))
        /// 	{
        /// 		if(LocalACL.IsExclusive)
        /// 			return false;
        /// 	}
        /// 	else
        /// 	{
        /// 		if(!GlobalACL.IsExclusive)
        /// 			return false;
        /// 
        /// 		if(!LocalACL.IsExclusive)
        /// 			return false;
        /// 	}
        /// }
        /// 
        /// return true;
        /// </code>
        /// </para>
        /// </remarks>
        /// <param name="InThreadID">A native OS thread ID; or zero if you want to check the current thread.</param>
        /// <returns><c>true</c> if the thread is intercepted, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">
        /// The underlying hook is already disposed.
        /// </exception>
        public bool IsThreadIntercepted(int InThreadID)
        {
            bool Result;

            if (IntPtr.Zero == _handle)
            {
                throw new ObjectDisposedException(typeof(LocalHook).FullName);
            }

            NativeAPI.DetourIsThreadIntercepted(_handle, InThreadID, out Result);

            return Result;
        }

        /// <summary>
        /// Returns the gloabl thread ACL associated with ALL hooks. Refer to <see cref="IsThreadIntercepted"/>
        /// for more information about access negotiation.
        /// </summary>
        public static HookAccessControl GlobalThreadACL { get { return _globalThreadACL; } }

        /// <summary>
        /// If you want to immediately uninstall a hook, the only way is to dispose it. A disposed
        /// hook is guaranteed to never invoke your handler again but may still consume
        /// memory even for process life-time! 
        /// </summary>
        /// <remarks>
        /// As we are living in a manged world, you don't have to dispose a hook because the next 
        /// garbage collection will do it for you, assuming that your code does not reference it
        /// anymore. But there are times when you want to uninstall it excplicitly, with no delay.
        /// If you dispose a disposed or not installed hook, nothing will happen!
        /// </remarks>
        public void Dispose()
        {
            lock (_threadSafe)
            {
                if (IntPtr.Zero == _handle)
                {
                    return;
                }

                NativeAPI.DetourUninstallHook(_handle);

                Marshal.FreeCoTaskMem(_handle);

                _handle = IntPtr.Zero;
                _callback = null;
                _hookProc = null;

                _selfHandle.Free();
            }
        }

        /// <summary>
        /// Installs a managed hook. After this you'll have to activate it by setting a proper <see cref="ThreadACL"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that not all entry points are hookable! In general methods like <c>CreateFileW</c>
        /// won't cause any trouble. But there might be methods that are not hookable because their
        /// entry point machine code is not eligable to be hooked. You should test all hooks on
        /// common environments like "Windows XP x86/x64 SP2/SP3" and "Windows Vista x86/x64 (SP1)".
        /// This is the only way to ensure that your application will work well on most machines.
        /// </para><para>
        /// Your handler delegate has to use the <see cref="UnmanagedFunctionPointerAttribute"/> and
        /// shall map to the same native method signature, otherwise the application will crash! The best
        /// way is to use predefined delegates used in related P-Invoke implementations usually found with Google.
        /// If you know how to write such native delegates you won't need internet resources of course.
        /// I recommend using C++.NET which allows you to just copy the related windows API to your managed
        /// class and thread it as delegate without any changes. This will also speed up the whole thing
        /// because no unnecessary marshalling is required! C++.NET is also better in most cases because you
        /// may access the whole native windows API from managed code without any effort what significantly eases
        /// writing of hook handlers.
        /// </para>
        /// <para>
        /// The given delegate is automatically prevented from being garbage collected until the hook itself
        /// is collected...
        /// </para>
        /// </remarks>
        /// <param name="InTargetProc">A target entry point that should be hooked.</param>
        /// <param name="InNewProc">A handler with the same signature as the original entry point
        /// that will be invoked for every call that has passed the Fiber Deadlock Barrier and various integrity checks.</param>
        /// <param name="InCallback">An uninterpreted callback that will later be available through <see cref="HookRuntimeInfo.Callback"/>.</param>
        /// <returns>
        /// A handle to the newly created hook.
        /// </returns>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory available to complete the operation. On 64-Bit this may also indicate
        /// that no memory can be allocated within a 31-Bit boundary around the given entry point.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The given function pointer does not map to executable memory (valid machine code) or 
        /// you passed <c>null</c> as delegate.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The given entry point contains machine code that can not be hooked.
        /// </exception>
        /// <exception cref="InsufficientMemoryException">
        /// The maximum amount of hooks has been installed. This is currently set to MAX_HOOK_COUNT (1024).
        /// </exception>
        public static LocalHook Create(
            IntPtr InTargetProc,
            Delegate InNewProc,
            object InCallback)
        {
            LocalHook Result = new LocalHook();

            Result._callback = InCallback;
            Result._hookProc = InNewProc;
            Result._handle = Marshal.AllocCoTaskMem(IntPtr.Size);
            Result._selfHandle = GCHandle.Alloc(Result, GCHandleType.Weak);

            Marshal.WriteIntPtr(Result._handle, IntPtr.Zero);

            try
            {
                NativeAPI.DetourInstallHook(
                    InTargetProc,
                    Marshal.GetFunctionPointerForDelegate(Result._hookProc),
                    GCHandle.ToIntPtr(Result._selfHandle),
                    Result._handle);
            }
            catch (Exception e)
            {
                Marshal.FreeCoTaskMem(Result._handle);
                Result._handle = IntPtr.Zero;

                Result._selfHandle.Free();

                throw e;
            }

            Result._threadACL = new HookAccessControl(Result._handle);

            return Result;
        }

        /// <summary>
        /// Installs an unmanaged hook. After this you'll have to activate it by setting a proper <see cref="ThreadACL"/>.
        /// <see cref="HookRuntimeInfo"/> WON'T be supported! Refer to the native "DetourBarrierXxx" APIs to
        /// access unmanaged hook runtime information.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that not all entry points are hookable! In general methods like <c>CreateFileW</c>
        /// won't cause any trouble. But there may be methods that are not hookable because their
        /// entry point machine code is not eligable to be hooked. You should test all hooks on
        /// common environments like "Windows XP x86/x64 SP1/SP2/SP3" and "Windows Vista x86/x64 (SP1)".
        /// This is the only way to ensure that your application will work well on most machines.
        /// </para><para>
        /// Unmanaged hooks will require a native DLL which handles the requests. This way
        /// you will get a high-performance interface, because
        /// a switch from unmanaged to managed code seems to be rather time consuming without doing anything
        /// useful (at least nothing visible); so a hook omitting this switch will be handled one or two
        /// orders of magnitudes faster until finally your handler gains execution. But as a managed hook is still executed
        /// within at last 1000 nano-seconds, even the "slow" managed implementation will be fast enough in most
        /// cases. With C++.NET you would be able to provide such native high-speed hooks for frequently
        /// called API methods, while still using managed ones for usual API methods, within a single assembly!
        /// A pure unmanaged, empty hook executes in approx. 70 nano-seconds, which is incredible fast
        /// considering the thread deadlock barrier and thread ACL negotiation that are already included in this benchmark!
        /// </para>
        /// </remarks>
        /// <param name="InTargetProc">A target entry point that should be hooked.</param>
        /// <param name="InNewProc">A handler with the same signature as the original entry point
        /// that will be invoked for every call that has passed the Thread Deadlock Barrier and various integrity checks.</param>
        /// <param name="InCallback">An uninterpreted callback that will later be available through <c>DetourBarrierGetCallback()</c>.</param>
        /// <returns>
        /// A handle to the newly created hook.
        /// </returns>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory available to complete the operation. On 64-Bit this may also indicate
        /// that no memory can be allocated within a 31-Bit boundary around the given entry point.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The given function pointer does not map to executable memory (valid machine code) or 
        /// you passed <c>null</c> as delegate.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The given entry point contains machine code that can not be hooked.
        /// </exception>
        /// <exception cref="InsufficientMemoryException">
        /// The maximum amount of hooks has been installed. This is currently set to MAX_HOOK_COUNT (1024).
        /// </exception>
        public static LocalHook CreateUnmanaged(
            IntPtr InTargetProc,
            IntPtr InNewProc,
            IntPtr InCallback)
        {
            LocalHook Result = new LocalHook();

            Result._callback = InCallback;
            Result._handle = Marshal.AllocCoTaskMem(IntPtr.Size);
            Result._selfHandle = GCHandle.Alloc(Result, GCHandleType.Weak);

            Marshal.WriteIntPtr(Result._handle, IntPtr.Zero);

            try
            {
                NativeAPI.DetourInstallHook(
                    InTargetProc,
                    InNewProc,
                    InCallback,
                    Result._handle);
            }
            catch (Exception e)
            {
                Marshal.FreeCoTaskMem(Result._handle);
                Result._handle = IntPtr.Zero;

                Result._selfHandle.Free();

                throw e;
            }

            Result._threadACL = new HookAccessControl(Result._handle);

            return Result;
        }

        /// <summary>
        /// Will return the address for a given DLL export symbol. The specified
        /// module has to be loaded into the current process space and also export
        /// the given method.
        /// </summary>
        /// <remarks>
        /// If you wonder how to get native entry points in a managed environment,
        /// this is the anwser. You will only be able to hook native code from a managed
        /// environment if you have access to a method like this, returning the native
        /// entry point. Please note that you will also hook any managed code, which
        /// of course ultimately relies on the native windows API!
        /// </remarks>
        /// <param name="InModule">A system DLL name like "kernel32.dll" or a full qualified path to any DLL.</param>
        /// <param name="InSymbolName">An exported symbol name like "CreateFileW".</param>
        /// <returns>The entry point for the given API method.</returns>
        /// <exception cref="DllNotFoundException">
        /// The given module is not loaded into the current process.
        /// </exception>
        /// <exception cref="MissingMethodException">
        /// The given module does not export the desired method.
        /// </exception>
        public static IntPtr GetProcAddress(
            string InModule,
            string InSymbolName)
        {
            if(InModule == null)
            {
                throw new ArgumentNullException(InModule);
            }
            if (InSymbolName == null)
            {
                throw new ArgumentNullException(InModule);
            }

            IntPtr Method = NativeAPI.DetourFindFunction(InModule, InSymbolName);
            if (Method == IntPtr.Zero)
            {
                throw new MissingMethodException("The given method does not exist.");
            }

            return Method;
        }

        /// <summary>
        /// Will return a delegate for a given DLL export symbol. The specified
        /// module has to be loaded into the current process space and also export
        /// the given method.
        /// </summary>
        /// <remarks><para>
        /// This method is usually not useful to hook something but it allows you
        /// to dynamically load native API methods into your managed environment instead
        /// of using the static P-Invoke approach provided by <see cref="DllImportAttribute"/>.
        /// </para></remarks>
        /// <typeparam name="TDelegate">A delegate using the <see cref="UnmanagedFunctionPointerAttribute"/> and
        /// exposing the same method signature as the specified native symbol.</typeparam>
        /// <param name="InModule">A system DLL name like "kernel32.dll" or a full qualified path to any DLL.</param>
        /// <param name="InSymbolName">An exported symbol name like "CreateFileW".</param>
        /// <returns>The managed delegate wrapping around the given native symbol.</returns>
        /// <exception cref="DllNotFoundException">
        /// The given module is not loaded into the current process.
        /// </exception>
        /// <exception cref="MissingMethodException">
        /// The given module does not export the given method.
        /// </exception>
        public static TDelegate GetProcDelegate<TDelegate>(
            string InModule,
            string InSymbolName)
        {
            return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(GetProcAddress(InModule, InSymbolName), typeof(TDelegate));
        }

        /// <summary>
        /// Processes any pending hook removals. Warning! This method can be quite slow (1 second) under certain circumstances.
        /// </summary>
        /// <see cref="NativeAPI.DetourWaitForPendingRemovals()"/>
        public static void Release()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            //NativeAPI.DetourWaitForPendingRemovals();
        }
    }
}
