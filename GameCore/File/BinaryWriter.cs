using System.Diagnostics;

namespace GameCore
{
    public interface IWriter
    {
        IArchitecture Architecture { get; }

        void Write(byte[] data, int index, int count);
    }

    public interface IStream
    {
        long Position { get; set; }
        long Length { get; set; }

        long Seek(long offset);
        void Close();
    }

    public interface IStreamWriter : IStream, IWriter
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

    public class DataStream : IDataStream
    {
        private readonly MemoryStream _stream;
        private readonly byte[] _buffer = new byte[256];

        public DataStream(MemoryStream stream, IArchitecture architecture)
        {
            _stream = stream;
            Architecture = architecture;
        }

        public DataStream(MemoryStream stream) : this(stream, ArchitectureUtils.LittleArchitecture64)
        {
        }

        public void Reset()
        {
            _stream.SetLength(8192);
            _stream.Position = 0;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public void Write(byte[] data, int index, int count)
        {
            _stream.Write(data, index, count);
        }

        public IArchitecture Architecture { get; set; }

        public ReadOnlySpan<byte> Data { get { return _stream.GetBuffer(); } }

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
            Memory = memory;
            Begin = offset;
            End = offset + length;
            Position = offset;
        }

        public byte[] Memory { get; set; }
        public int Begin{ get; set; }
        public int End{ get; set; }
        public int Cursor { get; set; }

        IArchitecture IWriter.Architecture { get { return Architecture; } }
        IArchitecture IBinaryReader.Architecture { get { return Architecture; } }
        public IArchitecture Architecture { get; init; }

        public long Position
        {
            get => Cursor;
            set => Cursor = (int)value;
        }

        public long Length
        {
            get => End;
            set => End = Begin + (int)value;
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
            var n = Math.Min(count, End - Cursor);
            Array.Copy(Memory, Cursor, buffer, offset, n);
            Position += n;
            return (int)n;
        }

        public void Write(byte[] data, int index, int count)
        {
            if (Position >= End)
                return;
            var n = Math.Min(count, End - Cursor);
            Array.Copy(data, index, Memory, Cursor, n);
            Position += n;
        }
    }

    public static class BinaryWriter
    {
        private static readonly byte[] s_buffer = new byte[1024];

        public static void Align(IStreamWriter stream, int alignment)
        {
            var alignedPos = (stream.Position + (alignment - 1)) & ~(long)(alignment - 1);
            if (alignedPos > stream.Position)
            {
                Debug.Assert(alignedPos - stream.Position < s_buffer.Length);
                var fillLength = (int)(alignedPos - stream.Position);
                Array.Fill<byte>(s_buffer, 0, 0, fillLength);
                stream.Write(s_buffer, 0, fillLength);
            }
        }

        public static void Write(IWriter writer, sbyte v)
        {
            s_buffer[0] = (byte)v;
            writer.Write(s_buffer, 0, 1);
        }

        public static void Write(IWriter writer, byte v)
        {
            s_buffer[0] = v;
            writer.Write(s_buffer, 0, 1);
        }

        public static void Write(IWriter writer, short v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, sizeof(short));
        }

        public static void Write(IWriter writer, ushort v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, sizeof(ushort));
        }

        public static void Write(IWriter writer, int v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, sizeof(int));
        }

        public static void Write(IWriter writer, uint v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, sizeof(uint));
        }

        public static void Write(IWriter writer, long v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, sizeof(long));
        }

        public static void Write(IWriter writer, ulong v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, sizeof(ulong));
        }

        public static void Write(IWriter writer, float v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, sizeof(float));
        }

        public static void Write(IWriter writer, double v)
        {
            writer.Architecture.Write(v, s_buffer, 0);
            writer.Write(s_buffer, 0, sizeof(double));
        }

        public static void Write(IWriter writer, byte[] data)
        {
            writer.Write(data, 0, data.Length);
        }

        public static void Write(IWriter writer, byte[] data, int index, int length)
        {
            writer.Write(data, index, length);
        }

        public static void Write(IWriter writer, ReadOnlySpan<byte> span)
        {
            writer.Write(span.ToArray(), 0, span.Length);
        }

        public static void Write(IWriter writer, string v)
        {
            var byteCount = System.Text.Encoding.UTF8.GetByteCount(v);
            Write(writer, byteCount);
            if (byteCount < s_buffer.Length)
            {
                System.Text.Encoding.UTF8.GetBytes(v, s_buffer);
                writer.Write(s_buffer, 0, byteCount);
            }
            else
            {
                var buffer = new byte[byteCount];
                System.Text.Encoding.UTF8.GetBytes(v, buffer);
                writer.Write(buffer, 0, byteCount);
            }
        }
    }
}
