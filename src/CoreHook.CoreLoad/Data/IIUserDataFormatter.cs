using System;

namespace CoreHook.CoreLoad.Data
{
    interface IUserDataFormatter
    {
        T DeserializeClass<T>(IntPtr data, int size);
    }
}
