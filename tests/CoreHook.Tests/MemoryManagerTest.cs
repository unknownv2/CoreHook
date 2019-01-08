using System.Diagnostics;
using Xunit;
using CoreHook.Memory;
using CoreHook.Memory.Processes;

namespace CoreHook.Tests
{
    public class MemoryManagerTest
    {
        [Fact]
        public void ShouldAllocateMemoryInProcess()
        {
            const int memoryAllocationSize = 0x400;

            using (IMemoryManager manager = new MemoryManager(new ManagedProcess(Process.GetCurrentProcess())))
            {
                var allocation = 
                    manager.Allocate(memoryAllocationSize, MemoryProtectionType.ReadWrite);
    
                Assert.Equal(false, allocation.IsFree);
                Assert.Equal(memoryAllocationSize, allocation.Size);
            }
        }
    }
}
