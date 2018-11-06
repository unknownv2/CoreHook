using System;
using System.Runtime.InteropServices;

namespace CoreHook
{
    internal static class NativeAPI32
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


        /*
            Setup the ACLs after hook installation. Please note that every
            hook starts suspended. You will have to set a proper ACL to
            make it active!
        */
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

        /*
            The following barrier methods are meant to be used in hook handlers only!

            They will all fail with STATUS_NOT_SUPPORTED if called outside a
            valid hook handler...
        */

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

    internal static class NativeAPI64
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
        
        /*
            Setup the ACLs after hook installation. Please note that every
            hook starts suspended. You will have to set a proper ACL to
            make it active!
        */
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

        /*
            The following barrier methods are meant to be used in hook handlers only!

            They will all fail with STATUS_NOT_SUPPORTED if called outside a
            valid hook handler...
        */

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

    public static class NativeAPI
    {
        public readonly static bool Is64Bit = IntPtr.Size == 8;

        [DllImport("kernel32.dll")]
        public static extern short RtlCaptureStackBackTrace(
            int framesToSkip,
            int framesToCapture,
            IntPtr backTrace,
            IntPtr backTraceHash);

        public const int STATUS_SUCCESS = unchecked((int)0);
        public const int STATUS_INVALID_PARAMETER = unchecked((int)0xC000000DL);
        public const int STATUS_INVALID_PARAMETER_1 = unchecked((int)0xC00000EFL);
        public const int STATUS_INVALID_PARAMETER_2 = unchecked((int)0xC00000F0L);
        public const int STATUS_INVALID_PARAMETER_3 = unchecked((int)0xC00000F1L);
        public const int STATUS_INVALID_PARAMETER_4 = unchecked((int)0xC00000F2L);
        public const int STATUS_INVALID_PARAMETER_5 = unchecked((int)0xC00000F3L);
        public const int STATUS_NOT_SUPPORTED = unchecked((int)0xC00000BBL);

        public const int STATUS_INTERNAL_ERROR = unchecked((int)0xC00000E5L);
        public const int STATUS_INSUFFICIENT_RESOURCES = unchecked((int)0xC000009AL);
        public const int STATUS_BUFFER_TOO_SMALL = unchecked((int)0xC0000023L);
        public const int STATUS_NO_MEMORY = unchecked((int)0xC0000017L);
        public const int STATUS_WOW_ASSERTION = unchecked((int)0xC0009898L);
        public const int STATUS_ACCESS_DENIED = unchecked((int)0xC0000022L);

        private static bool IsArchitectureArm()
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            return arch == Architecture.Arm || arch == Architecture.Arm64;
        }

        private static string ComposeString()
        {
            return "ERROR";
        }

        internal static void Force(int errorCode)
        {
            switch (errorCode)
            {
                case STATUS_SUCCESS: return;
                case STATUS_INVALID_PARAMETER: throw new ArgumentException("STATUS_INVALID_PARAMETER: " + ComposeString());
                case STATUS_INVALID_PARAMETER_1: throw new ArgumentException("STATUS_INVALID_PARAMETER_1: " + ComposeString());
                case STATUS_INVALID_PARAMETER_2: throw new ArgumentException("STATUS_INVALID_PARAMETER_2: " + ComposeString());
                case STATUS_INVALID_PARAMETER_3: throw new ArgumentException("STATUS_INVALID_PARAMETER_3: " + ComposeString());
                case STATUS_INVALID_PARAMETER_4: throw new ArgumentException("STATUS_INVALID_PARAMETER_4: " + ComposeString());
                case STATUS_INVALID_PARAMETER_5: throw new ArgumentException("STATUS_INVALID_PARAMETER_5: " + ComposeString());
                case STATUS_NOT_SUPPORTED: throw new NotSupportedException("STATUS_NOT_SUPPORTED: " + ComposeString());
                case STATUS_INTERNAL_ERROR: throw new ApplicationException("STATUS_INTERNAL_ERROR: " + ComposeString());
                case STATUS_INSUFFICIENT_RESOURCES: throw new InsufficientMemoryException("STATUS_INSUFFICIENT_RESOURCES: " + ComposeString());
                case STATUS_BUFFER_TOO_SMALL: throw new ArgumentException("STATUS_BUFFER_TOO_SMALL: " + ComposeString());
                case STATUS_NO_MEMORY: throw new OutOfMemoryException("STATUS_NO_MEMORY: " + ComposeString());
                case STATUS_WOW_ASSERTION: throw new OutOfMemoryException("STATUS_WOW_ASSERTION: " + ComposeString());
                case STATUS_ACCESS_DENIED: throw new AccessViolationException("STATUS_ACCESS_DENIED: " + ComposeString());

                default: throw new ApplicationException("Unknown error code (" + errorCode + "): " + ComposeString());
            }
        }

        public static int RtlGetLastError()
        {
            if (Is64Bit)
            {
                return NativeAPI64.RtlGetLastError();
            }
            else
            {
                return NativeAPI32.RtlGetLastError();
            }
        }

