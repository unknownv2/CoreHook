using System;

namespace CoreHook
{
    /// <summary>
    /// Class used for managing the thread ACL of a hook.
    /// </summary>
    internal class HookAccessControl : IHookAccessControl
    {
        private static readonly int[] DefaultThreadAcl = new int[0];

        public bool IsExclusive { get; private set; }

        public bool IsInclusive => !IsExclusive;

        private readonly IntPtr _handle;

        private int[] _acl = DefaultThreadAcl;

        internal HookAccessControl(IntPtr handle)
        {
            IsExclusive = handle == IntPtr.Zero;
            _handle = handle;
        }

        /// <summary>
        /// Overwrite the current ACL and set an exclusive list of threads.
        /// Exclusive means that all threads in <paramref name="acl"/> are
        /// not intercepted and all other threads are intercepted.
        /// </summary>
        /// <param name="acl">List of threads to exclude from intercepting.</param>
        public void SetExclusiveACL(int[] acl)
        {
            IsExclusive = true;

            _acl = acl == null ? DefaultThreadAcl : (int[])acl.Clone();

            if (_handle == IntPtr.Zero)
            {
                NativeApi.DetourSetGlobalExclusiveACL(_acl, _acl.Length);
            }
            else
            {
                NativeApi.DetourSetExclusiveACL(_acl, _acl.Length, _handle);
            }
        }

        /// <summary>
        /// Overwrite the current ACL and set an inclusive list of threads.
        /// Inclusive means that all threads in <paramref name="acl"/> are
        /// intercepted and any others not present in the list are not.
        /// </summary>
        /// <param name="acl">A list of threads to that are intercepted.</param>
        public void SetInclusiveACL(int[] acl)
        {
            IsExclusive = false;

            _acl = acl == null ? DefaultThreadAcl : (int[])acl.Clone();

            if (_handle == IntPtr.Zero)
            {
                NativeApi.DetourSetGlobalInclusiveACL(_acl, _acl.Length);
            }
            else
            {
                NativeApi.DetourSetInclusiveACL(_acl, _acl.Length, _handle);
            }
        }

        /// <summary>
        /// Get a copy of the current thread list for this ACL.
        /// The returned list can be edited without affecting the hook.
        /// </summary>
        /// <returns>A copy of the ACL's thread list.</returns>
        public int[] GetEntries()
        {
            return (int[])_acl.Clone();
        }
    }
}
