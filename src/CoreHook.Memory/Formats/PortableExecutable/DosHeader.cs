using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class DosHeader
    {
        internal ushort e_magic{ get; }
        internal ushort e_cblp{ get; }
        internal ushort e_cp{ get; }
        internal ushort e_crlc{ get; }
        internal ushort e_cparhdr{ get; }
        internal ushort e_minalloc{ get; }
        internal ushort e_maxalloc{ get; }
        internal ushort e_ss{ get; }
        internal ushort e_sp{ get; }
        internal ushort e_csum{ get; }
        internal ushort e_ip{ get; }
        internal ushort e_cs{ get; }
        internal ushort e_lfarlc{ get; }
        internal ushort e_ovno{ get; }
        internal ushort[] e_res1{ get; }
        internal ushort e_oemid{ get; }
        internal ushort e_oeminfo{ get; }
        internal ushort[] e_res2{ get; }
        internal uint e_lfanew{ get; }

        private const int res1_count = 4;
        private const int res2_count = 10;

        internal DosHeader(BinaryReader reader)
        {
            e_magic = reader.ReadUInt16();
            e_cblp = reader.ReadUInt16();
            e_cp = reader.ReadUInt16();
            e_crlc = reader.ReadUInt16();
            e_cparhdr = reader.ReadUInt16();
            e_minalloc = reader.ReadUInt16();
            e_maxalloc = reader.ReadUInt16();
            e_ss = reader.ReadUInt16();
            e_sp = reader.ReadUInt16();
            e_csum = reader.ReadUInt16();
            e_ip = reader.ReadUInt16();
            e_cs = reader.ReadUInt16();
            e_lfarlc = reader.ReadUInt16();
            e_ovno = reader.ReadUInt16();
            e_res1 = new ushort[res1_count];
            for (int i = 0; i < res1_count; i++)
            {
                e_res1[i] = reader.ReadUInt16();
            }

            e_oemid = reader.ReadUInt16();
            e_oeminfo = reader.ReadUInt16();
            e_res2 = new ushort[res2_count];
            for (int i = 0; i < res2_count; i++)
            {
                e_res2[i] = reader.ReadUInt16();
            }

            e_lfanew = reader.ReadUInt16();
        }
    }
}
