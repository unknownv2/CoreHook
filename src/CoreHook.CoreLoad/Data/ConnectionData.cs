using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace CoreHook.CoreLoad.Data
{
    internal class ConnectionData<T, U> where T : IRemoteEntryInfo
    {
        public enum ConnectionState
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
                // Get the unmanaged data
                Marshal.PtrToStructure(unmanagedInfoPointer, data.UnmanagedInfo);

                if (data.UnmanagedInfo.UserDataSize >= int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("UserDataSize");
                }

                // Deserialize user data class passed to CoreLoad
                data.RemoteInfo = deserializer.DeserializeClass(
                    data.UnmanagedInfo.UserData,
                    data.UnmanagedInfo.UserDataSize);
                /*
                using (Stream passThruStream = new MemoryStream())
                {
                    if (data.UnmanagedInfo.UserDataSize >= int.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("UserDataSize");
                    }

                    byte[] passThruBytes = new byte[data.UnmanagedInfo.UserDataSize];

                    Marshal.Copy(data.UnmanagedInfo.UserData, passThruBytes, 0, data.UnmanagedInfo.UserDataSize);

                    passThruStream.Write(passThruBytes, 0, passThruBytes.Length);

                    passThruStream.Position = 0;

                    data.RemoteInfo = DeserializeClass(passThruStream);
                    //DeserializeClass<ManagedRemoteInfo>(passThruStream);
                    /*
                    BinaryFormatter format = new BinaryFormatter
                    {
                        Binder = new AllowAllAssemblyVersionsDeserializationBinder()
                    };
                    object remoteInfo = format.Deserialize(passThruStream);
                    if(remoteInfo is ManagedRemoteInfo info)
                    {
                        data.RemoteInfo = info;
                    }
                    else
                    {
                        throw new InvalidCastException($"Deserialized data was not of type {nameof(ManagedRemoteInfo)}");
                    }/
                }*/
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
            return data;
        }

        
        public static ConnectionData<T, U> LoadData(IntPtr unmanagedInfoPointer)
        {
            var data = new ConnectionData<T, U>
            {
                State = ConnectionState.Valid,
                UnmanagedInfo = (T)Activator.CreateInstance(typeof(T))
            };
            try
            {
                // Get the unmanaged data
                Marshal.PtrToStructure(unmanagedInfoPointer, data.UnmanagedInfo);
                using (Stream passThruStream = new MemoryStream())
                {
                    if(data.UnmanagedInfo.UserDataSize >= int.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("UserDataSize");
                    }

                    byte[] passThruBytes = new byte[data.UnmanagedInfo.UserDataSize];

                    Marshal.Copy(data.UnmanagedInfo.UserData, passThruBytes, 0, data.UnmanagedInfo.UserDataSize);

                    passThruStream.Write(passThruBytes, 0, passThruBytes.Length);

                    passThruStream.Position = 0;

                    data.RemoteInfo = DeserializeClass(passThruStream);
                    //DeserializeClass<ManagedRemoteInfo>(passThruStream);
                    /*
                    BinaryFormatter format = new BinaryFormatter
                    {
                        Binder = new AllowAllAssemblyVersionsDeserializationBinder()
                    };
                    object remoteInfo = format.Deserialize(passThruStream);
                    if(remoteInfo is ManagedRemoteInfo info)
                    {
                        data.RemoteInfo = info;
                    }
                    else
                    {
                        throw new InvalidCastException($"Deserialized data was not of type {nameof(ManagedRemoteInfo)}");
                    }*/
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
            return data;
        }

        public static U DeserializeClass(Stream binaryStream)
        {
            var format = new BinaryFormatter
            {
                Binder = new AllowAllAssemblyVersionsDeserializationBinder()
            };
            object remoteInfo = format.Deserialize(binaryStream);
            if (remoteInfo is U info)
            {
                return info;
            }
            else
            {
                throw new InvalidCastException($"Deserialized data was not of type {nameof(U)}");
            }
        }
    }
}
