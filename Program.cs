using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace minecraftloctool
{
    internal class Program
    {
        class BEBinaryWriter : BinaryWriter
        {
            public BEBinaryWriter(Stream output) : base(output) { }

            public override void Write(UInt16 value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                base.Write(bytes);
            }

            public override void Write(Int32 value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                base.Write(bytes);
            }

            public override void Write(UInt32 value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                base.Write(bytes);
            }

            public override void Write(Int64 value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                base.Write(bytes);
            }

            public override void Write(UInt64 value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                base.Write(bytes);
            }
        }

        class BEBinaryReader : BinaryReader
        {
            public BEBinaryReader(Stream input) : base(input) { }
            public override ushort ReadUInt16()
            {
                byte[] bytes = base.ReadBytes(2);
                Array.Reverse(bytes);
                return BitConverter.ToUInt16(bytes, 0);
            }
            public override int ReadInt32()
            {
                byte[] bytes = base.ReadBytes(4);
                Array.Reverse(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
            public override ulong ReadUInt64()
            {
                byte[] bytes = base.ReadBytes(8);
                Array.Reverse(bytes);
                return BitConverter.ToUInt64(bytes, 0);
            }

        }
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("USAGE:\nExtract: tool.exe file.loc\nRebuild: tool.exe file.loc directoryWithFiles");
                Console.ReadLine();
                return;
            }
            if (args.Length == 1)
            {
                Extract(args[0]);
            }
            else
            {
                Rebuild(args[0], args[1]);
            }
        }
        public static void Extract(string file)
        {
            string dir = Path.GetFileNameWithoutExtension(file) + "\\";
            Directory.CreateDirectory(Path.GetFileNameWithoutExtension(file));
            var reader = new BEBinaryReader(File.OpenRead(file));
            bool IsBigEndian = reader.ReadInt32() == 2 ? true : false;
            int LangCount = reader.ReadInt32();
            string[] langs = new string[LangCount];
            bool IsLineIDS = reader.ReadByte() == 1 ? true : false;
            int LineIDcount = reader.ReadInt32();
            string[] LineID = new string[LineIDcount / 2];
            for (int i = 0; i < LineIDcount / 2; i++)
            {
                LineID[i] = reader.ReadUInt64().ToString();
            }
            reader.ReadInt32();
            for (int i = 0; i < LangCount; i++)
            {
                int len = reader.ReadUInt16();
                langs[i] = Encoding.UTF8.GetString(reader.ReadBytes(len));
                reader.ReadInt32();
            }
            Console.WriteLine();
            for (int i = 0; i < LangCount; i++)
            {
                reader.BaseStream.Position += 5;
                int langLen = reader.ReadUInt16();
                string language = Encoding.UTF8.GetString(reader.ReadBytes(langLen));
                int count = reader.ReadInt32();
                string[] strings = new string[count];
                for (int a = 0; a < count; a++)
                {
                    int len = reader.ReadUInt16();
                    strings[a] = Encoding.UTF8.GetString(reader.ReadBytes(len)).Replace("\n", "<lf>").Replace("\r", "<br>");
                }
                File.WriteAllLines(dir + language + ".txt", strings);
            }
        }
        public static void Rebuild(string file, string dir)
        {
            string[] files = Directory.GetFiles(dir, "*.txt", SearchOption.TopDirectoryOnly);
            string[] LangNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
                LangNames[i] = fileNameWithoutExtension;
                Console.WriteLine(fileNameWithoutExtension);
            }
            var reader = new BEBinaryReader(File.OpenRead(file));
            bool IsBigEndian = reader.ReadInt32() == 2 ? true : false;
            int LangCount = reader.ReadInt32();
            if (LangCount != files.Length)
            {
                throw new Exception($"Files bigger or smaller of the languages in the {file}");
            }
            string[] langs = new string[LangCount];
            bool IsLineIDS = reader.ReadByte() == 1 ? true : false;
            int LineIDcount = reader.ReadInt32();
            string[] LineID = new string[LineIDcount / 2];
            for (int i = 0; i < LineIDcount / 2; i++)
            {
                LineID[i] = reader.ReadUInt64().ToString();
            }
            reader.ReadInt32();
            for (int i = 0; i < LangCount; i++)
            {
                int len = reader.ReadUInt16();
                langs[i] = Encoding.UTF8.GetString(reader.ReadBytes(len));
                reader.ReadInt32();
            }
            int pos = (int)reader.BaseStream.Position;
            reader.Close();
            var writer = new BEBinaryWriter(File.OpenWrite(file));
            writer.BaseStream.Position = pos;
            for (int i = 0; i < LangCount; i++)
            {
                writer.BaseStream.Position += 5;
                writer.Write((ushort)Encoding.UTF8.GetBytes(LangNames[i]).Length);
                writer.Write(Encoding.UTF8.GetBytes(LangNames[i]));
                string[] strings = File.ReadAllLines(files[i]);
                writer.Write((uint)strings.Length);
                for (int a = 0; a < strings.Length; a++)
                {
                    writer.Write((ushort)Encoding.UTF8.GetBytes(strings[a]).Length);
                    writer.Write(Encoding.UTF8.GetBytes(strings[a]));
                }
            }
        }
    }
}
