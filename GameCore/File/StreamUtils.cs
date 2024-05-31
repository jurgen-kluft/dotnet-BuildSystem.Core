using System;
using System.IO;
using ActorSystem = DataBuildSystem.ActorFlow.System;

namespace GameCore
{
    public static partial class  CMath
    {
        public static int AlignUp(int position, int alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }
        public static long AlignUp(long position, int alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }
        public static long AlignUp(long position, uint alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }
        public static ulong AlignUp(ulong position, uint alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }
        public static bool TryAlignUp(long position, long alignment, out long aligned)
        {
            aligned = (position + (alignment - 1)) & ~(alignment - 1);
            return aligned == position;
        }
        public static bool IsAligned(long position, long alignment)
        {
            var newPos = (position + (alignment - 1)) & ~(alignment - 1);
            return newPos == position;
        }
        public static int AlignUp32(int position, int alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }
        public static uint AlignUp32(uint position, uint alignment)
        {
            return (position + (alignment - 1)) & ~(alignment - 1);
        }
        public static bool TryAlignUp32(int position, int alignment, out int aligned)
        {
            aligned = (position + (alignment - 1)) & ~(alignment - 1);
            return aligned == position;
        }

    }

    public sealed class TextStream
    {
        public static StreamReader OpenForRead(string filename)
        {
            if (!File.Exists(filename))
                return null;
            try
            {
                var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                return new StreamReader(stream);
            }
            catch (Exception e)
            {
                // ignored
            }

            return null;
        }

        public static StreamWriter OpenForWrite(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    var stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    return new StreamWriter(stream);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
            return new StreamWriter(new MemoryStream(Array.Empty<byte>()));
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
            var success = false;
            FileStream stream = null;
            try
            {
                stream = new FileStream(Filename, (mode == EMode.Read) ? FileMode.Open : FileMode.Create, (mode == EMode.Read) ? FileAccess.Read : FileAccess.Write);
                if (mode == EMode.Write) Writer = ArchitectureUtils.CreateBinaryWriter(stream, Platform);
                else if (mode == EMode.Read) Reader = ArchitectureUtils.CreateBinaryReader(stream, Platform);
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
        private static long Position(IBinaryStream writer)
        {
            var p = writer.Position;
            return p;
        }

        public static bool Aligned(IBinaryStream writer, int alignment)
        {
            var p = CMath.AlignUp(writer.Position, alignment);
            return (p == writer.Position);
        }

        public static void Align(IBinaryStream writer, int alignment)
        {
            writer.Position = CMath.AlignUp(writer.Position, alignment);
        }
    }
}
