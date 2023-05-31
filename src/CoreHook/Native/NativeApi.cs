using System;

namespace CoreHook.Native;
/// <summary>
/// APIs for calling into the native detouring module.
/// </summary>
public static class NativeApi
{
    /// <summary>
    /// Determine if the current application is 32 or 64 bit.
    /// </summary>
    public static readonly bool Is64Bit = nint.Size == 8;

    internal const int STATUS_SUCCESS = 0;

    internal static void HandleErrorCode(int errorCode)
    {
        if (errorCode != STATUS_SUCCESS)
        {
            throw new ApplicationException($"Unknown error {errorCode}.");
        }
    }

    public static void DetourUninstallAllHooks()
    {
        if (Is64Bit)
        {
            NativeApi64.DetourUninstallAllHooks();
        }
        else
        {
            NativeApi32.DetourUninstallAllHooks();
        }
    }

    public static void DetourInstallHook(nint entryPoint, nint hookProcedure, nint callback, nint handle)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourInstallHook(entryPoint, hookProcedure, callback, handle));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourInstallHook(entryPoint, hookProcedure, callback, handle));
        }
    }

    public static void DetourUninstallHook(nint refHandle)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourUninstallHook(refHandle));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourUninstallHook(refHandle));
        }
    }

    public static void DetourWaitForPendingRemovals()
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourWaitForPendingRemovals());
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourWaitForPendingRemovals());
        }
    }

    public static void DetourIsThreadIntercepted(nint handle, int threadId, out bool result)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourIsThreadIntercepted(handle, threadId, out result));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourIsThreadIntercepted(handle, threadId, out result));
        }
    }

    public static void DetourSetInclusiveACL(int[] threadIdList, int threadCount, nint handle)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourSetInclusiveACL(threadIdList, threadCount, handle));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourSetInclusiveACL(threadIdList, threadCount, handle));
        }
    }

    public static void DetourSetExclusiveACL(int[] threadIdList, int threadCount, nint handle)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourSetExclusiveACL(threadIdList, threadCount, handle));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourSetExclusiveACL(threadIdList, threadCount, handle));
        }
    }

    public static void DetourSetGlobalInclusiveACL(int[] threadIdList, int threadCount)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourSetGlobalInclusiveACL(threadIdList, threadCount));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourSetGlobalInclusiveACL(threadIdList, threadCount));
        }
    }

    public static void DetourSetGlobalExclusiveACL(int[] threadIdList, int threadCount)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourSetGlobalExclusiveACL(threadIdList, threadCount));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourSetGlobalExclusiveACL(threadIdList, threadCount));
        }
    }


    public static void DetourBarrierGetCallingModule(out nint returnValue)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourBarrierGetCallingModule(out returnValue));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourBarrierGetCallingModule(out returnValue));
        }
    }

    public static int DetourBarrierGetCallback(out nint returnValue)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourBarrierGetCallback(out returnValue);
        }
        else
        {
            return NativeApi32.DetourBarrierGetCallback(out returnValue);
        }
    }

    public static void DetourBarrierGetReturnAddress(out nint returnValue)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourBarrierGetReturnAddress(out returnValue));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourBarrierGetReturnAddress(out returnValue));
        }
    }

    public static void DetourBarrierGetAddressOfReturnAddress(out nint returnValue)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourBarrierGetAddressOfReturnAddress(out returnValue));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourBarrierGetAddressOfReturnAddress(out returnValue));
        }
    }

    public static void DetourBarrierBeginStackTrace(out nint backup)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourBarrierBeginStackTrace(out backup));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourBarrierBeginStackTrace(out backup));
        }
    }

    public static void DetourBarrierCallStackTrace(nint backup, long maxCount, out long outMaxCount)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourBarrierCallStackTrace(backup, maxCount, out outMaxCount));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourBarrierCallStackTrace(backup, maxCount, out outMaxCount));
        }

    }
    public static void DetourBarrierEndStackTrace(nint backup)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourBarrierEndStackTrace(backup));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourBarrierEndStackTrace(backup));
        }
    }

    public static void DetourGetHookBypassAddress(nint handle, out nint address)
    {
        if (Is64Bit)
        {
            HandleErrorCode(NativeApi64.DetourGetHookBypassAddress(handle, out address));
        }
        else
        {
            HandleErrorCode(NativeApi32.DetourGetHookBypassAddress(handle, out address));
        }

    }

    public static bool DetourCreateProcessWithDllExA(
        string lpApplicationName,
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
        nint pfCreateProcessW)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourCreateProcessWithDllExA(
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
                    pfCreateProcessW);
        }
        else
        {
            return NativeApi32.DetourCreateProcessWithDllExA(
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
                    pfCreateProcessW);
        }
    }
    public static bool DetourCreateProcessWithDllExW(
        string lpApplicationName,
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
        nint pfCreateProcessW)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourCreateProcessWithDllExW(
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
                    pfCreateProcessW);
        }
        else
        {
            return NativeApi32.DetourCreateProcessWithDllExW(
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
                    pfCreateProcessW);
        }
    }
    public static bool DetourCreateProcessWithDllsExA(
        string lpApplicationName,
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
        nint pfCreateProcessW)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourCreateProcessWithDllsExA(
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
                    pfCreateProcessW);
        }
        else
        {
            return NativeApi32.DetourCreateProcessWithDllsExA(
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
                    pfCreateProcessW);
        }
    }
    public static bool DetourCreateProcessWithDllsExW(
        string lpApplicationName,
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
        nint pfCreateProcessW)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourCreateProcessWithDllsExW(
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
                    pfCreateProcessW);
        }
        else
        {
            return NativeApi32.DetourCreateProcessWithDllsExW(
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
                    pfCreateProcessW);
        }
    }
    public static nint DetourFindFunction(
        string lpModule,
        string lpFunction)
    {
        if (Is64Bit)
        {
            return NativeApi64.DetourFindFunction(lpModule,
                lpFunction);
        }
        else
        {
            return NativeApi32.DetourFindFunction(lpModule,
                lpFunction);
        }
    }
}
