using System.IO;

namespace CoreHook.BinaryInjection.BinaryLoader.Serializer
{
    public interface IBinarySerializer
    {
        byte[] Serialize();
    }
    public interface IBinaryStreamSerializer
    {
        void Serialize(MemoryStream stream);
    }
}
