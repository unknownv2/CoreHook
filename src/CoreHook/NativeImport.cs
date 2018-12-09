using System;
using System.Runtime.InteropServices;

namespace CoreHook
{
    internal static class NativeApi32
    {
        private const string DllName = "corehook32";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern string RtlGetLastErrorStringCopy();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RtlGetLastError();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void DetourUninstallAllHooks();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourInstallHook(
            IntPtr entryPoint,
            IntPtr hookProcedure,
            IntPtr callback,
            IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourUninstallHook(IntPtr refHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourWaitForPendingRemovals();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetGlobalInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] threadIdList,
                    int threadCount);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetGlobalExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] threadIdList,
                    int threadCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourIsThreadIntercepted(
                    IntPtr handle,
                    int threadId,
                    out bool result);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetCallback(out IntPtr returnValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetReturnAddress(out IntPtr returnValue);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetAddressOfReturnAddress(out IntPtr returnValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierBeginStackTrace(out IntPtr backup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierEndStackTrace(IntPtr backup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetCallingModule(out IntPtr returnValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierCallStackTrace(
            IntPtr returnValue, long maxCount, out long maxStackTraceCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int DetourGetHookBypassAddress(IntPtr handle, out IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern bool DetourCreateProcessWithDllExW(string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool DetourCreateProcessWithDllExA(string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessW);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern bool DetourCreateProcessWithDllsExW(string lpApplicationName,
              string lpCommandLine,
              IntPtr lpProcessAttributes,
              IntPtr lpThreadAttributes,
              bool bInheritHandles,
              uint dwCreationFlags,
              IntPtr lpEnvironment,
              string lpCurrentDirectory,
              IntPtr lpStartupInfo,
              IntPtr lpProcessInformation,
              uint nDlls,
              IntPtr rlpDlls,
              IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool DetourCreateProcessWithDllsExA(string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr DetourFindFunction(
            string lpModule,
            string lpFunction);
    }

    internal static class NativeApi64
    {
        private const string DllName = "corehook64";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern string RtlGetLastErrorStringCopy();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RtlGetLastError();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void DetourUninstallAllHooks();
        
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourInstallHook(
            IntPtr entryPoint,
            IntPtr hookProcedure,
            IntPtr callback,
            IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourUninstallHook(IntPtr refHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourWaitForPendingRemovals();
       
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetGlobalInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] threadIdList,
                    int threadCount);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetGlobalExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] threadIdList,
                    int threadCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourIsThreadIntercepted(
                    IntPtr handle,
                    int threadId,
                    out bool result);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetCallback(out IntPtr returnValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetReturnAddress(out IntPtr returnValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetAddressOfReturnAddress(out IntPtr returnValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierCallStackTrace(IntPtr returnValue, long maxCount, out long outMaxCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierBeginStackTrace(out IntPtr backup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierEndStackTrace(IntPtr backup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetCallingModule(out IntPtr returnValue);
          
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int DetourGetHookBypassAddress(IntPtr handle, out IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern bool DetourCreateProcessWithDllExW(string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool DetourCreateProcessWithDllExA(string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern bool DetourCreateProcessWithDllsExW(string lpApplicationName,
              string lpCommandLine,
              IntPtr lpProcessAttributes,
              IntPtr lpThreadAttributes,
              bool bInheritHandles,
              uint dwCreationFlags,
              IntPtr lpEnvironment,
              string lpCurrentDirectory,
              IntPtr lpStartupInfo,
              IntPtr lpProcessInformation,
              uint nDlls,
              IntPtr rlpDlls,
              IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool DetourCreateProcessWithDllsExA(string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessW);
    
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr DetourFindFunction(
            string lpModule,
            string lpFunction);
    }

    /// <summary>
    /// APIs for calling into the native detouring module.
    /// </summary>
    public static class NativeApi
    {
        /// <summary>
        /// Determine if the current application is 32 or 64 bit.
        /// </summary>
        public static readonly bool Is64Bit = IntPtr.Size == 8;

        internal const int StatusSuccess = 0;

        internal static void HandleErrorCode(int errorCode)
        {
            switch (errorCode)
            {
                case StatusSuccess:
                {
                    return;
                }
                default:
                {
                    throw new ApplicationException($"Unknown error {errorCode}.");
                }
            }
        }

        public static void DetourUninstallAllHooks()
        {
            if (Is64Bit)
            {
                NativeApi64.DetourUninstallAllHooks();
            }
            else
            {
                NativeApi32.DetourUninstallAllHooks();
            }
        }

        public static void DetourInstallHook(
            IntPtr entryPoint,
            IntPtr hookProcedure,
            IntPtr callback,
            IntPtr handle)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourInstallHook(entryPoint, hookProcedure, callback, handle));
            }
            else
            {

                HandleErrorCode(NativeApi32.DetourInstallHook(entryPoint, hookProcedure, callback, handle));
            }
        }

        public static void DetourUninstallHook(IntPtr refHandle)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourUninstallHook(refHandle));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourUninstallHook(refHandle));
            }
        }


        public static void DetourWaitForPendingRemovals()
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourWaitForPendingRemovals());
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourWaitForPendingRemovals());
            }
        }

        public static void DetourIsThreadIntercepted(
                    IntPtr handle,
                    int threadId,
                    out bool result)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourIsThreadIntercepted(handle, threadId, out result));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourIsThreadIntercepted(handle, threadId, out result));
            }
        }

        public static void DetourSetInclusiveACL(
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourSetInclusiveACL(threadIdList, threadCount, handle));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourSetInclusiveACL(threadIdList, threadCount, handle));
            }
        }

        public static void DetourSetExclusiveACL(
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourSetExclusiveACL(threadIdList, threadCount, handle));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourSetExclusiveACL(threadIdList, threadCount, handle));
            }
        }

        public static void DetourSetGlobalInclusiveACL(
                    int[] threadIdList,
                    int threadCount)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourSetGlobalInclusiveACL(threadIdList, threadCount));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourSetGlobalInclusiveACL(threadIdList, threadCount));
            }
        }

        public static void DetourSetGlobalExclusiveACL(
                    int[] threadIdList,
                    int threadCount)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourSetGlobalExclusiveACL(threadIdList, threadCount));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourSetGlobalExclusiveACL(threadIdList, threadCount));
            }
        }


        public static void DetourBarrierGetCallingModule(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourBarrierGetCallingModule(out returnValue));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourBarrierGetCallingModule(out returnValue));
            }
        }

        public static int DetourBarrierGetCallback(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                return NativeApi64.DetourBarrierGetCallback(out returnValue);
            }
            else
            {
                return NativeApi32.DetourBarrierGetCallback(out returnValue);
            }
        }

        public static void DetourBarrierGetReturnAddress(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourBarrierGetReturnAddress(out returnValue));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourBarrierGetReturnAddress(out returnValue));
            }
        }

        public static void DetourBarrierGetAddressOfReturnAddress(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourBarrierGetAddressOfReturnAddress(out returnValue));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourBarrierGetAddressOfReturnAddress(out returnValue));
            }
        }

        public static void DetourBarrierBeginStackTrace(out IntPtr backup)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourBarrierBeginStackTrace(out backup));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourBarrierBeginStackTrace(out backup));
            }
        }

        public static void DetourBarrierCallStackTrace(IntPtr backup, long maxCount, out long outMaxCount)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourBarrierCallStackTrace(backup, maxCount, out outMaxCount));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourBarrierCallStackTrace(backup, maxCount, out outMaxCount));
            }

        }
        public static void DetourBarrierEndStackTrace(IntPtr backup)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourBarrierEndStackTrace(backup));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourBarrierEndStackTrace(backup));
            }
        }

        public static void DetourGetHookBypassAddress(IntPtr handle, out IntPtr address)
        {
            if (Is64Bit)
            {
                HandleErrorCode(NativeApi64.DetourGetHookBypassAddress(handle, out address));
            }
            else
            {
                HandleErrorCode(NativeApi32.DetourGetHookBypassAddress(handle, out address));
            }

        }

        public static bool DetourCreateProcessWithDllExA(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessW)
        {
            if (Is64Bit)
            {
                return (NativeApi64.DetourCreateProcessWithDllExA(
                        lpApplicationName,
                        lpCommandLine,
                        lpProcessAttributes,
                        lpThreadAttributes,
                        bInheritHandles,
                        dwCreationFlags,
                        lpEnvironment,
                        lpCurrentDirectory,
                        lpStartupInfo,
                        lpProcessInformation,
                        lpDllName,
                        pfCreateProcessW));
            }
            else
            {
                return (NativeApi32.DetourCreateProcessWithDllExA(
                        lpApplicationName,
                        lpCommandLine,
                        lpProcessAttributes,
                        lpThreadAttributes,
                        bInheritHandles,
                        dwCreationFlags,
                        lpEnvironment,
                        lpCurrentDirectory,
                        lpStartupInfo,
                        lpProcessInformation,
                        lpDllName,
                        pfCreateProcessW));
            }
        }
        public static bool DetourCreateProcessWithDllExW(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessW)
        {
            if (Is64Bit)
            {
                return (NativeApi64.DetourCreateProcessWithDllExW(
                        lpApplicationName,
                        lpCommandLine,
                        lpProcessAttributes,
                        lpThreadAttributes,
                        bInheritHandles,
                        dwCreationFlags,
                        lpEnvironment,
                        lpCurrentDirectory,
                        lpStartupInfo,
                        lpProcessInformation,
                        lpDllName,
                        pfCreateProcessW));
            }
            else
            {
                return (NativeApi32.DetourCreateProcessWithDllExW(
                        lpApplicationName,
                        lpCommandLine,
                        lpProcessAttributes,
                        lpThreadAttributes,
                        bInheritHandles,
                        dwCreationFlags,
                        lpEnvironment,
                        lpCurrentDirectory,
                        lpStartupInfo,
                        lpProcessInformation,
                        lpDllName,
                        pfCreateProcessW));
            }
        }
        public static bool DetourCreateProcessWithDllsExA(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessW)
        {
            if (Is64Bit)
            {
                return (NativeApi64.DetourCreateProcessWithDllsExA(
                        lpApplicationName,
                        lpCommandLine,
                        lpProcessAttributes,
                        lpThreadAttributes,
                        bInheritHandles,
                        dwCreationFlags,
                        lpEnvironment,
                        lpCurrentDirectory,
                        lpStartupInfo,
                        lpProcessInformation,
                        nDlls,
                        rlpDlls,
                        pfCreateProcessW));
            }
            else
            {
                return (NativeApi32.DetourCreateProcessWithDllsExA(
                        lpApplicationName,
                        lpCommandLine,
                        lpProcessAttributes,
                        lpThreadAttributes,
                        bInheritHandles,
                        dwCreationFlags,
                        lpEnvironment,
                        lpCurrentDirectory,
                        lpStartupInfo,
                        lpProcessInformation,
                        nDlls,
                        rlpDlls,
                        pfCreateProcessW));
            }
        }
        public static bool DetourCreateProcessWithDllsExW(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            IntPtr lpStartupInfo,
            IntPtr lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessW)
        {
            if (Is64Bit)
            {
                return (NativeApi64.DetourCreateProcessWithDllsExW(
                        lpApplicationName,
                        lpCommandLine,
                        lpProcessAttributes,
                        lpThreadAttributes,
                        bInheritHandles,
                        dwCreationFlags,
                        lpEnvironment,
                        lpCurrentDirectory,
                        lpStartupInfo,
                        lpProcessInformation,
                        nDlls,
                        rlpDlls,
                        pfCreateProcessW));
            }
            else
            {
                return (NativeApi32.DetourCreateProcessWithDllsExW(
                        lpApplicationName,
                        lpCommandLine,
                        lpProcessAttributes,
                        lpThreadAttributes,
                        bInheritHandles,
                        dwCreationFlags,
                        lpEnvironment,
                        lpCurrentDirectory,
                        lpStartupInfo,
                        lpProcessInformation,
                        nDlls,
                        rlpDlls,
                        pfCreateProcessW));
            }
        }
        public static IntPtr DetourFindFunction(
            string lpModule,
            string lpFunction)
        {
            if (Is64Bit)
            {
                return (NativeApi64.DetourFindFunction(lpModule,
                    lpFunction));
            }
            else
            {
                return (NativeApi32.DetourFindFunction(lpModule,
                    lpFunction));
            }
        }
    }
}
