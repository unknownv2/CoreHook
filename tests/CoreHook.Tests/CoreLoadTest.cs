using System;
using Xunit;

namespace CoreHook.Tests
{
    public class CoreLoadTest
    {
        [Fact]
        public void ShouldThrowArgumentOutOfRange_For_Loader_IntPtrZeroParameter()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CoreLoad.Loader.Load(IntPtr.Zero));
        }
    }
}
