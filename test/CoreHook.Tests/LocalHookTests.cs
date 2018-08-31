using System.Runtime.InteropServices;
using Xunit;

namespace CoreHook.Tests
{
    public class LocalHookTests
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool Beep(uint dwFreq, uint dwDuration);

        [return: MarshalAs(UnmanagedType.Bool)]
        delegate bool BeepDelegate(uint dwFreq, uint dwDuration);

        private bool _beepHookCalled;

        [return: MarshalAs(UnmanagedType.Bool)]
        bool BeepHook(uint dwFreq, uint dwDuration)
        {
            _beepHookCalled = true;

            Beep(dwFreq, dwDuration);

            return false;
        }

        [Fact]
        public void DetourIsInstalled()
        {
            _beepHookCalled = false;

            LocalHook hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "Beep"),
                new BeepDelegate(BeepHook),
                this);

            hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

            Assert.False(Beep(100, 100));

            Assert.True(_beepHookCalled);
        }
        [Fact]
        public void DetourIsBypassedByOriginalFunction()
        {
            _beepHookCalled = false;

            LocalHook hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "Beep"),
                new BeepDelegate(BeepHook),
                this);

            hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

            Assert.False(Beep(100, 100));

            Assert.True(_beepHookCalled);

            _beepHookCalled = false;

            BeepDelegate beep = (BeepDelegate)Marshal.GetDelegateForFunctionPointer(hook.HookBypassAddress, typeof(BeepDelegate));

            beep(100, 100);
            Assert.False(_beepHookCalled);
        }
    }
}
