using System.Diagnostics;

namespace GameCore
{
    public interface IBinaryWriter
    {
        void Write(sbyte v);
        void Write(byte v);
        void Write(short v);
        void Write(ushort v);
        void Write(int v);
        void Write(uint v);
        void Write(long v);
        void Write(ulong v);
        void Write(float v);
        void Write(double v);
        void Write(byte[] data, int index, int count);
        void Write(ReadOnlySpan<byte> span);
        void Write(string v);
    }

    public interface IBinaryStream
    {
        IArchitecture Architecture { get; }

        long Position { get; set; }
        long Length { get; set; }

        long Seek(long offset);
        void Close();
    }

    public interface IBinaryStreamWriter : IBinaryStream, IBinaryWriter
    {
    }

    public interface IBinaryDataStream : IBinaryStreamReader, IBinaryStreamWriter
    {
    }

    public class BinaryStreamWriter : IBinaryStreamWriter
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer = new byte[32];

        public BinaryStreamWriter(Stream stream)
        {
            _stream = stream;
        }

        public void Write(sbyte v)
        {
            _stream.WriteByte((byte)v);
        }

        public void Write(byte v)
        {
            _stream.WriteByte(v);
        }

        public void Write(short v)
        {
            _buffer[0] = (byte)v;
            _buffer[1] = (byte)(v >> 8);
            _stream.Write(_buffer, 0, 2);
        }

        public void Write(ushort v)
        {
            _buffer[0] = (byte)v;
            _buffer[1] = (byte)(v >> 8);
            _stream.Write(_buffer, 0, 2);
        }

        public void Write(int v)
        {
            _buffer[0] = (byte)v;
            _buffer[1] = (byte)(v >> 8);
            _buffer[2] = (byte)(v >> 16);
            _buffer[3] = (byte)(v >> 24);
            _stream.Write(_buffer, 0, 4);
        }

        public void Write(uint v)
        {
            _buffer[0] = (byte)v;
            _buffer[1] = (byte)(v >> 8);
            _buffer[2] = (byte)(v >> 16);
            _buffer[3] = (byte)(v >> 24);
            _stream.Write(_buffer, 0, 4);
        }

        public void Write(long v)
        {
            _buffer[0] = (byte)v;
            _buffer[1] = (byte)(v >> 8);
            _buffer[2] = (byte)(v >> 16);
            _buffer[3] = (byte)(v >> 24);
            _buffer[4] = (byte)(v >> 32);
            _buffer[5] = (byte)(v >> 40);
            _buffer[6] = (byte)(v >> 48);
            _buffer[7] = (byte)(v >> 56);
            _stream.Write(_buffer, 0, 8);
        }

        public void Write(ulong v)
        {
            _buffer[0] = (byte)v;
            _buffer[1] = (byte)(v >> 8);
            _buffer[2] = (byte)(v >> 16);
            _buffer[3] = (byte)(v >> 24);
            _buffer[4] = (byte)(v >> 32);
            _buffer[5] = (byte)(v >> 40);
            _buffer[6] = (byte)(v >> 48);
            _buffer[7] = (byte)(v >> 56);
            _stream.Write(_buffer, 0, 8);
        }

        public void Write(float v)
        {
            if (BitConverter.TryWriteBytes(_buffer, v))
                _stream.Write(_buffer, 0, 4);
        }

        public void Write(double v)
        {
            if (BitConverter.TryWriteBytes(_buffer, v))
                _stream.Write(_buffer, 0, 8);
        }

        public void Write(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            _stream.Write(data, index, count);
        }

        public void Write(ReadOnlySpan<byte> span)
        {
            _stream.Write(span);
        }

