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

        public UserDataBinaryFormatter(IFormatter formatter = null)
        {
            _formatter = formatter ?? GetDefaultFormatter();
        }

        private IFormatter GetDefaultFormatter()
        {
            return new BinaryFormatter
            {
                Binder = new AllowAllAssemblyVersionsDeserializationBinder()
            };
        }

        public T Deserialize<T>(Stream stream)
        {
            return DeserializeClass<T>(stream);
        }

        public T Deserialize<T>(IntPtr data, int size)
        {
            if (data == IntPtr.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(data),
                    "Invalid data address");
            }

            if (size >= int.MaxValue)
            {
                throw new InvalidOperationException("Data size is too large for deserializing");
            }

            byte[] objectData = new byte[size];

            Marshal.Copy(data, objectData, 0, size);

            using (Stream stream = new MemoryStream(objectData))
            {
                return DeserializeClass<T>(stream);
            }
        }

        private T DeserializeClass<T>(Stream binaryStream)
        {
            object remoteInfo = _formatter.Deserialize(binaryStream);
            if (remoteInfo is T info)
            {
                return info;
            }

            throw new InvalidCastException($"Deserialized data was not of type {nameof(T)}");
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            _formatter.Serialize(serializationStream, graph);
        }
    }
}
