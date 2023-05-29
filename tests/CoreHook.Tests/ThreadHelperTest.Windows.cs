using CoreHook.BinaryInjection.RemoteInjection;

using System;
using System.Diagnostics;
using System.IO;

using Xunit;

namespace CoreHook.Tests;

public partial class ThreadHelperTest
{
    [Fact]
    public void ShouldGetFunctionAddressForCurrentProcess()
    {
        IntPtr functionAddress = IntPtr.Zero;
        string moduleFileName = Path.Combine(
                Environment.ExpandEnvironmentVariables("%Windir%"),
                "System32",
                "kernel32.dll");
        const string functionName = "LoadLibraryW";

        using var process = new ManagedProcess(Process.GetCurrentProcess());

        functionAddress = process.GetProcAddress(moduleFileName, functionName);

        Assert.NotEqual(IntPtr.Zero, functionAddress);
    }
}
