using System.Runtime.InteropServices;

namespace CoreHook.Native;

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
    public static extern int DetourInstallHook(nint entryPoint, nint hookProcedure, nint callback, nint handle);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourUninstallHook(nint refHandle);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourWaitForPendingRemovals();

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourSetInclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount, nint handle);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourSetExclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount, nint handle);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourSetGlobalInclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount);


    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourSetGlobalExclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourIsThreadIntercepted(nint handle, int threadId, out bool result);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourBarrierGetCallback(out nint returnValue);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourBarrierGetReturnAddress(out nint returnValue);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourBarrierGetAddressOfReturnAddress(out nint returnValue);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourBarrierCallStackTrace(nint returnValue, long maxCount, out long outMaxCount);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourBarrierBeginStackTrace(out nint backup);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourBarrierEndStackTrace(nint backup);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourBarrierGetCallingModule(out nint returnValue);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int DetourGetHookBypassAddress(nint handle, out nint address);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern bool DetourCreateProcessWithDllExW(string lpApplicationName,
        string lpCommandLine,
        nint lpProcessAttributes,
        nint lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        nint lpEnvironment,
        string lpCurrentDirectory,
        nint lpStartupInfo,
        nint lpProcessInformation,
        string lpDllName,
        nint pfCreateProcessW);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool DetourCreateProcessWithDllExA(string lpApplicationName,
        string lpCommandLine,
        nint lpProcessAttributes,
        nint lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        nint lpEnvironment,
        string lpCurrentDirectory,
        nint lpStartupInfo,
        nint lpProcessInformation,
        string lpDllName,
        nint pfCreateProcessW);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern bool DetourCreateProcessWithDllsExW(string lpApplicationName,
          string lpCommandLine,
          nint lpProcessAttributes,
          nint lpThreadAttributes,
          bool bInheritHandles,
          uint dwCreationFlags,
          nint lpEnvironment,
          string lpCurrentDirectory,
          nint lpStartupInfo,
          nint lpProcessInformation,
          uint nDlls,
          nint rlpDlls,
          nint pfCreateProcessW);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool DetourCreateProcessWithDllsExA(string lpApplicationName,
        string lpCommandLine,
        nint lpProcessAttributes,
        nint lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        nint lpEnvironment,
        string lpCurrentDirectory,
        nint lpStartupInfo,
        nint lpProcessInformation,
        uint nDlls,
        nint rlpDlls,
        nint pfCreateProcessW);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern nint DetourFindFunction(string lpModule, string lpFunction);
}
