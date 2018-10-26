using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace CoreHook.CoreLoad.Data
{
    internal class UserDataBinaryFormatter<T> : IUserDataFormatter<T>
    {
        internal UserDataBinaryFormatter() { }

        public T DeserializeClass(IntPtr data, int size)
        {
            using (Stream passThruStream = new MemoryStream())
            {
                byte[] passThruBytes = new byte[size];

                Marshal.Copy(data, passThruBytes, 0, size);

                passThruStream.Write(passThruBytes, 0, passThruBytes.Length);

                passThruStream.Position = 0;

                return DeserializeClass(passThruStream);
            }
        }

        public static T DeserializeClass(Stream binaryStream)
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

        public T DeserializeClass(IFormatter formatter, Stream stream)
        {
            return default(T);
        }
    }
}
