using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace CoreHook.CoreLoad.Data
{
    public class UserDataBinaryFormatter : IUserDataFormatter
    {
        private readonly IFormatter _formatter;
        public UserDataBinaryFormatter()
        {
            _formatter = new BinaryFormatter
            {
                Binder = new AllowAllAssemblyVersionsDeserializationBinder()
            };
        }

        public UserDataBinaryFormatter(IFormatter formatter)
        {
            _formatter = formatter;
        }

        public T Deserialize<T>(IntPtr data, int size)
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

        private T DeserializeClass<T>(Stream binaryStream)
        {
            object remoteInfo = _formatter.Deserialize(binaryStream);
            if (remoteInfo is T info)
            {
                return info;
            }
            else
            {
                throw new InvalidCastException($"Deserialized data was not of type {nameof(T)}");
            }
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            _formatter.Serialize(serializationStream, graph);
        }
    }
}
