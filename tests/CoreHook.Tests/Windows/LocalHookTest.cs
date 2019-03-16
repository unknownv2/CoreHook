using System;
using System.Runtime.InteropServices;
using Xunit;

namespace CoreHook.Tests.Windows
{
    [Collection("Local Hook Tests")]
    public class LocalHookTest
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool BeepDelegate(int dwFreq, int dwDuration);

        private bool _beepHookCalled;

        [return: MarshalAs(UnmanagedType.Bool)]
        private bool BeepHook(int dwFreq, int dwDuration)
        {
            _beepHookCalled = true;

            Interop.Kernel32.Beep(dwFreq, dwDuration);

            return false;
        }

        [Fact]
        public void ShouldIntallDetourToFunctionAddress()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress(Interop.Libraries.Kernel32, "Beep"),
                new BeepDelegate(BeepHook),
                this))
            {
                _beepHookCalled = false;

                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                Assert.False(Interop.Kernel32.Beep(100, 100));

                Assert.True(_beepHookCalled);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate uint GetTickCountDelegate();

        [DllImport(Interop.Libraries.Kernel32)]
        public static extern uint GetTickCount();

        private bool _getTickCountCalled;

        private uint Detour_GetTickCount()
        {
            _getTickCountCalled = true;

            return 0;
        }

        [Fact]
        public void ShouldBypassDetourWithCallToOriginalFunction()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress(Interop.Libraries.Kernel32, "GetTickCount"),
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
        public delegate ulong GetTickCount64Delegate();

        private bool _getTickCount64Called;

        private ulong Detour_GetTickCount64()
        {
            _getTickCount64Called = true;

            return 0;
        }

        [Fact]
        public void ShouldBypassInstalledFunctionDetour()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress(Interop.Libraries.Kernel32, "GetTickCount64"),
                new GetTickCount64Delegate(Detour_GetTickCount64),
                this))
            {
                _getTickCount64Called = false;

                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                Assert.Equal<ulong>(0, Interop.Kernel32.GetTickCount64());

                Assert.True(_getTickCount64Called);

                _getTickCount64Called = false;

                var getTickCount64 = hook.OriginalAddress.ToFunction<GetTickCount64Delegate>();

                Assert.NotEqual<ulong>(0, getTickCount64());

                Assert.False(_getTickCount64Called);
            }
        }

        [Fact]
        public void Invalid_LocalHook_Create_Detour_Delegate_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => LocalHook.Create(
                LocalHook.GetProcAddress(Interop.Libraries.Kernel32, "CreateFileW"),
                null,
                this));
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate bool QueryPerformanceCounterDelegate(out long performanceCount);

        private bool Detour_QueryPerformanceCounter(out long performanceCount)
        {
            performanceCount = 0;
            return false;
        }

        [Fact]
        public void ShouldCreateNullDetourCallback()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress(Interop.Libraries.Kernel32, "QueryPerformanceCounter"),
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

        [Fact]
        public void ShouldEnableHookWithProperty()
        {
            using (var hook = HookFactory.CreateHook<GetVersionDelegate>(
                LocalHook.GetProcAddress(Interop.Libraries.Kernel32, "GetVersion"),
                Detour_GetVersion, this))
            {
                // Enable the hook for all threads
                hook.Enabled = true;
                Assert.Equal<uint>(0, Interop.Kernel32.GetVersion());
                Assert.Equal<uint>(0, hook.Target());

                // Disable the hook for all threads
                hook.Enabled = false;
                Assert.NotEqual<uint>(0, Interop.Kernel32.GetVersion());
                Assert.NotEqual<uint>(0, hook.Target());
                Assert.NotEqual<uint>(0, hook.Original());

                // Enable the hook for the current thread
                hook.ThreadACL.SetInclusiveACL(new int[1]);
                Assert.Equal<uint>(0, Interop.Kernel32.GetVersion());
                Assert.Equal<uint>(0, hook.Target());
                Assert.NotEqual<uint>(0, hook.Original());
                
                // Disable the hook for the current thread
                hook.ThreadACL.SetExclusiveACL(new int[1]);
                Assert.NotEqual<uint>(0, Interop.Kernel32.GetVersion());
                Assert.NotEqual<uint>(0, hook.Target());
            }
        }

        private uint Detour_GetVersion() => 0;
    }
}
