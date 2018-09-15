using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace CoreHook.Tests
{
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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern ushort AddAtomW(string lpString);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern uint GetAtomNameW(ushort nAtom, StringBuilder lpBuffer, int nSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern ushort DeleteAtom(ushort nAtom);

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private delegate ushort AddAtomWDelegate(string lpString);

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private delegate ushort InternalAddAtomDelegate(bool local, bool unicode,
            string atomName, int arg4);

        private InternalAddAtomDelegate InternalAddAtomFunction;
        private bool _internalAddAtomCalled;
        private bool _AddAtomCalled;

        private ushort InternalAddAtomHook(bool local,
            bool unicode, string atomName, int arg4)
        {
            _internalAddAtomCalled = true;            

            return InternalAddAtomFunction(local, unicode, atomName, arg4);
        }
        private ushort AddAtomHook(string atomName)
        {
            _AddAtomCalled = true;

            return AddAtomW(atomName);
        }
#if WIN64
        [Fact]
        public void DetourInternalFunction()
        {
            LocalHook hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "InternalAddAtom"),
                new InternalAddAtomDelegate(InternalAddAtomHook),
                this);
            hook.ThreadACL.SetInclusiveACL(new int[] { 0 });
            InternalAddAtomFunction = (InternalAddAtomDelegate)
                        Marshal.GetDelegateForFunctionPointer(hook.HookBypassAddress, typeof(InternalAddAtomDelegate));

            _internalAddAtomCalled = false;

            string atomName = "TestLocalAtomName";            
            ushort atomId = AddAtomW(atomName);

            Assert.NotEqual(0, atomId);
            Assert.True(_internalAddAtomCalled);

            int maxPathLength = 260;
            StringBuilder atomBuffer = new StringBuilder(maxPathLength);
            uint bufLength = GetAtomNameW(atomId, atomBuffer, maxPathLength);
            string retrievedAtomName = atomBuffer.ToString();

            Assert.Equal((uint)atomName.Length, bufLength);
            Assert.Equal(retrievedAtomName.Length, atomName.Length);

            Assert.Equal(retrievedAtomName, atomName);

            Assert.Equal<ushort>(0, DeleteAtom(atomId));
        }

        [Fact]
        public void DetourAPIAndInternalFunction()
        {
            LocalHook hookInternal = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "InternalAddAtom"),
                new InternalAddAtomDelegate(InternalAddAtomHook),
                this);
            hookInternal.ThreadACL.SetInclusiveACL(new int[] { 0 });

            LocalHook hookAPI = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "AddAtomW"),
                new AddAtomWDelegate(AddAtomHook),
                this);
            hookAPI.ThreadACL.SetInclusiveACL(new int[] { 0 });

            InternalAddAtomFunction = (InternalAddAtomDelegate)
                Marshal.GetDelegateForFunctionPointer(hookInternal.HookBypassAddress, typeof(InternalAddAtomDelegate));

            _internalAddAtomCalled = false;
            _AddAtomCalled = false;

            string atomName = "TestLocalAtomName";
            ushort atomId = AddAtomW(atomName);

            Assert.NotEqual(0, atomId);
            Assert.True(_internalAddAtomCalled);
            Assert.True(_AddAtomCalled);

            int maxPathLength = 260;
            StringBuilder atomBuffer = new StringBuilder(maxPathLength);
            uint bufLength = GetAtomNameW(atomId, atomBuffer, maxPathLength);
            string retrievedAtomName = atomBuffer.ToString();

            Assert.NotEqual<uint>(0, bufLength);
            Assert.Equal((uint)atomName.Length, bufLength);
            Assert.Equal(retrievedAtomName.Length, atomName.Length);

            Assert.Equal(retrievedAtomName, atomName);

            Assert.Equal<ushort>(0, DeleteAtom(atomId));
        }
#endif
        private delegate ulong GetCurrentNlsCacheDelegate();

        private GetCurrentNlsCacheDelegate GetCurrentNlsCacheFunction;

        private bool _GetCurrentNlsCacheCalled;

        [DllImport("kernelbase.dll",
             CharSet = CharSet.Unicode,
             SetLastError = true,
             CallingConvention = CallingConvention.StdCall)]
        private static extern int CompareStringW(uint locale, uint dwCmpFlags,
            string lpString1, int cchCount1, string lpString2, int cchCount2);

        private const int LOCALE_USER_DEFAULT = 0x400;

        // case sensitive compare
        private const int NORM_LINGUISTIC_CASING = 0x08000000;

        // The string indicated by lpString1 is greater in lexical value
        // than the string indicated by lpString2.
        private const int CSTR_GREATER_THAN = 3;

        private ulong GetCurrentNlsCacheHook()
        {
            _GetCurrentNlsCacheCalled = true;

            return GetCurrentNlsCacheFunction();
        }

        [Fact]
        public void DetourInternalFunction2()
        {
            _GetCurrentNlsCacheCalled = false;

            LocalHook hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernelbase.dll", "GetCurrentNlsCache"),
                new GetCurrentNlsCacheDelegate(GetCurrentNlsCacheHook),
                this);
            hook.ThreadACL.SetInclusiveACL(new int[] { 0 });
            GetCurrentNlsCacheFunction = (GetCurrentNlsCacheDelegate)
                Marshal.GetDelegateForFunctionPointer(hook.HookBypassAddress, typeof(GetCurrentNlsCacheDelegate));

            string stringA = "HelloWorld";
            string stringB = "Hello";

            int comparisonResult = CompareStringW(
                LOCALE_USER_DEFAULT,
                NORM_LINGUISTIC_CASING,
                stringA,
                stringA.Length,
                stringB,
                stringB.Length);

            Assert.Equal(CSTR_GREATER_THAN, comparisonResult);
            Assert.True(_GetCurrentNlsCacheCalled);
        }
    }
}
