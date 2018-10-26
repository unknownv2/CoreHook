using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace CoreHook.CoreLoad.Data
{
    internal class UserDataBinaryFormatter : IUserDataFormatter
    {
        internal UserDataBinaryFormatter() { }

        public T DeserializeClass<T>(IntPtr data, int size)
        {
            using (Stream passThruStream = new MemoryStream())
            {
                byte[] passThruBytes = new byte[size];

                Marshal.Copy(data, passThruBytes, 0, size);

                passThruStream.Write(passThruBytes, 0, passThruBytes.Length);

                passThruStream.Position = 0;

                return DeserializeClass<T>(passThruStream);
            }
        }

        internal static T DeserializeClass<T>(Stream binaryStream)
        {
            var format = new BinaryFormatter
            {
                Binder = new AllowAllAssemblyVersionsDeserializationBinder()
            };

            object remoteInfo = format.Deserialize(binaryStream);
            if (remoteInfo is T info)
            {
                return info;
            }
            else
            {
                throw new InvalidCastException($"Deserialized data was not of type {nameof(T)}");
            }
        }
    }
}
