using System;
using System.Runtime.InteropServices;

namespace CoreHook.Memory.Processes;

internal static class NativeApi32
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
        uint nDlls,
        IntPtr rlpDlls,
        IntPtr pfCreateProcessA);
}

internal static class NativeApi64
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
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
          ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
          out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
        uint nDlls,
        IntPtr rlpDlls,
        IntPtr pfCreateProcessA);
}

internal static class NativeProcessApi
{
    public static readonly bool Is64Bit = IntPtr.Size == 8;

    public static bool DetourCreateProcessWithDllExA(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
        string lpDllName,
        IntPtr pfCreateProcessA)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourCreateProcessWithDllExA(lpApplicationName,
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
                    pfCreateProcessA);
        }
        else
        {
            return NativeApi32.DetourCreateProcessWithDllExA(lpApplicationName,
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
                    pfCreateProcessA);
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
        string lpDllName,
        IntPtr pfCreateProcessW)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourCreateProcessWithDllExW(lpApplicationName,
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
                    pfCreateProcessW);
        }
        else
        {
            return NativeApi32.DetourCreateProcessWithDllExW(lpApplicationName,
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
                    pfCreateProcessW);
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
        uint nDlls,
        IntPtr rlpDlls,
        IntPtr pfCreateProcessA)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourCreateProcessWithDllsExA(lpApplicationName,
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
                    pfCreateProcessA);
        }
        else
        {
            return NativeApi32.DetourCreateProcessWithDllsExA(lpApplicationName,
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
                    pfCreateProcessA);
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
        ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
        out Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation,
        uint nDlls,
        IntPtr rlpDlls,
        IntPtr pfCreateProcessW)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourCreateProcessWithDllsExW(lpApplicationName,
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
                    pfCreateProcessW);
        }
        else
        {
            return NativeApi32.DetourCreateProcessWithDllsExW(lpApplicationName,
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
                    pfCreateProcessW);
        }
    }
}
