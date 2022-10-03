using System;
using System.IO;

namespace GameCore
{
    public static class Alignment
    {
        public static Int64 Align(Int64 position, Int64 alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }
        public static bool IsAligned(Int64 position, Int64 alignment)
        {
            Int64 newpos = (position + (alignment - 1)) & ~(alignment - 1);
            return newpos == position;
        }

        public static Int32 Align32(Int32 position, Int32 alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }

    }

    public class TextStream
    {
        private FileStream mFileStream;

        public TextStream(string filename)
        {
            Filename = filename;
        }

        public enum EMode
        {
            Read,
            Write,
        }

        public string Filename { get; private set; }
        public StreamWriter Writer { get; private set; }
        public StreamReader Reader { get; private set; }

        public bool Exists()
        {
            return File.Exists(Filename);
        }

        public bool Open(EMode mode)
        {
            bool success = false;
            FileStream stream = null;
            try
            {
                stream = new FileStream(Filename, (mode == EMode.Read) ? FileMode.Open : FileMode.Create, (mode == EMode.Read) ? FileAccess.Read : FileAccess.Write);
                switch (mode)
                {
                    case EMode.Write:
                        Writer = new StreamWriter(stream);
                        break;
                    case EMode.Read:
                        Reader = new StreamReader(stream);
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                mFileStream = stream;
                success = true;
            }

            return success;
        }

        public void Close()
        {
            if (Writer != null)
            {
                Writer.Flush();
                Writer.Close();
                mFileStream.Close();
            }
            else if (Reader != null)
            {
                Reader.Close();
                mFileStream.Close();
            }

            Writer = null;
            Reader = null;
            mFileStream = null;
        }
    }

    public class BinaryStream
    {
        private FileStream mFileStream;

        public BinaryStream(string filename, EEndian endian)
        {
            Filename = filename;
            Endian = endian;
        }

        public enum EMode
        {
            Read,
            Write,
        }

        private string Filename { get; }
        private EEndian Endian { get; }
        private IBinaryWriter Writer { get; set; }
        private IBinaryReader Reader { get; set; }

        public bool Open(EMode mode)
        {
            bool success = false;
            FileStream stream = null;
            try
            {
                stream = new FileStream(Filename, (mode == EMode.Read) ? FileMode.Open : FileMode.Create, (mode == EMode.Read) ? FileAccess.Read : FileAccess.Write);
                if (mode == EMode.Write) Writer = EndianUtils.CreateBinaryWriter(stream, Endian);
                else if (mode == EMode.Read) Reader = EndianUtils.CreateBinaryReader(stream, Endian);
            }
            catch (Exception)
            {
                stream = null;
            }
            finally
            {
                mFileStream = stream;
                success = true;
            }

            return success;
        }

        public void Close()
        {
            if (Writer != null)
            {
                Writer.Close();
                Writer = null;
                mFileStream.Close();
            }
            else if (Reader != null)
            {
                Reader.Close();
                Reader = null;
                mFileStream.Close();
            }

            mFileStream = null;
        }
    }

    public static class StreamUtils
    {
        private static Int64 Position(IBinaryWriter writer)
        {
            Int64 p = writer.Position;
            return p;
        }

        public static bool Aligned(IBinaryWriter writer, Int64 alignment)
        {
            Int64 p = Alignment.Align(writer.Position, alignment);
            return (p == writer.Position);
        }

        public static void Align(IBinaryWriter writer, Int64 alignment)
        {
            writer.Position = Alignment.Align(writer.Position, alignment);
        }
    }
}
