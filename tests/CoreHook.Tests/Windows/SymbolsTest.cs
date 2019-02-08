using System;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace CoreHook.Tests.Windows
{
    [Collection("Sequential")]
    public class SymbolsTest
    {
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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern ushort FindAtomW(string lpString);

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private delegate ushort AddAtomWDelegate(string lpString);

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private delegate ushort InternalAddAtomDelegate(bool local, bool unicode,
            string atomName, int arg4);

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private delegate ushort InternalFindAtom(bool local, bool unicode,
            string atomName);

        private InternalAddAtomDelegate InternalAddAtomFunction;
        private InternalFindAtom InternalFindAtomFunction;

        private bool _internalAddAtomCalled;
        private bool _internalFindAtomCalled;
        private bool _addAtomCalled;

        // Windows max file system path string size
        private const int MaxPathLength = 260;

        private ushort InternalAddAtomHook(bool local,
            bool unicode, string atomName, int arg4)
        {
            _internalAddAtomCalled = true;

            return InternalAddAtomFunction(local, unicode, atomName, arg4);
        }
        private ushort AddAtomHook(string atomName)
        {
            _addAtomCalled = true;

            return AddAtomW(atomName);
        }

        /// <summary>
        /// Detour a private function and call the internal function 
        /// using the detour bypass address to skip the detour barrier call
        /// </summary>
        [Fact]
        public void DetourInternalFunction()
        {
            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "InternalAddAtom"),
                new InternalAddAtomDelegate(InternalAddAtomHook),
                this))
            {
                InternalAddAtomFunction = hook.OriginalAddress.ToFunction<InternalAddAtomDelegate>();

                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                _internalAddAtomCalled = false;

                string atomName = "TestLocalAtomName";
                ushort atomId = AddAtomW(atomName);

                Assert.NotEqual(0, atomId);
                Assert.True(_internalAddAtomCalled);

                StringBuilder atomBuffer = new StringBuilder(MaxPathLength);
                uint bufLength = GetAtomNameW(atomId, atomBuffer, MaxPathLength);
                string retrievedAtomName = atomBuffer.ToString();

                Assert.Equal((uint)atomName.Length, bufLength);
                Assert.Equal(retrievedAtomName.Length, atomName.Length);

                Assert.Equal(retrievedAtomName, atomName);

                Assert.Equal<ushort>(0, DeleteAtom(atomId));
            }
        }

        /// <summary>
        /// Detour a private function and call the function's direct address
        /// when the detour is called without skipping the detour barrier
        /// </summary>
        [Fact]
        public void DetourAPIAndInternalFunction()
        {
            // Create the internal function detour
            using (var hookInternal = LocalHook.Create(
                 LocalHook.GetProcAddress("kernel32.dll", "InternalAddAtom"),
                 new InternalAddAtomDelegate(InternalAddAtomHook),
                 this))
            // Create the public API detour 
            using (var hookAPI = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "AddAtomW"),
                new AddAtomWDelegate(AddAtomHook),
                this))
            {

                hookInternal.ThreadACL.SetInclusiveACL(new int[] { 0 });
                InternalAddAtomFunction = hookInternal.TargetAddress.ToFunction<InternalAddAtomDelegate>();

                hookAPI.ThreadACL.SetInclusiveACL(new int[] { 0 });

                _internalAddAtomCalled = false;
                _addAtomCalled = false;

                string atomName = "TestLocalAtomName";
                ushort atomId = AddAtomW(atomName);

                Assert.NotEqual(0, atomId);
                Assert.True(_internalAddAtomCalled);
                Assert.True(_addAtomCalled);

                StringBuilder atomBuffer = new StringBuilder(MaxPathLength);
                uint bufLength = GetAtomNameW(atomId, atomBuffer, MaxPathLength);
                string retrievedAtomName = atomBuffer.ToString();

                Assert.NotEqual<uint>(0, bufLength);
                Assert.Equal((uint)atomName.Length, bufLength);
                Assert.Equal(retrievedAtomName.Length, atomName.Length);

                Assert.Equal(retrievedAtomName, atomName);

                Assert.Equal<ushort>(0, DeleteAtom(atomId));
            }
        }

        [Fact]
        public void DetourAPIAndInternalFunctionUsingBypassAddress()
        {
            using (var hookInternal = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "InternalAddAtom"),
                new InternalAddAtomDelegate(InternalAddAtomHook),
                this))
            using (var hookAPI = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "AddAtomW"),
                new AddAtomWDelegate(AddAtomHook),
                this))
            {
                hookInternal.ThreadACL.SetInclusiveACL(new int[] { 0 });

                hookAPI.ThreadACL.SetInclusiveACL(new int[] { 0 });

                InternalAddAtomFunction = hookInternal.OriginalAddress.ToFunction<InternalAddAtomDelegate>();

                _internalAddAtomCalled = false;
                _addAtomCalled = false;

                string atomName = "TestLocalAtomName";
                ushort atomId = AddAtomW(atomName);

                Assert.NotEqual(0, atomId);
                Assert.True(_internalAddAtomCalled);
                Assert.True(_addAtomCalled);

                StringBuilder atomBuffer = new StringBuilder(MaxPathLength);
                uint bufLength = GetAtomNameW(atomId, atomBuffer, MaxPathLength);
                string retrievedAtomName = atomBuffer.ToString();

                Assert.NotEqual<uint>(0, bufLength);
                Assert.Equal((uint)atomName.Length, bufLength);
                Assert.Equal(retrievedAtomName.Length, atomName.Length);

                Assert.Equal(retrievedAtomName, atomName);

                Assert.Equal<ushort>(0, DeleteAtom(atomId));
            }
        }

        [Fact]
        public void DetourApiAndInternalFunctionUsingInterfaceBypassAddress()
        {
            using (var hookInternal = HookFactory.CreateHook<InternalFindAtom>(
                LocalHook.GetProcAddress("kernel32.dll", "InternalFindAtom"),
                InternalFindAtom_Hook,
                this))
            {
                hookInternal.ThreadACL.SetInclusiveACL(new int[] { 0 });
            
                InternalFindAtomFunction = hookInternal.Original;

                _internalFindAtomCalled = false;

                string atomName = "TestLocalAtomName";
                ushort atomId = AddAtomW(atomName);

                ushort foundAtomId = FindAtomW(atomName);

                Assert.NotEqual(0, atomId);
                Assert.True(_internalFindAtomCalled);
                Assert.Equal(atomId, foundAtomId);

                Assert.Equal<ushort>(0, DeleteAtom(atomId));
            }
        }

        ushort InternalFindAtom_Hook(bool local, bool unicode, string atomName)
        {
            _internalFindAtomCalled = true;
            return InternalFindAtomFunction(local, unicode, atomName);
        }

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

            using (var hook = LocalHook.Create(
                LocalHook.GetProcAddress("kernelbase.dll", "GetCurrentNlsCache"),
                new GetCurrentNlsCacheDelegate(GetCurrentNlsCacheHook),
                this))
            {
                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });
                GetCurrentNlsCacheFunction = hook.OriginalAddress.ToFunction<GetCurrentNlsCacheDelegate>();

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

        [Fact]
        public void Find_Invalid_Export_Function_Throws_MissingMethodException()
        {
            Assert.Throws<MissingMethodException>(() => LocalHook.GetProcAddress("kernel32.dll", "ThisFunctionDoesNotExist"));
        }

        [Fact]
        public void Find_Invalid_Export_Module_Throws_MissingMethodException()
        {
            Assert.Throws<MissingMethodException>(() => LocalHook.GetProcAddress("UnknownModule.dll", "CreateFileW"));
        }

        [Fact]
        public void Find_Invalid_Export_ModuleFunction_Throws_MissingMethodException()
        {
            Assert.Throws<MissingMethodException>(() => LocalHook.GetProcAddress("UnknownModule.dll", "ThisFunctionDoesNotExist"));
        }
    }
}
