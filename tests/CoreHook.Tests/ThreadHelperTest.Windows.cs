using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using CoreHook.Memory;

namespace CoreHook.Tests
{
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

            using (var processHandle = new ManagedProcess(Process.GetCurrentProcess()).SafeHandle)
            {
                functionAddress = ThreadHelper.GetProcAddress(
                                 processHandle,
                                 moduleFileName,
                                 functionName);
            }

            Assert.NotEqual(IntPtr.Zero, functionAddress);
        }
    }
}