        public static void DetourUninstallAllHooks()
        {
            if (Is64Bit)
            {
                NativeAPI64.DetourUninstallAllHooks();
            }
            else
            {
                NativeAPI32.DetourUninstallAllHooks();
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
                Force(NativeAPI64.DetourInstallHook(entryPoint, hookProcedure, callback, handle));
            }
            else
            {

                Force(NativeAPI32.DetourInstallHook(entryPoint, hookProcedure, callback, handle));
            }          
        }

        public static void DetourUninstallHook(IntPtr refHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourUninstallHook(refHandle));
            }
            else
            {
                Force(NativeAPI32.DetourUninstallHook(refHandle));
            }
        }


        public static void DetourWaitForPendingRemovals()
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourWaitForPendingRemovals());
            }
            else
            {
                Force(NativeAPI32.DetourWaitForPendingRemovals());
            }
        }

        public static void DetourIsThreadIntercepted(
                    IntPtr handle,
                    int threadId,
                    out bool result)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourIsThreadIntercepted(handle, threadId, out result));
            }
            else
            {
                Force(NativeAPI32.DetourIsThreadIntercepted(handle, threadId, out result));
            }
        }

        public static void DetourSetInclusiveACL(
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourSetInclusiveACL(threadIdList, threadCount, handle));
            }
            else
            {
                Force(NativeAPI32.DetourSetInclusiveACL(threadIdList, threadCount, handle));
            }
        }

        public static void DetourSetExclusiveACL(
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourSetExclusiveACL(threadIdList, threadCount, handle));
            }
            else
            {
                Force(NativeAPI32.DetourSetExclusiveACL(threadIdList, threadCount, handle));
            }
        }

        public static void DetourSetGlobalInclusiveACL(
                    int[] threadIdList,
                    int threadCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourSetGlobalInclusiveACL(threadIdList, threadCount));
            }
            else
            {
                Force(NativeAPI32.DetourSetGlobalInclusiveACL(threadIdList, threadCount));
            }
        }

        public static void DetourSetGlobalExclusiveACL(
                    int[] threadIdList,
                    int threadCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourSetGlobalExclusiveACL(threadIdList, threadCount));
            }
            else
            {
                Force(NativeAPI32.DetourSetGlobalExclusiveACL(threadIdList, threadCount));
            }
        }


        public static void DetourBarrierGetCallingModule(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourBarrierGetCallingModule(out returnValue));
            }
            else
            {
                Force(NativeAPI32.DetourBarrierGetCallingModule(out returnValue));
            }
        }

        public static int DetourBarrierGetCallback(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                return NativeAPI64.DetourBarrierGetCallback(out returnValue);
            }
            else
            {
                return NativeAPI32.DetourBarrierGetCallback(out returnValue);
            }
        }

        public static void DetourBarrierGetReturnAddress(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourBarrierGetReturnAddress(out returnValue));
            }
            else
            {
                Force(NativeAPI32.DetourBarrierGetReturnAddress(out returnValue));
            }
        }

        public static void DetourBarrierGetAddressOfReturnAddress(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourBarrierGetAddressOfReturnAddress(out returnValue));
            }
            else
            {
                Force(NativeAPI32.DetourBarrierGetAddressOfReturnAddress(out returnValue));
            }
        }

        public static void DetourBarrierBeginStackTrace(out IntPtr backup)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourBarrierBeginStackTrace(out backup));
            }
            else
            {
                Force(NativeAPI32.DetourBarrierBeginStackTrace(out backup));
            }
        }

        public static void DetourBarrierCallStackTrace(IntPtr backup, long maxCount, out long outMaxCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourBarrierCallStackTrace(backup, maxCount, out outMaxCount));
            }
            else
            {
                Force(NativeAPI32.DetourBarrierCallStackTrace(backup, maxCount, out outMaxCount));
            }

        }
        public static void DetourBarrierEndStackTrace(IntPtr backup)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourBarrierEndStackTrace(backup));
            }
            else
            {
                Force(NativeAPI32.DetourBarrierEndStackTrace(backup));
            }
        }

        public static void DetourGetHookBypassAddress(IntPtr handle, out IntPtr address)
        {
            if (Is64Bit)
            {
                Force(NativeAPI64.DetourGetHookBypassAddress(handle, out address));
            }
            else
            {
                Force(NativeAPI32.DetourGetHookBypassAddress(handle, out address));
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
                return (NativeAPI64.DetourCreateProcessWithDllExA(
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
                return (NativeAPI32.DetourCreateProcessWithDllExA(
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
                return (NativeAPI64.DetourCreateProcessWithDllExW(
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
                return (NativeAPI32.DetourCreateProcessWithDllExW(
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
                return (NativeAPI64.DetourCreateProcessWithDllsExA(
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
                return (NativeAPI32.DetourCreateProcessWithDllsExA(
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
                return (NativeAPI64.DetourCreateProcessWithDllsExW(
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
                return (NativeAPI32.DetourCreateProcessWithDllsExW(
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
                return (NativeAPI64.DetourFindFunction(lpModule,
                    lpFunction));
            }
            else
            {
                return (NativeAPI32.DetourFindFunction(lpModule,
                    lpFunction));
            }
        }
    }
}
