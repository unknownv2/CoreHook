using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace CoreHook.Tests
{
    public class LocalHookTests
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

            Assert.True(beep(100, 100));

            Assert.False(_beepHookCalled);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern ushort AddAtomW(string lpString);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetAtomNameW(ushort nAtom, StringBuilder lpBuffer, int nSize);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern ushort DeleteAtom(ushort nAtom);

        private delegate ushort AddAtomWDelegate(string lpString);
        private delegate ushort InternalAddAtomDelegate(bool local, bool unicode, string atomName, int arg4);

        private InternalAddAtomDelegate InternalAddAtomFunction;
        private bool _internalAddAtomCalled;

        private ushort InternalAddAtomHook(bool local, bool unicode, string atomName, int arg4)
        {
            _internalAddAtomCalled = true;            

            return InternalAddAtomFunction(local, unicode, atomName, arg4);
        }

        [Fact]
        public void DetourInternalFunction()
        {
            _internalAddAtomCalled = false;

            LocalHook hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "InternalAddAtom"),
                new InternalAddAtomDelegate(InternalAddAtomHook),
                this);
            hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

            InternalAddAtomFunction = (InternalAddAtomDelegate)
                Marshal.GetDelegateForFunctionPointer(hook.HookBypassAddress, typeof(InternalAddAtomDelegate));

            string atomName = "TestLocalAtomName";            
            ushort atomId = AddAtomW(atomName);

            Assert.NotEqual(0, atomId);
            Assert.True(_internalAddAtomCalled);

            int maxPathLength = 260;
            StringBuilder atomBuffer = new StringBuilder(maxPathLength);
            uint bufLength = GetAtomNameW(atomId, atomBuffer, maxPathLength);
            string retrievedAtomName = atomBuffer.ToString();

            Assert.Equal(retrievedAtomName.Length, atomName.Length);
            Assert.Equal(retrievedAtomName, atomName);

            Assert.Equal<ushort>(0, DeleteAtom(atomId));
        }
    }
}
