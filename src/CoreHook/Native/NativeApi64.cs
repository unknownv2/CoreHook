using System.Runtime.InteropServices;

namespace CoreHook.Native;

internal static partial class NativeApi64
{
    private const string DllName = "corehook64";

    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial string RtlGetLastErrorStringCopy();

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int RtlGetLastError();

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial void DetourUninstallAllHooks();

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourInstallHook(nint entryPoint, nint hookProcedure, nint callback, nint handle);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourUninstallHook(nint refHandle);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourWaitForPendingRemovals();

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourSetInclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount, nint handle);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourSetExclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount, nint handle);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourSetGlobalInclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount);


    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourSetGlobalExclusiveACL([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] threadIdList, int threadCount);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DetourIsThreadIntercepted(nint handle, int threadId, out bool result);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourBarrierGetCallback(out nint returnValue);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourBarrierGetReturnAddress(out nint returnValue);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourBarrierGetAddressOfReturnAddress(out nint returnValue);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourBarrierCallStackTrace(nint returnValue, long maxCount, out long outMaxCount);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourBarrierBeginStackTrace(out nint backup);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourBarrierEndStackTrace(nint backup);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourBarrierGetCallingModule(out nint returnValue);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial int DetourGetHookBypassAddress(nint handle, out nint address);

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

    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
    [UnmanagedCallConv(CallConvs = new System.Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    public static partial nint DetourFindFunction(string lpModule, string lpFunction);
}
