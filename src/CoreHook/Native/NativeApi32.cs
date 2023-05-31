using System.Runtime.InteropServices;

namespace CoreHook.Native;

internal static class NativeApi32
{
    private const string DllName = "corehook32";

    //[LibraryImport(DllName)]//, StringMarshalling = StringMarshalling.Utf16)]
    //[UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal static extern string RtlGetLastErrorStringCopy();

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int RtlGetLastError();

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern void DetourUninstallAllHooks();

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourInstallHook(nint entryPoint, nint hookProcedure, nint callback, nint handle);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourUninstallHook(nint refHandle);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourWaitForPendingRemovals();

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourSetInclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount, nint handle);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourSetExclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount, nint handle);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourSetGlobalInclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount);


    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourSetGlobalExclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourIsThreadIntercepted(nint handle, int threadId, out bool result);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourBarrierGetCallback(out nint returnValue);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourBarrierGetReturnAddress(out nint returnValue);


    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourBarrierGetAddressOfReturnAddress(out nint returnValue);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourBarrierBeginStackTrace(out nint backup);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourBarrierEndStackTrace(nint backup);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourBarrierGetCallingModule(out nint returnValue);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    internal static extern int DetourBarrierCallStackTrace(
        nint returnValue, long maxCount, out long maxStackTraceCount);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal static extern int DetourGetHookBypassAddress(nint handle, out nint address);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal static extern bool DetourCreateProcessWithDllExW(string lpApplicationName,
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
    internal static extern bool DetourCreateProcessWithDllExA(string lpApplicationName,
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
    internal static extern bool DetourCreateProcessWithDllsExW(string lpApplicationName,
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
    internal static extern bool DetourCreateProcessWithDllsExA(string lpApplicationName,
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
    internal static extern nint DetourFindFunction(string lpModule, string lpFunction);
}
