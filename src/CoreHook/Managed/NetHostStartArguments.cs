using System.Runtime.InteropServices;

namespace CoreHook.Managed;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct NetHostStartArguments
{
    /// <summary>
    /// Library that resolves dependencies and passes arguments to
    /// the .NET payload Assembly.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public readonly string ClrBootstrapLibrary;

    /// <summary>
    /// Directory path which contains the folder with a `dotnet.runtimeconfig.json`
    /// containing properties for initializing CoreCLR.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public readonly string ClrRootPath;

    /// <summary>
    /// Option to enable the logging module inside the HostLibrary.
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public readonly bool VerboseLog;

    public NetHostStartArguments(string clrBootstrapLibrary, string clrRootPath, bool verboseLog)
    {
        ClrBootstrapLibrary = clrBootstrapLibrary;
        ClrRootPath = clrRootPath;
        VerboseLog = verboseLog;
    }
}