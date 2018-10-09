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
        void Serialize(MemoryStream stream);
    }
    public class RemoteFunctionArgs : IBinarySerializer
    {
        public bool Is64BitProcess;
        public IntPtr UserData;
        public uint UserDataSize;
        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(Is64BitProcess ? UserData.ToInt64() : UserData.ToInt32());
                    writer.Write(UserDataSize);
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
