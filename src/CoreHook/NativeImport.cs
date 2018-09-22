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
            IntPtr InEntryPoint,
            IntPtr InHookProc,
            IntPtr InCallback,
            IntPtr OutHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourUninstallHook(IntPtr RefHandle);

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
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetGlobalInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetGlobalExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourIsThreadIntercepted(
                    IntPtr InHandle,
                    int InThreadID,
                    out bool OutResult);

        /*
            The following barrier methods are meant to be used in hook handlers only!

            They will all fail with STATUS_NOT_SUPPORTED if called outside a
            valid hook handler...
        */

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetCallback(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetReturnAddress(out IntPtr OutValue);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetAddressOfReturnAddress(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierBeginStackTrace(out IntPtr OutBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierEndStackTrace(IntPtr OutBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetCallingModule(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierCallStackTrace(IntPtr OutValue, long maxCount, out long outMaxCount);
        /*
            Debug helper API.
        */
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DbgAttachDebugger();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DbgGetThreadIdByHandle(
            IntPtr InThreadHandle,
            out int OutThreadId);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DbgGetProcessIdByHandle(
            IntPtr InProcessHandle,
            out int OutProcessId);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DbgHandleToObjectName(
            IntPtr InNamedHandle,
            IntPtr OutNameBuffer,
            int InBufferSize,
            out int OutRequiredSize);

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
            IntPtr InEntryPoint,
            IntPtr InHookProc,
            IntPtr InCallback,
            IntPtr OutHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourUninstallHook(IntPtr RefHandle);

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
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetGlobalInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourSetGlobalExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourIsThreadIntercepted(
                    IntPtr InHandle,
                    int InThreadID,
                    out bool OutResult);

        /*
            The following barrier methods are meant to be used in hook handlers only!

            They will all fail with STATUS_NOT_SUPPORTED if called outside a
            valid hook handler...
        */

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetCallback(out IntPtr OutValue);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetReturnAddress(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetAddressOfReturnAddress(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierCallStackTrace(IntPtr OutValue, long maxCount, out long outMaxCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierBeginStackTrace(out IntPtr OutBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierEndStackTrace(IntPtr OutBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DetourBarrierGetCallingModule(out IntPtr OutValue);

        /*
            Debug helper API.
        */
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DbgAttachDebugger();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DbgGetThreadIdByHandle(
            IntPtr InThreadHandle,
            out int OutThreadId);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DbgGetProcessIdByHandle(
            IntPtr InProcessHandle,
            out int OutProcessId);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int DbgHandleToObjectName(
            IntPtr InNamedHandle,
            IntPtr OutNameBuffer,
            int InBufferSize,
            out int OutRequiredSize);

        
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
        public static extern void CloseHandle(IntPtr InHandle);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentProcessId();

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr InModule, string InProcName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string InPath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string InPath);

        [DllImport("kernel32.dll")]
        public static extern short RtlCaptureStackBackTrace(
            int InFramesToSkip,
            int InFramesToCapture,
            IntPtr OutBackTrace,
            IntPtr OutBackTraceHash);

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
            if (Is64Bit) NativeAPI_x64.DetourUninstallAllHooks();
            else NativeAPI_x86.DetourUninstallAllHooks();
        }

        public static void DetourInstallHook(
            IntPtr InEntryPoint,
            IntPtr InHookProc,
            IntPtr InCallback,
            IntPtr OutHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourInstallHook(InEntryPoint, InHookProc, InCallback, OutHandle));
            }
            else
            {

                Force(NativeAPI_x86.DetourInstallHook(InEntryPoint, InHookProc, InCallback, OutHandle));
            }          
        }

        public static void DetourUninstallHook(IntPtr RefHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourUninstallHook(RefHandle));
            }
            else
            {
                Force(NativeAPI_x86.DetourUninstallHook(RefHandle));
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
                    IntPtr InHandle,
                    int InThreadID,
                    out bool OutResult)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourIsThreadIntercepted(InHandle, InThreadID, out OutResult));
            }
            else
            {
                Force(NativeAPI_x86.DetourIsThreadIntercepted(InHandle, InThreadID, out OutResult));
            }
        }

        public static void DetourSetInclusiveACL(
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourSetInclusiveACL(InThreadIdList, InThreadCount, InHandle));
            }
            else
            {
                Force(NativeAPI_x86.DetourSetInclusiveACL(InThreadIdList, InThreadCount, InHandle));
            }
        }

        public static void DetourSetExclusiveACL(
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourSetExclusiveACL(InThreadIdList, InThreadCount, InHandle));
            }
            else
            {
                Force(NativeAPI_x86.DetourSetExclusiveACL(InThreadIdList, InThreadCount, InHandle));
            }
        }

        public static void DetourSetGlobalInclusiveACL(
                    int[] InThreadIdList,
                    int InThreadCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourSetGlobalInclusiveACL(InThreadIdList, InThreadCount));
            }
            else
            {
                Force(NativeAPI_x86.DetourSetGlobalInclusiveACL(InThreadIdList, InThreadCount));
            }
        }

        public static void DetourSetGlobalExclusiveACL(
                    int[] InThreadIdList,
                    int InThreadCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourSetGlobalExclusiveACL(InThreadIdList, InThreadCount));
            }
            else
            {
                Force(NativeAPI_x86.DetourSetGlobalExclusiveACL(InThreadIdList, InThreadCount));
            }
        }


        public static void DetourBarrierGetCallingModule(out IntPtr OutValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierGetCallingModule(out OutValue));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierGetCallingModule(out OutValue));
            }
        }

        public static void DetourBarrierGetCallback(out IntPtr OutValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierGetCallback(out OutValue));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierGetCallback(out OutValue));
            }
        }

        public static void DetourBarrierGetReturnAddress(out IntPtr OutValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierGetReturnAddress(out OutValue));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierGetReturnAddress(out OutValue));
            }
        }

        public static void DetourBarrierGetAddressOfReturnAddress(out IntPtr OutValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierGetAddressOfReturnAddress(out OutValue));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierGetAddressOfReturnAddress(out OutValue));
            }
        }

        public static void DetourBarrierBeginStackTrace(out IntPtr OutBackup)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierBeginStackTrace(out OutBackup));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierBeginStackTrace(out OutBackup));
            }
        }

        public static void DetourBarrierCallStackTrace(IntPtr OutBackup, long maxCount, out long outMaxCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierCallStackTrace(OutBackup, maxCount, out outMaxCount));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierCallStackTrace(OutBackup, maxCount, out outMaxCount));
            }

        }
        public static void DetourBarrierEndStackTrace(IntPtr OutBackup)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.DetourBarrierEndStackTrace(OutBackup));
            }
            else
            {
                Force(NativeAPI_x86.DetourBarrierEndStackTrace(OutBackup));
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