        public void Write(string v)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(v);
            Write(data.Length + 1);
            Write(data);
            Write((byte)0);
        }

        public IArchitecture Architecture => ArchitectureUtils.LittleArchitecture64;

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

    public sealed class BinaryEndianWriter : IBinaryStreamWriter
    {
        private readonly IArchitecture _architecture;
        private readonly IBinaryStreamWriter _writer;
        private readonly byte[] _buffer = new byte[8];

        public BinaryEndianWriter(IArchitecture architecture, IBinaryStreamWriter writer)
        {
            _architecture = architecture;
            _writer = writer;
        }

        public void Write(byte[] data)
        {
            _writer.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            if (count == 0) return;
            Debug.Assert((index + count) <= data.Length);
            _writer.Write(data, index, count);
        }

        public void Write(ReadOnlySpan<byte> span)
        {
            _writer.Write(span);
        }

        public void Write(sbyte v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(byte v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(short v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(ushort v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(int v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(uint v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(long v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(ulong v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(float v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(double v)
        {
            var n = _architecture.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(string s)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(s);
            Write(data.Length);
            Write(data);
            Write((byte)0);
        }

        public IArchitecture Architecture => _architecture;

        public long Position
        {
            get => _writer.Position;
            set => _writer.Position = value;
        }

        public long Length
        {
            get => _writer.Length;
            set => _writer.Length = value;
        }

        public long Seek(long offset)
        {
            return _writer.Seek(offset);
        }

        public void Close()
        {
            _writer.Close();
        }
    }

    public sealed class BinaryFileWriter : IBinaryWriter
    {
        private BinaryEndianWriter _binaryWriter;
        private BinaryStreamWriter _binaryStreamWriter;
        private Stream mStream;

        public void Open(Stream s, IArchitecture architecture)
        {
            mStream = s;
            _binaryStreamWriter = new(mStream);
            _binaryWriter = new(architecture, _binaryStreamWriter);
        }

        public void Write(byte[] data)
        {
            _binaryWriter.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            _binaryWriter.Write(data, index, count);
        }

        public void Write(ReadOnlySpan<byte> span)
        {
            _binaryWriter.Write(span);
        }

        public void Write(sbyte v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(byte v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(short v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(ushort v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(int v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(uint v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(long v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(ulong v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(float v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(double v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(string s)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(s);
            Write(data.Length);
            Write(data);
            Write((byte)0);
        }

        public long Position
        {
            get => _binaryStreamWriter.Position;
            set => _binaryStreamWriter.Position = value;
        }

        public long Length
        {
            get => _binaryStreamWriter.Length;
            set => _binaryStreamWriter.Length = value;
        }

        public long Seek(long offset)
        {
            return _binaryStreamWriter.Seek(offset);
        }

        public void Close()
        {
            _binaryStreamWriter.Close();
            mStream.Close();
        }
    }

    public class BinaryMemoryWriter : IBinaryStreamWriter
    {
        private BinaryEndianWriter _binaryWriter;
        private BinaryStreamWriter _binaryStreamWriter;
        private MemoryStream _stream;

        public bool Open(MemoryStream ms, IArchitecture architecture)
        {
            _stream = ms;
            _binaryStreamWriter = new(ms);
            _binaryWriter = new(architecture, _binaryStreamWriter);
            return true;
        }

        public void Reset()
        {
            _stream.Position = 0;
        }

        public void Write(byte[] data)
        {
            _binaryWriter.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            _binaryWriter.Write(data, index, count);
        }

        public void Write(ReadOnlySpan<byte> span)
        {
            _binaryWriter.Write(span);
        }

        public void Write(sbyte v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(byte v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(short v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(ushort v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(int v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(uint v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(long v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(ulong v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(float v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(double v)
        {
            _binaryWriter.Write(v);
        }

        public void Write(string s)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(s);
            Write(data.Length);
            Write(data);
            Write((byte)0);
        }

        public IArchitecture Architecture => _binaryStreamWriter.Architecture;

        public long Position
        {
            get => _binaryStreamWriter.Position;
            set => _binaryStreamWriter.Position = value;
        }

        public long Length
        {
            get => _binaryStreamWriter.Length;
            set => _binaryStreamWriter.Length = (value);
        }

        public long Seek(long offset)
        {
            return _binaryStreamWriter.Seek(offset);
        }

        public void Close()
        {
            _binaryStreamWriter.Close();
            _stream.Close();
        }
    }

    public class BinaryMemoryBlock : IBinaryDataStream
    {
        public void Setup(byte[] memory, int offset, int length, IArchitecture architecture)
        {
            _memory = memory;
            _begin = offset;
            _end = offset + length;
            _architecture = architecture;
            Position = offset;
        }

        public void Setup(byte[] memory, int offset, int length)
        {
            Setup(memory, offset, length, ArchitectureUtils.LittleArchitecture64);
        }

        private byte[] _memory;
        private int _begin;
        private int _end;
        private int _position;
        private IArchitecture _architecture;

        public IArchitecture Architecture => _architecture;

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

        public ReadOnlyMemory<byte> GetMemory(int offset, int length)
        {
            return new ReadOnlyMemory<byte>(_memory, offset, length);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var n = Math.Min(count, _end - _position);
            Array.Copy(_memory, _position, buffer, offset, n);
            Position += n;
            return (int)n;
        }

        public sbyte ReadInt8()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadInt8(_memory, _position);
            Position += 1;
            return v;
        }

        public byte ReadUInt8()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadUInt8(_memory, _position);
            Position += 1;
            return v;
        }

        public short ReadInt16()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadInt16(_memory, _position);
            Position += 2;
            return v;
        }

        public ushort ReadUInt16()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadUInt16(_memory, _position);
            Position += 2;
            return v;
        }

        public int ReadInt32()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadInt32(_memory, _position);
            Position += 4;
            return v;
        }

        public uint ReadUInt32()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadUInt32(_memory, _position);
            Position += 4;
            return v;
        }

        public long ReadInt64()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadInt64(_memory, _position);
            Position += 8;
            return v;
        }

        public ulong ReadUInt64()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadUInt64(_memory, _position);
            Position += 8;
            return v;
        }

        public float ReadFloat()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadFloat(_memory, _position);
            Position += 4;
            return v;
        }

        public double ReadDouble()
        {
            if (Position >= _end)
                return 0;
            var v = _architecture.ReadDouble(_memory, _position);
            Position += 8;
            return v;
        }

        public string ReadString()
        {
            var length = ReadInt32();
            var data = new byte[length];
            Read(data, 0, length);
            return System.Text.Encoding.UTF8.GetString(data, 0, length);
        }

        public void Write(sbyte v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 1;
        }

        public void Write(byte v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 1;
        }

        public void Write(short v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 2;
        }

        public void Write(ushort v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 2;
        }

        public void Write(int v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 4;
        }

        public void Write(uint v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 4;
        }

        public void Write(long v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 8;
        }

        public void Write(ulong v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 8;
        }

        public void Write(float v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 4;
        }

        public void Write(double v)
        {
            if (Position >= _end)
                return;
            _architecture.Write(v, _memory, _position);
            _position += 8;
        }

        public void Write(byte[] data, int index, int count)
        {
            if (Position >= _end)
                return;
            var n = Math.Min(count, _end - _position);
            Array.Copy(data, index, _memory, _position, n);
            Position += n;
        }

        public void Write(ReadOnlySpan<byte> span)
        {
            if (Position >= _end)
                return;
            span.CopyTo(_memory.AsSpan(_position));
            Position += span.Length;
        }

        public void Write(string v)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(v);
            Write(data.Length + 1);
            Write(data, 0, data.Length);
            Write((byte)0);
        }
    }
}
