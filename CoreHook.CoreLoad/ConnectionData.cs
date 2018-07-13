using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Diagnostics;

namespace CoreHook.CoreLoad
{
    internal class ConnectionData
    {
        public enum ConnectionState
        {
            Invalid = 0,
            NoChannel = 1,
            Valid = Int32.MaxValue
        }

        private RemoteEntryInfo _unmanagedInfo;
        private ManagedRemoteInfo _remoteInfo;
        private ConnectionState _state;

        /// <summary>
        /// Gets the state of the current <see cref="HostConnectionData"/>.
        /// </summary>
        public ConnectionState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the unmanaged data containing the pointer to the memory block containing <see cref="RemoteInfo"/>;
        /// </summary>
        public RemoteEntryInfo UnmanagedInfo
        {
            get { return _unmanagedInfo; }
        }

        public ManagedRemoteInfo RemoteInfo
        {
            get { return _remoteInfo; }
        }

        private ConnectionData()
        {
            _state = ConnectionState.Invalid;
            _remoteInfo = null;
            _unmanagedInfo = null;
        }
        /// <summary>
        /// Loads <see cref="HostConnectionData"/> from the <see cref="IntPtr"/> specified.
        /// </summary>
        /// <param name="unmanagedInfoPointer"></param>
        public static ConnectionData LoadData(IntPtr unmanagedInfoPointer)
        {
            var data = new ConnectionData
            {
                _state = ConnectionState.Valid,
                _unmanagedInfo = new RemoteEntryInfo()
            };
            try
            {
                // Get the unmanaged data
                Marshal.PtrToStructure(unmanagedInfoPointer, data._unmanagedInfo);
                using (Stream passThruStream = new MemoryStream())
                {
                    byte[] passThruBytes = new byte[data._unmanagedInfo.UserDataSize];
                    BinaryFormatter format = new BinaryFormatter();
                    // Workaround for deserialization when not using GAC registration
                    format.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                    Marshal.Copy(data._unmanagedInfo.UserData, passThruBytes, 0, data._unmanagedInfo.UserDataSize);
                    passThruStream.Write(passThruBytes, 0, passThruBytes.Length);
                    passThruStream.Position = 0;
                    data._remoteInfo = (ManagedRemoteInfo)format.Deserialize(passThruStream);
                }
            }
            catch (Exception ExtInfo)
            {
                Debug.WriteLine(ExtInfo.ToString());
            }
            return data;
        }

    }
}
