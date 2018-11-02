using System.Diagnostics;
using Xunit;
using CoreHook.Memory;

namespace CoreHook.Tests
{
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
}
