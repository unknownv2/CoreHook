using System;
using System.IO;
using CoreHook.BinaryInjection.Loader.Serialization;

namespace CoreHook.BinaryInjection.Loader
{
    /// <summary>
    /// Manages the arguments passed to the plugin loader.
    /// The loader transforms the arguments into 
    /// a managed configuration class and initializes the plugin.
    /// </summary>
    public class PluginConfigurationArguments : ISerializableObject
    {
        public bool Is64BitProcess;
        public IntPtr UserData;
        public int UserDataSize;

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Serialize information about the serialized class 
                // data that is passed to the remote function.
                if (Is64BitProcess)
                {
                    writer.Write(UserData.ToInt64());
                    writer.Write(UserDataSize);
                }
                else
                {
                    writer.Write(UserData.ToInt32());
                    writer.Write(UserDataSize);
                    // Add padding to fill the whole buffer.
                    writer.Write(new byte[4]);
                }
                return ms.ToArray();
            }
        }
    }
}
