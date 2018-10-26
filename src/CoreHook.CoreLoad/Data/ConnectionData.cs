using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CoreHook.CoreLoad.Data
{
    internal class ConnectionData<T, U> where T : IRemoteEntryInfo
    {
        internal enum ConnectionState
        {
            Invalid = 0,
            NoChannel = 1,
            Valid = int.MaxValue
        }

        /// <summary>
        /// Gets the state of the current <see cref="ConnectionData"/>.
        /// </summary>
        internal ConnectionState State { get; private set; }

        /// <summary>
        /// Gets the unmanaged data containing the pointer to the memory block containing a class of type <see cref="T"/>;
        /// </summary>
        internal T UnmanagedInfo { get; private set; }

        internal U RemoteInfo { get; private set; }

        private ConnectionData()
        {
            State = ConnectionState.Invalid;
            RemoteInfo = default(U);
            UnmanagedInfo = default(T);
        }

        /// <summary>
        /// Loads <see cref="ConnectionData"/> from the <see cref="IntPtr"/> specified.
        /// </summary>
        /// <param name="unmanagedInfoPointer"></param>
        /// <param name="formatter"></param>
        public static ConnectionData<T, U> LoadData(
            IntPtr unmanagedInfoPointer,
            IUserDataFormatter formatter)
        {
            var data = new ConnectionData<T, U>
            {
                State = ConnectionState.Valid,
                UnmanagedInfo = (T)Activator.CreateInstance(typeof(T))
            };
            try
            {
                // Get the unmanaged data containing the remote user parameters
                Marshal.PtrToStructure(unmanagedInfoPointer, data.UnmanagedInfo);

                if (data.UnmanagedInfo.UserDataSize >= int.MaxValue)
                {
                    throw new InvalidOperationException("UserDataSize is too large to load.");
                }

                // Deserialize user data class passed to CoreLoad
                data.RemoteInfo = formatter.DeserializeClass<U>(
                    data.UnmanagedInfo.UserData,
                    data.UnmanagedInfo.UserDataSize);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
            return data;
        }
    }
}
