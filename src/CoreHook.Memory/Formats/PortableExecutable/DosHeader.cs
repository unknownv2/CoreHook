using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class DosHeader
    {
        public ushort e_magic;
        public ushort e_cblp;
        public ushort e_cp;
        public ushort e_crlc;
        public ushort e_cparhdr;
        public ushort e_minalloc;
        public ushort e_maxalloc;
        public ushort e_ss;
        public ushort e_sp;
        public ushort e_csum;
        public ushort e_ip;
        public ushort e_cs;
        public ushort e_lfarlc;
        public ushort e_ovno;
        public ushort[] e_res1;
        public ushort e_oemid;
        public ushort e_oeminfo;
        public ushort[] e_res2;
        public uint e_lfanew;

        const int res1_count = 4;
        const int res2_count = 10;

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
