using System;
using System.Runtime.InteropServices;

namespace CoreHook
{
    static class NativeAPI_x86
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

    static class NativeAPI_x64
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
        public const int MAX_HOOK_COUNT = 1024;
        public const int MAX_ACE_COUNT = 128;
        public readonly static bool Is64Bit = IntPtr.Size == 8;

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern void CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentProcessId();

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr module, string procedureName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string path);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string path);

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
            return $"{RtlGetLastErrorString()} (Code: {RtlGetLastError()})";
        }

        internal static void Force(int InErrorCode)
        {
            switch (InErrorCode)
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

                default: throw new ApplicationException("Unknown error code (" + InErrorCode + "): " + ComposeString());
            }
        }

        public static int RtlGetLastError()
        {
            if (Is64Bit)
            {
                return NativeAPI_x64.RtlGetLastError();
            }
            else
            {
                return NativeAPI_x86.RtlGetLastError();
            }
        }

        public static string RtlGetLastErrorString()
        {
            if (Is64Bit)
            {
                return NativeAPI_x64.RtlGetLastErrorStringCopy();
            }
            else
            {
                return NativeAPI_x86.RtlGetLastErrorStringCopy();
            }
        }


        public static void DetourUninstallAllHooks()
        {
            if (Is64Bit)
            {
                NativeAPI_x64.DetourUninstallAllHooks();
            }
            else
            {
                NativeAPI_x86.DetourUninstallAllHooks();
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
                Force(NativeAPI_x64.DetourInstallHook(entryPoint, hookProcedure, callback, handle));
            }
            else
            {

                Force(NativeAPI_x86.DetourInstallHook(entryPoint, hookProcedure, callback, handle));
            }          
        }

        public static void DetourUninstallHook(IntPtr refHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourUninstallHook(refHandle));
            }
            else
            {
                Force(NativeAPI_x86.DetourUninstallHook(refHandle));
            }
        }


        public static void DetourWaitForPendingRemovals()
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourWaitForPendingRemovals());
            }
            else
            {
                Force(NativeAPI_x86.DetourWaitForPendingRemovals());
            }
        }

        public static void DetourIsThreadIntercepted(
                    IntPtr handle,
                    int threadId,
                    out bool result)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourIsThreadIntercepted(handle, threadId, out result));
            }
            else
            {
                Force(NativeAPI_x86.DetourIsThreadIntercepted(handle, threadId, out result));
            }
        }

        public static void DetourSetInclusiveACL(
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourSetInclusiveACL(threadIdList, threadCount, handle));
            }
            else
            {
                Force(NativeAPI_x86.DetourSetInclusiveACL(threadIdList, threadCount, handle));
            }
        }

        public static void DetourSetExclusiveACL(
                    int[] threadIdList,
                    int threadCount,
                    IntPtr handle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourSetExclusiveACL(threadIdList, threadCount, handle));
            }
            else
            {
                Force(NativeAPI_x86.DetourSetExclusiveACL(threadIdList, threadCount, handle));
            }
        }

        public static void DetourSetGlobalInclusiveACL(
                    int[] threadIdList,
                    int threadCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourSetGlobalInclusiveACL(threadIdList, threadCount));
            }
            else
            {
                Force(NativeAPI_x86.DetourSetGlobalInclusiveACL(threadIdList, threadCount));
            }
        }

        public static void DetourSetGlobalExclusiveACL(
                    int[] threadIdList,
                    int threadCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourSetGlobalExclusiveACL(threadIdList, threadCount));
            }
            else
            {
                Force(NativeAPI_x86.DetourSetGlobalExclusiveACL(threadIdList, threadCount));
            }
        }


        public static void DetourBarrierGetCallingModule(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierGetCallingModule(out returnValue));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierGetCallingModule(out returnValue));
            }
        }

        public static void DetourBarrierGetCallback(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierGetCallback(out returnValue));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierGetCallback(out returnValue));
            }
        }

        public static void DetourBarrierGetReturnAddress(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierGetReturnAddress(out returnValue));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierGetReturnAddress(out returnValue));
            }
        }

        public static void DetourBarrierGetAddressOfReturnAddress(out IntPtr returnValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierGetAddressOfReturnAddress(out returnValue));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierGetAddressOfReturnAddress(out returnValue));
            }
        }

        public static void DetourBarrierBeginStackTrace(out IntPtr backup)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierBeginStackTrace(out backup));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierBeginStackTrace(out backup));
            }
        }

        public static void DetourBarrierCallStackTrace(IntPtr backup, long maxCount, out long outMaxCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierCallStackTrace(backup, maxCount, out outMaxCount));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierCallStackTrace(backup, maxCount, out outMaxCount));
            }

        }
        public static void DetourBarrierEndStackTrace(IntPtr backup)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierEndStackTrace(backup));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierEndStackTrace(backup));
            }
        }

        public static void DetourGetHookBypassAddress(IntPtr handle, out IntPtr address)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourGetHookBypassAddress(handle, out address));
            }
            else
            {
                Force(NativeAPI_x86.DetourGetHookBypassAddress(handle, out address));
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
                return (NativeAPI_x64.DetourCreateProcessWithDllExA(
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
                return (NativeAPI_x86.DetourCreateProcessWithDllExA(
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
                return (NativeAPI_x64.DetourCreateProcessWithDllExW(
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
                return (NativeAPI_x86.DetourCreateProcessWithDllExW(
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
                return (NativeAPI_x64.DetourCreateProcessWithDllsExA(
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
                return (NativeAPI_x86.DetourCreateProcessWithDllsExA(
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
                return (NativeAPI_x64.DetourCreateProcessWithDllsExW(
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
                return (NativeAPI_x86.DetourCreateProcessWithDllsExW(
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
                return (NativeAPI_x64.DetourFindFunction(lpModule,
                    lpFunction));
            }
            else
            {
                return (NativeAPI_x86.DetourFindFunction(lpModule,
                    lpFunction));
            }
        }
    }
}
