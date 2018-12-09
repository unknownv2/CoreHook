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
    internal class PluginConfigurationArguments : ISerializableObject
    {
        private readonly bool _is64BitProcess;
        private readonly IntPtr _userData;
        private readonly int _userDataSize;

        internal PluginConfigurationArguments(bool is64BitProcess, IntPtr userData, int userDataSize)
        {
            _is64BitProcess = is64BitProcess;
            _userData = userData;
            _userDataSize = userDataSize;
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Store the address and size of the serialized plugin arguments.
                if (_is64BitProcess)
                {
                    writer.Write(_userData.ToInt64());
                    writer.Write(_userDataSize);
                }
                else
                {
                    writer.Write(_userData.ToInt32());
                    writer.Write(_userDataSize);
                    // Add padding to fill the whole buffer.
                    writer.Write(new byte[4]);
                }
                return ms.ToArray();
            }
        }
    }
}
