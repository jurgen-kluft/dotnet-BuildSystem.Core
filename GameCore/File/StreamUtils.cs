using System;
using System.IO;
using ActorSystem = DataBuildSystem.ActorFlow.System;

namespace GameCore
{
    public static partial class  CMath
    {
        public static Int64 Align(Int64 position, Int64 alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }
        public static bool TryAlign(Int64 position, Int64 alignment, out Int64 aligned)
        {
            aligned = (position + (alignment - 1)) & ~(alignment - 1);
            return aligned == position;
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
        public static bool TryAlign32(Int32 position, Int32 alignment, out Int32 aligned)
        {
            aligned = (position + (alignment - 1)) & ~(alignment - 1);
            return aligned == position;
        }

    }

    public sealed class TextStream
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
        public System.IO.StreamWriter Writer { get; private set; }
        public System.IO.StreamReader Reader { get; private set; }

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
                        Writer = new System.IO.StreamWriter(stream);
                        break;
                    case EMode.Read:
                        Reader = new System.IO.StreamReader(stream);
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

    public sealed class BinaryFileStream
    {
        private FileStream mFileStream;

        public BinaryFileStream(string filename, EPlatform platform)
        {
            Filename = filename;
            Platform = platform;
        }

        public enum EMode
        {
            Read,
            Write,
        }

        private string Filename { get; }
        private EPlatform Platform { get; }
        private IBinaryWriter Writer { get; set; }
        private IBinaryReader Reader { get; set; }

        public bool Open(EMode mode)
        {
            bool success = false;
            FileStream stream = null;
            try
            {
                stream = new FileStream(Filename, (mode == EMode.Read) ? FileMode.Open : FileMode.Create, (mode == EMode.Read) ? FileAccess.Read : FileAccess.Write);
                if (mode == EMode.Write) Writer = EndianUtils.CreateBinaryWriter(stream, Platform);
                else if (mode == EMode.Read) Reader = EndianUtils.CreateBinaryReader(stream, Platform);
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
                Writer = null;
                mFileStream.Close();
            }
            else if (Reader != null)
            {
                Reader = null;
                mFileStream.Close();
            }

            mFileStream = null;
        }
    }

    public static class StreamUtils
    {
        private static Int64 Position(IBinaryStream writer)
        {
            var p = writer.Position;
            return p;
        }

        public static bool Aligned(IBinaryStream writer, Int64 alignment)
        {
            var p = CMath.Align(writer.Position, alignment);
            return (p == writer.Position);
        }

        public static void Align(IBinaryStream writer, Int64 alignment)
        {
            writer.Position = CMath.Align(writer.Position, alignment);
        }
    }
}
