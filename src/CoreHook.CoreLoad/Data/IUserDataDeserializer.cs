using System;

namespace CoreHook.CoreLoad.Data
{
    interface IUserDataFormatter<T>
    {
        T DeserializeClass(IntPtr data, int size);
    }
}
