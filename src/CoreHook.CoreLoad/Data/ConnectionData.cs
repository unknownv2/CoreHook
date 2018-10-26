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
        /// Gets the unmanaged data containing the pointer to the memory block containing <see cref="RemoteInfo"/>;
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
        public static ConnectionData<T, U> LoadData(
            IntPtr unmanagedInfoPointer, 
            IUserDataFormatter<U> deserializer)
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
                    throw new ArgumentOutOfRangeException("UserDataSize");
                }

                // Deserialize user data class passed to CoreLoad
                data.RemoteInfo = deserializer.DeserializeClass(
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
