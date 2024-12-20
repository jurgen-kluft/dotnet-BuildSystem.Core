
namespace GameCore
{
    public interface IBinaryReader
    {
        IArchitecture Architecture { get; }
        int Read(byte[] buffer, int offset, int count);
    }

    public interface IStreamReader : IBinaryStream, IBinaryReader
    {
    }

    public sealed class FileStreamReader : IStreamReader
    {
        private FileStream _fileStream;

        public FileStreamReader() : this(ArchitectureUtils.LittleArchitecture64)
        {
        }

        public FileStreamReader(IArchitecture architecture)
        {
            Architecture = architecture;
        }

        public FileStreamReader(FileStream fs, IArchitecture architecture)
        {
            _fileStream = fs;
            Architecture = architecture;
        }

        public FileStreamReader(FileStream fs) : this(fs, ArchitectureUtils.LittleArchitecture64)
        {
        }

        public bool Open(string filepath)
        {
            if (!File.Exists(filepath)) return false;

            _fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            return true;

        }

        public void Close()
        {
            if (_fileStream == null) return;

            _fileStream.Close();
        }

        public IArchitecture Architecture { get; set; }

        public long Position
        {
            get => _fileStream.Position;
            set => _fileStream.Position = value;
        }

        public long Length
        {
            get => _fileStream.Length;
            set => _fileStream.Position = value;
        }

        public long Seek(long offset)
        {
            return _fileStream.Seek(offset, SeekOrigin.Begin);
        }

        public bool SkipBytes(long numBytes)
        {
            _fileStream.Position += numBytes;
            return true;
        }

        public int Read(byte[] data, int offset, int size)
        {
            return _fileStream.Read(data, offset, size);
        }
    }


    public static class BinaryReader
    {
        private static readonly byte[] s_buffer = new byte[256];

        public static void Read(IBinaryReader reader, out sbyte v)
        {
            reader.Read(s_buffer, 0, 1);
            v = (sbyte)s_buffer[0];
        }

        public static void Read(IBinaryReader reader, out byte v)
        {
            reader.Read(s_buffer, 0, 1);
            v = s_buffer[0];
        }

        public static void Read(IBinaryReader reader, out short v)
        {
            reader.Read(s_buffer, 0, 2);
            v = reader.Architecture.ReadInt16(s_buffer, 0);
        }

        public static void Read(IBinaryReader reader, out ushort v)
        {
            reader.Read(s_buffer, 0, 2);
            v = reader.Architecture.ReadUInt16(s_buffer, 0);
        }

        public static void Read(IBinaryReader reader, out int v)
        {
            reader.Read(s_buffer, 0, 4);
            v = reader.Architecture.ReadInt32(s_buffer, 0);
        }

        public static void Read(IBinaryReader reader, out uint v)
        {
            reader.Read(s_buffer, 0, 4);
            v = reader.Architecture.ReadUInt32(s_buffer, 0);
        }

        public static void Read(IBinaryReader reader, out long v)
        {
            reader.Read(s_buffer, 0, 8);
            v = reader.Architecture.ReadInt64(s_buffer, 0);
        }

        public static void Read(IBinaryReader reader, out ulong v)
        {
            reader.Read(s_buffer, 0, 8);
            v = reader.Architecture.ReadUInt64(s_buffer, 0);
        }

        public static void Read(IBinaryReader reader, out float v)
        {
            reader.Read(s_buffer, 0, 4);
            v = reader.Architecture.ReadFloat(s_buffer, 0);
        }

        public static void Read(IBinaryReader reader, out double v)
        {
            reader.Read(s_buffer, 0, 8);
            v = reader.Architecture.ReadDouble(s_buffer, 0);
        }

        public static void Read(IBinaryReader reader, byte[] data, int index, int length)
        {
            reader.Read(data, index, length);
        }

        public static void Read(IBinaryReader reader, out string v)
        {
            Read(reader, out int byteCount);
            if (byteCount < s_buffer.Length)
            {
                reader.Read(s_buffer, 0, byteCount);
                v = System.Text.Encoding.UTF8.GetString(s_buffer, 0, byteCount);
            }
            else
            {
                var buffer = new byte[byteCount];
                reader.Read(buffer, 0, byteCount);
                v = System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
            }
        }
    }
}
