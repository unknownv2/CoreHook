using System;
using System.Runtime.InteropServices;

namespace CoreHook.Memory.Processes
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
        public readonly static bool Is64Bit = IntPtr.Size == 8;
 
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
