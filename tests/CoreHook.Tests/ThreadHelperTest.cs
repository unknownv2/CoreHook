using CoreHook.BinaryInjection.RemoteInjection;

using System.Diagnostics;
using Xunit;

namespace CoreHook.Tests;

public partial class ThreadHelperTest
{
    [Fact]
    public void ShouldOpenProcessHandleForCurrentProcess()
    {
        using (var processHandle = new ManagedProcess(Process.GetCurrentProcess()).SafeHandle)
        {
            Assert.NotEqual(true, processHandle.IsInvalid);
        }
    }
}
