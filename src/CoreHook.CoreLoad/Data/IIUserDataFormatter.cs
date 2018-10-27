using System;
using System.IO;

namespace CoreHook.CoreLoad.Data
{
    public interface IUserDataFormatter
    {
        T Deserialize<T>(IntPtr data, int size);
        void Serialize(Stream serializationStream, object graph);
    }
}
