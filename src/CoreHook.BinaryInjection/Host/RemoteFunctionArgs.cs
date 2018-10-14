using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.IO;

namespace CoreHook.BinaryInjection
{
    public interface IBinarySerializer
    {
        byte[] Serialize();
    }
    public interface IBinaryStreamSerializer
    {
        void Serialize(MemoryStream stream);
    }
    public class RemoteFunctionArgs : IBinarySerializer
    {
        public bool Is64BitProcess;
        public IntPtr UserData;
        public int UserDataSize;
        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
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
                }
                return ms.ToArray();
            }
        }

        public void Serialize(MemoryStream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                if(Is64BitProcess)
                {
                    writer.Write(UserData.ToInt64());
                }
                else
                {
                    writer.Write(UserData.ToInt32());
                }
                writer.Write(UserDataSize);
            }
        }
    }
}
