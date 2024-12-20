using System.Diagnostics;

namespace GameCore
{
    public interface IBinaryWriter
    {
        IArchitecture Architecture { get; }

        void Write(byte[] data, int index, int count);
    }

    public interface IBinaryStream
    {
        long Position { get; set; }
        long Length { get; set; }

        long Seek(long offset);
        void Close();
    }

    public interface IStreamWriter : IBinaryStream, IBinaryWriter
    {
    }

    public interface IDataStream : IStreamReader, IStreamWriter
    {
        new IArchitecture Architecture { get; }
    }

    public class FileStreamWriter : IStreamWriter
    {
        private readonly FileStream _stream;
        private readonly byte[] _buffer = new byte[16];

        public FileStreamWriter(FileStream stream, IArchitecture architecture)
        {
            _stream = stream;
            Architecture = architecture;
        }

        public FileStreamWriter(FileStream stream) : this(stream, ArchitectureUtils.LittleArchitecture64)
        {
        }

        public void Write(byte[] data, int index, int count)
        {
            _stream.Write(data, index, count);
        }

        public IArchitecture Architecture { get; set; }

        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public long Length
        {
            get => _stream.Length;
            set => _stream.SetLength(value);
        }

        public long Seek(long offset)
        {
            return _stream.Seek(offset, SeekOrigin.Begin);
        }

        public void Close()
        {
            _stream.Close();
        }
    }

    public class MemoryWriter : IStreamWriter
    {
        private readonly MemoryStream _stream;
        private readonly byte[] _buffer = new byte[256];

        public MemoryWriter(MemoryStream stream, IArchitecture architecture)
        {
            _stream = stream;
            Architecture = architecture;
        }

        public MemoryWriter(MemoryStream stream) : this(stream, ArchitectureUtils.LittleArchitecture64)
        {
        }

        public void Reset()
        {
            _stream.Position = 0;
        }

        public void Write(byte[] data, int index, int count)
        {
            _stream.Write(data, index, count);
        }

        public IArchitecture Architecture { get; set; }

        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public long Length
        {
            get => _stream.Length;
            set => _stream.SetLength(value);
        }

        public long Seek(long offset)
        {
            return _stream.Seek(offset, SeekOrigin.Begin);
        }

        public void Close()
        {
            _stream.Close();
        }
    }

    public class MemoryBlock : IDataStream
    {
        public MemoryBlock(IArchitecture architecture)
        {
            Architecture = architecture;
        }

        public void Setup(byte[] memory, int offset, int length)
        {
            _memory = memory;
            _begin = offset;
            _end = offset + length;
            Position = offset;
        }

        private byte[] _memory;
        private int _begin;
        private int _end;
        private int _position;

        IArchitecture IBinaryWriter.Architecture { get { return Architecture; } }
        IArchitecture IBinaryReader.Architecture { get { return Architecture; } }
        public IArchitecture Architecture { get; init; }

        public long Position
        {
            get => _position;
            set => _position = (int)value;
        }

        public long Length
        {
            get => _end;
            set => _end = _begin + (int)value;
        }

        public long Seek(long offset)
        {
            Position = offset;
            return Position;
        }

        public void Close()
        {
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var n = Math.Min(count, _end - _position);
            Array.Copy(_memory, _position, buffer, offset, n);
            Position += n;
            return (int)n;
        }

        public void Write(byte[] data, int index, int count)
        {
            if (Position >= _end)
                return;
            var n = Math.Min(count, _end - _position);
            Array.Copy(data, index, _memory, _position, n);
            Position += n;
        }
    }

    public static class BinaryWriter
    {
        private static readonly byte[] s_buffer = new byte[256];

        public static void Write(IBinaryWriter writer, sbyte v)
        {
            s_buffer[0] = (byte)v;
            writer.Write(s_buffer, 0, 1);
        }

        public static void Write(IBinaryWriter writer, byte v)
        {
            s_buffer[0] = v;
            writer.Write(s_buffer, 0, 1);
        }

        public static void Write(IBinaryWriter writer, short v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, 2);
        }

        public static void Write(IBinaryWriter writer, ushort v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, 2);
        }

        public static void Write(IBinaryWriter writer, int v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, 4);
        }

        public static void Write(IBinaryWriter writer, uint v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, 4);
        }

        public static void Write(IBinaryWriter writer, long v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, 8);
        }

        public static void Write(IBinaryWriter writer, ulong v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, 8);
        }

        public static void Write(IBinaryWriter writer, float v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, 4);
        }

        public static void Write(IBinaryWriter writer, double v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, 8);
        }

        public static void Write(IBinaryWriter writer, byte[] data)
        {
            writer.Write(data, 0, data.Length);
        }

        public static void Write(IBinaryWriter writer, byte[] data, int index, int length)
        {
            writer.Write(data, index, length);
        }

        public static void Write(IBinaryWriter writer, ReadOnlySpan<byte> span)
        {
            writer.Write(span.ToArray(), 0, span.Length);
        }

        public static void Write(IBinaryWriter writer, string v)
        {
            var byteCount = System.Text.Encoding.UTF8.GetByteCount(v);
            if (byteCount < s_buffer.Length)
            {
                System.Text.Encoding.UTF8.GetBytes(v, s_buffer);
                Write(writer, byteCount);
                writer.Write(s_buffer, 0, byteCount);
            }
            else
            {
                var buffer = new byte[byteCount];
                System.Text.Encoding.UTF8.GetBytes(v, buffer);
                Write(writer, byteCount);
                writer.Write(buffer, 0, byteCount);
            }
        }
    }
}
