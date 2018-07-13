using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.Unmanaged.Windows
{
    static class NativeAPI_x86
    {
        private const String DllName = "corehook32.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern Boolean DetourCreateProcessWithDllExW(
            [MarshalAs(UnmanagedType.LPWStr)]String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            String lpDllName,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern Boolean DetourCreateProcessWithDllExA(
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            String lpDllName,
            IntPtr pfCreateProcessA);


        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern Boolean DetourCreateProcessWithDllsExW(
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern Boolean DetourCreateProcessWithDllsExA(
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessA);
    }

    static class NativeAPI_x64
    {
        private const String DllName = "corehook64.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern Boolean DetourCreateProcessWithDllExW(
            [MarshalAs(UnmanagedType.LPWStr)]String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            String lpDllName,
            IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern Boolean DetourCreateProcessWithDllExA(
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            String lpDllName,
            IntPtr pfCreateProcessA);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern Boolean DetourCreateProcessWithDllsExW(
              [MarshalAs(UnmanagedType.LPWStr)]String lpApplicationName,
              String lpCommandLine,
              IntPtr lpProcessAttributes,
              IntPtr lpThreadAttributes,
              bool bInheritHandles,
              uint dwCreationFlags,
              IntPtr lpEnvironment,
              String lpCurrentDirectory,
              ref NativeMethods.StartupInfo lpStartupInfo,
              out NativeMethods.ProcessInformation lpProcessInformation,
              uint nDlls,
              IntPtr rlpDlls,
              IntPtr pfCreateProcessW);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern Boolean DetourCreateProcessWithDllsExA(
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            uint nDlls,
            IntPtr rlpDlls,
            IntPtr pfCreateProcessA);
    }

    public static class NativeAPI
    {
        public const Int32 MAX_HOOK_COUNT = 1024;
        public const Int32 MAX_ACE_COUNT = 128;
        public readonly static Boolean Is64Bit = IntPtr.Size == 8;

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern void CloseHandle(IntPtr InHandle);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentProcessId();

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr InModule, String InProcName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(String InPath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(String InPath);

        [DllImport("kernel32.dll")]
        public static extern Int16 RtlCaptureStackBackTrace(
            Int32 InFramesToSkip,
            Int32 InFramesToCapture,
            IntPtr OutBackTrace,
            IntPtr OutBackTraceHash);

        public const Int32 STATUS_SUCCESS = unchecked((Int32)0);
        public const Int32 STATUS_INVALID_PARAMETER = unchecked((Int32)0xC000000DL);
        public const Int32 STATUS_INVALID_PARAMETER_1 = unchecked((Int32)0xC00000EFL);
        public const Int32 STATUS_INVALID_PARAMETER_2 = unchecked((Int32)0xC00000F0L);
        public const Int32 STATUS_INVALID_PARAMETER_3 = unchecked((Int32)0xC00000F1L);
        public const Int32 STATUS_INVALID_PARAMETER_4 = unchecked((Int32)0xC00000F2L);
        public const Int32 STATUS_INVALID_PARAMETER_5 = unchecked((Int32)0xC00000F3L);
        public const Int32 STATUS_NOT_SUPPORTED = unchecked((Int32)0xC00000BBL);

        public const Int32 STATUS_INTERNAL_ERROR = unchecked((Int32)0xC00000E5L);
        public const Int32 STATUS_INSUFFICIENT_RESOURCES = unchecked((Int32)0xC000009AL);
        public const Int32 STATUS_BUFFER_TOO_SMALL = unchecked((Int32)0xC0000023L);
        public const Int32 STATUS_NO_MEMORY = unchecked((Int32)0xC0000017L);
        public const Int32 STATUS_WOW_ASSERTION = unchecked((Int32)0xC0009898L);
        public const Int32 STATUS_ACCESS_DENIED = unchecked((Int32)0xC0000022L);

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
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
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
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            String lpDllName,
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
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref NativeMethods.StartupInfo lpStartupInfo,
            out NativeMethods.ProcessInformation lpProcessInformation,
            String lpDllName,
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
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
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
            String lpApplicationName,
            String lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
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
