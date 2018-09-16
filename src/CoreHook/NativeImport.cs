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
        public static extern void LhUninstallAllHooks();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhInstallHook(
            IntPtr InEntryPoint,
            IntPtr InHookProc,
            IntPtr InCallback,
            IntPtr OutHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhUninstallHook(IntPtr RefHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhWaitForPendingRemovals();


        /*
            Setup the ACLs after hook installation. Please note that every
            hook starts suspended. You will have to set a proper ACL to
            make it active!
        */
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhSetInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhSetExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhSetGlobalInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhSetGlobalExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhIsThreadIntercepted(
                    IntPtr InHandle,
                    int InThreadID,
                    out bool OutResult);

        /*
            The following barrier methods are meant to be used in hook handlers only!

            They will all fail with STATUS_NOT_SUPPORTED if called outside a
            valid hook handler...
        */

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierGetCallback(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierGetReturnAddress(out IntPtr OutValue);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierGetAddressOfReturnAddress(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierBeginStackTrace(out IntPtr OutBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierEndStackTrace(IntPtr OutBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierGetCallingModule(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierCallStackTrace(IntPtr OutValue, long maxCount, out long outMaxCount);
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


        /*
            Injection support API.
        */
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int RhInjectLibrary(
            int InTargetPID,
            int InWakeUpTID,
            int InInjectionOptions,
            string InLibraryPath_x86,
            string InLibraryPath_x64,
            IntPtr InPassThruBuffer,
            int InPassThruSize);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RhIsX64Process(
            int InProcessId,
            out bool OutResult);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool RhIsAdministrator();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RhGetProcessToken(int InProcessId, out IntPtr OutToken);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int RtlInstallService(
            string InServiceName,
            string InExePath,
            string InChannelName);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RhWakeUpProcess();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int RtlCreateSuspendedProcess(
           string InEXEPath,
           string InCommandLine,
            int InProcessCreationFlags,
           out int OutProcessId,
           out int OutThreadId);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int RhInstallDriver(
           string InDriverPath,
           string InDriverName);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RhInstallSupportDriver();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool RhIsX64System();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int LhGetHookBypassAddress(IntPtr handle, out IntPtr address);

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
        public static extern void LhUninstallAllHooks();
        
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhInstallHook(
            IntPtr InEntryPoint,
            IntPtr InHookProc,
            IntPtr InCallback,
            IntPtr OutHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhUninstallHook(IntPtr RefHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhWaitForPendingRemovals();
        
        /*
            Setup the ACLs after hook installation. Please note that every
            hook starts suspended. You will have to set a proper ACL to
            make it active!
        */
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhSetInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhSetExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhSetGlobalInclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhSetGlobalExclusiveACL(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                    int[] InThreadIdList,
                    int InThreadCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhIsThreadIntercepted(
                    IntPtr InHandle,
                    int InThreadID,
                    out bool OutResult);

        /*
            The following barrier methods are meant to be used in hook handlers only!

            They will all fail with STATUS_NOT_SUPPORTED if called outside a
            valid hook handler...
        */

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierGetCallback(out IntPtr OutValue);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierGetReturnAddress(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierGetAddressOfReturnAddress(out IntPtr OutValue);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierCallStackTrace(IntPtr OutValue, long maxCount, out long outMaxCount);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierBeginStackTrace(out IntPtr OutBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierEndStackTrace(IntPtr OutBackup);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int LhBarrierGetCallingModule(out IntPtr OutValue);

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


        /*
            Injection support API.
        */
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int RhInjectLibrary(
            int InTargetPID,
            int InWakeUpTID,
            int InInjectionOptions,
            string InLibraryPath_x86,
            string InLibraryPath_x64,
            IntPtr InPassThruBuffer,
            int InPassThruSize);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RhIsX64Process(
            int InProcessId,
            out bool OutResult);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool RhIsAdministrator();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RhGetProcessToken(int InProcessId, out IntPtr OutToken);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int RtlInstallService(
            string InServiceName,
            string InExePath,
            string InChannelName);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int RtlCreateSuspendedProcess(
           string InEXEPath,
           string InCommandLine,
            int InProcessCreationFlags,
           out int OutProcessId,
           out int OutThreadId);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RhWakeUpProcess();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int RhInstallDriver(
           string InDriverPath,
           string InDriverName);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int RhInstallSupportDriver();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool RhIsX64System();
        
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int LhGetHookBypassAddress(IntPtr handle, out IntPtr address);

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


        public static void LhUninstallAllHooks()
        {
            if (Is64Bit) NativeAPI_x64.LhUninstallAllHooks();
            else NativeAPI_x86.LhUninstallAllHooks();
        }

        public static void LhInstallHook(
            IntPtr InEntryPoint,
            IntPtr InHookProc,
            IntPtr InCallback,
            IntPtr OutHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhInstallHook(InEntryPoint, InHookProc, InCallback, OutHandle));
            }
            else
            {

                Force(NativeAPI_x86.LhInstallHook(InEntryPoint, InHookProc, InCallback, OutHandle));
            }          
        }

        public static void LhUninstallHook(IntPtr RefHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhUninstallHook(RefHandle));
            }
            else
            {
                Force(NativeAPI_x86.LhUninstallHook(RefHandle));
            }
        }


        public static void LhWaitForPendingRemovals()
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhWaitForPendingRemovals());
            }
            else
            {
                Force(NativeAPI_x86.LhWaitForPendingRemovals());
            }
        }

        public static void LhIsThreadIntercepted(
                    IntPtr InHandle,
                    int InThreadID,
                    out bool OutResult)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhIsThreadIntercepted(InHandle, InThreadID, out OutResult));
            }
            else
            {
                Force(NativeAPI_x86.LhIsThreadIntercepted(InHandle, InThreadID, out OutResult));
            }
        }

        public static void LhSetInclusiveACL(
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhSetInclusiveACL(InThreadIdList, InThreadCount, InHandle));
            }
            else
            {
                Force(NativeAPI_x86.LhSetInclusiveACL(InThreadIdList, InThreadCount, InHandle));
            }
        }

        public static void LhSetExclusiveACL(
                    int[] InThreadIdList,
                    int InThreadCount,
                    IntPtr InHandle)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhSetExclusiveACL(InThreadIdList, InThreadCount, InHandle));
            }
            else
            {
                Force(NativeAPI_x86.LhSetExclusiveACL(InThreadIdList, InThreadCount, InHandle));
            }
        }

        public static void LhSetGlobalInclusiveACL(
                    int[] InThreadIdList,
                    int InThreadCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhSetGlobalInclusiveACL(InThreadIdList, InThreadCount));
            }
            else
            {
                Force(NativeAPI_x86.LhSetGlobalInclusiveACL(InThreadIdList, InThreadCount));
            }
        }

        public static void LhSetGlobalExclusiveACL(
                    int[] InThreadIdList,
                    int InThreadCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhSetGlobalExclusiveACL(InThreadIdList, InThreadCount));
            }
            else
            {
                Force(NativeAPI_x86.LhSetGlobalExclusiveACL(InThreadIdList, InThreadCount));
            }
        }


        public static void LhBarrierGetCallingModule(out IntPtr OutValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhBarrierGetCallingModule(out OutValue));
            }
            else
            {
                Force(NativeAPI_x86.LhBarrierGetCallingModule(out OutValue));
            }
        }

        public static void LhBarrierGetCallback(out IntPtr OutValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhBarrierGetCallback(out OutValue));
            }
            else
            {
                Force(NativeAPI_x86.LhBarrierGetCallback(out OutValue));
            }
        }

        public static void LhBarrierGetReturnAddress(out IntPtr OutValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhBarrierGetReturnAddress(out OutValue));
            }
            else
            {
                Force(NativeAPI_x86.LhBarrierGetReturnAddress(out OutValue));
            }
        }

        public static void LhBarrierGetAddressOfReturnAddress(out IntPtr OutValue)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhBarrierGetAddressOfReturnAddress(out OutValue));
            }
            else
            {
                Force(NativeAPI_x86.LhBarrierGetAddressOfReturnAddress(out OutValue));
            }
        }

        public static void LhBarrierBeginStackTrace(out IntPtr OutBackup)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhBarrierBeginStackTrace(out OutBackup));
            }
            else
            {
                Force(NativeAPI_x86.LhBarrierBeginStackTrace(out OutBackup));
            }
        }

        public static void LhBarrierCallStackTrace(IntPtr OutBackup, long maxCount, out long outMaxCount)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhBarrierCallStackTrace(OutBackup, maxCount, out outMaxCount));
            }
            else
            {
                Force(NativeAPI_x86.LhBarrierCallStackTrace(OutBackup, maxCount, out outMaxCount));
            }

        }
        public static void LhBarrierEndStackTrace(IntPtr OutBackup)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhBarrierEndStackTrace(OutBackup));
            }
            else
            {
                Force(NativeAPI_x86.LhBarrierEndStackTrace(OutBackup));
            }
        }

        public static void LhGetHookBypassAddress(IntPtr handle, out IntPtr address)
        {
            if (Is64Bit)
            {
                Force(NativeAPI_x64.LhGetHookBypassAddress(handle, out address));
            }
            else
            {
                Force(NativeAPI_x86.LhGetHookBypassAddress(handle, out address));
            }

        }
        private static T DoSomething<T>(Func<T> actionWithResult)
        {
            return actionWithResult();
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
                return (NativeAPI_x64.DetourCreateProcessWithDllExA(lpApplicationName,
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
                return (NativeAPI_x86.DetourCreateProcessWithDllExA(lpApplicationName,
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
                return (NativeAPI_x64.DetourCreateProcessWithDllExW(lpApplicationName,
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
                return (NativeAPI_x86.DetourCreateProcessWithDllExW(lpApplicationName,
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
                return (NativeAPI_x64.DetourCreateProcessWithDllsExA(lpApplicationName,
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
                return (NativeAPI_x86.DetourCreateProcessWithDllsExA(lpApplicationName,
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
                return (NativeAPI_x64.DetourCreateProcessWithDllsExW(lpApplicationName,
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
                return (NativeAPI_x86.DetourCreateProcessWithDllsExW(lpApplicationName,
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
