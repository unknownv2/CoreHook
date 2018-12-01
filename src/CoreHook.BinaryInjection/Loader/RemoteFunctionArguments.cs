using System;
using System.IO;
using CoreHook.BinaryInjection.Loader.Serializers;

namespace CoreHook.BinaryInjection.Loader
{
    public class RemoteFunctionArguments : IBinarySerializer
    {
        public bool Is64BitProcess;
        public IntPtr UserData;
        public int UserDataSize;

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // serialize information about the serialized class 
                // data that is passed to the remote function
                if (Is64BitProcess)
                {
                    writer.Write(UserData.ToInt64());
                    writer.Write(UserDataSize);
                }
                else
                {
                    writer.Write(UserData.ToInt32());
                    writer.Write(UserDataSize);
                    // add padding to fill the whole buffer
                    writer.Write(new byte[4]);
                }
                return ms.ToArray();
            }
        }
    }
}
