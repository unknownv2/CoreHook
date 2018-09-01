using System;

namespace CoreHook.Unmanaged
{
    public class SafeHandle : IDisposable
    {
        public IntPtr Handle { get; private set; }

        public static SafeHandle Wrap(IntPtr hHandle)
        {
            return new SafeHandle(hHandle);
        }

        private SafeHandle(IntPtr hHandle)
        {
            Handle = hHandle;
        }

        public void Dispose()
        {
            if (Handle != NativeMethods.InvalidHandleValue)
            {
                NativeMethods.CloseHandle(Handle);
                Handle = NativeMethods.InvalidHandleValue;
            }
        }
    }
}
