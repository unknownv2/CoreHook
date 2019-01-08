using System;
using System.Runtime.InteropServices;
using Xunit;

namespace CoreHook.Tests.Windows
{
    [Collection("Sequential")]
    public class LocalHookTest
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Beep(uint dwFreq, uint dwDuration);

        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool BeepDelegate(uint dwFreq, uint dwDuration);

        private bool _beepHookCalled;

        [return: MarshalAs(UnmanagedType.Bool)]
        private bool BeepHook(uint dwFreq, uint dwDuration)
        {
            _beepHookCalled = true;

            Beep(dwFreq, dwDuration);

            return false;
        }

        [Fact]
        public void DetourIsInstalled()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "Beep"),
                new BeepDelegate(BeepHook),
                this))
            {
                _beepHookCalled = false;

                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                Assert.False(Beep(100, 100));

                Assert.True(_beepHookCalled);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate uint GetTickCountDelegate();
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();
        private bool _getTickCountCalled;

        private uint Detour_GetTickCount()
        {
            _getTickCountCalled = true;
            return 0;
        }

        [Fact]
        public void DetourIsBypassedByOriginalFunction()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "GetTickCount"),
                new GetTickCountDelegate(Detour_GetTickCount),
                this))
            {
                _getTickCountCalled = false;

                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                var getTickCount = hook.OriginalAddress.ToFunction<GetTickCountDelegate>();

                Assert.NotEqual<uint>(0, getTickCount());

                Assert.False(_getTickCountCalled);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate long GetTickCount64Delegate();
        [DllImport("kernel32.dll")]
        public static extern long GetTickCount64();
        private bool _getTickCount64Called;

        private long Detour_GetTickCount64()
        {
            _getTickCount64Called = true;
            return 0;
        }

        [Fact]
        public void DetourCanBeBypassedAfterDetourCall()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "GetTickCount64"),
                new GetTickCount64Delegate(Detour_GetTickCount64),
                this))
            {
                _getTickCount64Called = false;

                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                Assert.Equal(0, GetTickCount64());

                Assert.True(_getTickCount64Called);

                _getTickCount64Called = false;

                var getTickCount64 = hook.OriginalAddress.ToFunction<GetTickCount64Delegate>();

                Assert.NotEqual(0, getTickCount64());

                Assert.False(_getTickCount64Called);
            }
        }

        [Fact]
        public void Invalid_LocalHook_Create_Detour_Delegate_Throws_ArgumentNUllException()
        {
            Assert.Throws<ArgumentNullException>(() => LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
                null,
                this));
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate bool QueryPerformanceCounterDelegate(out long performanceCount);

        private bool Detour_QueryPerformanceCounter(out long performanceCount)
        {
            performanceCount = 0;
            return false;
        }

        [Fact]
        public void TestInvalidDetourCallback()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "QueryPerformanceCounter"),
                new QueryPerformanceCounterDelegate(Detour_QueryPerformanceCounter),
                null))
            {    
                Assert.Null(hook.Callback);
                Assert.NotNull(hook.ThreadACL);
                Assert.NotNull(hook.OriginalAddress);
                Assert.NotEqual(IntPtr.Zero, hook.OriginalAddress);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate uint GetVersionDelegate();

        [DllImport("kernel32.dll")]
        public static extern uint GetVersion();

        [Fact]
        public void ShouldEnableHookWithProperty()
        {
            using (var hook = HookFactory.CreateHook<GetVersionDelegate>(
                LocalHook.GetProcAddress("kernel32.dll", "GetVersion"),
                GetVersionDetour, this))
            {
                // Enable the hook for all threads
                hook.Enabled = true;
                Assert.Equal<uint>(0, GetVersion());
                Assert.Equal<uint>(0, hook.Target());

                // Disable the hook for all threads
                hook.Enabled = false;
                Assert.NotEqual<uint>(0, GetVersion());
                Assert.NotEqual<uint>(0, hook.Target());
                Assert.NotEqual<uint>(0, hook.Original());

                // Enable the hook for the current thread
                hook.ThreadACL.SetInclusiveACL(new int[1]);
                Assert.Equal<uint>(0, GetVersion());
                Assert.Equal<uint>(0, hook.Target());
                Assert.NotEqual<uint>(0, hook.Original());
                
                // Disable the hook for the current thread
                hook.ThreadACL.SetExclusiveACL(new int[1]);
                Assert.NotEqual<uint>(0, GetVersion());
                Assert.NotEqual<uint>(0, hook.Target());
            }
        }
        private uint GetVersionDetour()
        {
            return 0;
        }  
    }
}
