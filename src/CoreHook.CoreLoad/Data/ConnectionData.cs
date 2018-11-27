using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CoreHook.CoreLoad.Data
{
    internal class ConnectionData<T, U> where T : IRemoteEntryInfo
    {
        /// <summary>
        /// Gets the state of the current <see cref="ConnectionData{T,U}"/>.
        /// </summary>
        internal PluginInitializationState State { get; private set; }

        /// <summary>
        /// Gets the unmanaged data containing the pointer to the memory block containing a class of type <see cref="T"/>;
        /// </summary>
        internal T UnmanagedInfo { get; private set; }

        internal U RemoteInfo { get; private set; }

        private ConnectionData()
        {
            State = PluginInitializationState.Invalid;
            RemoteInfo = default(U);
            UnmanagedInfo = default(T);
        }

        /// <summary>
        /// Loads <see cref="ConnectionData{T,U}"/> from the <see cref="IntPtr"/> specified
        /// in <paramref name="unmanagedInfoPointer"/>.
        /// </summary>
        /// <param name="unmanagedInfoPointer">Pointer to the user arguments data.</param>
        /// <param name="formatter">Deserializer for the user arguments data.</param>
        public static ConnectionData<T, U> LoadData(
            IntPtr unmanagedInfoPointer,
            IUserDataFormatter formatter)
        {
            var data = new ConnectionData<T, U>
            {
                State = PluginInitializationState.Valid,
                UnmanagedInfo = (T)Activator.CreateInstance(typeof(T))
            };
            try
            {
                // Get the unmanaged data containing the remote user parameters
                Marshal.PtrToStructure(unmanagedInfoPointer, data.UnmanagedInfo);

                // Deserialize user data class passed to CoreLoad
                data.RemoteInfo = formatter.Deserialize<U>(
                    data.UnmanagedInfo.UserData,
                    data.UnmanagedInfo.UserDataSize);
            }
            catch (Exception exception)
            {
                data.State = PluginInitializationState.Invalid;
                Debug.WriteLine(exception.ToString());
            }
            return data;
        }
    }
}
