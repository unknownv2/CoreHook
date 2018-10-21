using System;
using System.Runtime.InteropServices;

namespace CoreHook.Memory.Windows
{
    static class NativeAPI_x86
    {
        private const string DllName = "corehook32.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DetourCreateProcessWithDllExW(
            [MarshalAs(UnmanagedType.LPWStr)]string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DetourCreateProcessWithDllExA(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessA);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DetourCreateProcessWithDllsExW(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DetourCreateProcessWithDllsExA(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessA);
    }

    static class NativeAPI_x64
    {
        private const string DllName = "corehook64.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DetourCreateProcessWithDllExW(
            [MarshalAs(UnmanagedType.LPWStr)]string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DetourCreateProcessWithDllExA(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessA);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DetourCreateProcessWithDllsExW(
              [MarshalAs(UnmanagedType.LPWStr)]string lpApplicationName,
              string lpCommandLine,
              IntPtr lpProcessAttributes,
              IntPtr lpThreadAttributes,
              bool bInheritHandles,
              uint dwCreationFlags,
              IntPtr lpEnvironment,
              string lpCurrentDirectory,
              ref NativeMethods.StartupInfo lpStartupInfo,
              out NativeMethods.ProcessInformation lpProcessInformation,
              uint nDlls,
              IntPtr rlpDlls,
              IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DetourCreateProcessWithDllsExA(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessA);
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

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
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
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            string lpDllName,
            IntPtr pfCreateProcessA)
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
                        ref lpStartupInfo,
                        out lpProcessInformation,
                        lpDllName,
                        pfCreateProcessA));
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
                        ref lpStartupInfo,
                        out lpProcessInformation,
                        lpDllName,
                        pfCreateProcessA));
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
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
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
                        ref lpStartupInfo,
                        out lpProcessInformation,
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
                        ref lpStartupInfo,
                        out lpProcessInformation,
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
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessA)
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
                        ref lpStartupInfo,
                        out lpProcessInformation,
                        nDlls,
                        rlpDlls,
                        pfCreateProcessA));
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
                        ref lpStartupInfo,
                        out lpProcessInformation,
                        nDlls,
                        rlpDlls,
                        pfCreateProcessA));
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
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
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
                        ref lpStartupInfo,
                        out lpProcessInformation,
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
                        ref lpStartupInfo,
                        out lpProcessInformation,
                        nDlls,
                        rlpDlls,
                        pfCreateProcessW));
            }
        }
    }
}
