using System;
using System.IO;

namespace CoreHook.CoreLoad.Data
{
    public interface IUserDataFormatter
    {
        T Deserialize<T>(IntPtr data, int size);
        T Deserialize<T>(Stream stream);
        void Serialize(Stream serializationStream, object graph);
    }
}
