using System.Diagnostics;

namespace GameCore
{
    #region IBinaryWriter and IBinaryStream

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
        void Write(string v);
    }

    public interface IBinaryStream
    {
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
        private Stream mStream;
        private byte[] mBuffer = new byte[32];

        public BinaryStreamWriter(Stream stream)
        {
            mStream = stream;
        }

        public void Write(sbyte v)
        {
            mStream.WriteByte((byte)v);
        }

        public void Write(byte v)
        {
            mStream.WriteByte(v);
        }

        public void Write(short v)
        {
            mBuffer[0] = (byte)v;
            mBuffer[1] = (byte)(v >> 8);
            mStream.Write(mBuffer, 0, 2);
        }

        public void Write(ushort v)
        {
            mBuffer[0] = (byte)v;
            mBuffer[1] = (byte)(v >> 8);
            mStream.Write(mBuffer, 0, 2);
        }

        public void Write(int v)
        {
            mBuffer[0] = (byte)v;
            mBuffer[1] = (byte)(v >> 8);
            mBuffer[2] = (byte)(v >> 16);
            mBuffer[3] = (byte)(v >> 24);
            mStream.Write(mBuffer, 0, 4);
        }

        public void Write(uint v)
        {
            mBuffer[0] = (byte)v;
            mBuffer[1] = (byte)(v >> 8);
            mBuffer[2] = (byte)(v >> 16);
            mBuffer[3] = (byte)(v >> 24);
            mStream.Write(mBuffer, 0, 4);
        }

        public void Write(long v)
        {
            mBuffer[0] = (byte)v;
            mBuffer[1] = (byte)(v >> 8);
            mBuffer[2] = (byte)(v >> 16);
            mBuffer[3] = (byte)(v >> 24);
            mBuffer[4] = (byte)(v >> 32);
            mBuffer[5] = (byte)(v >> 40);
            mBuffer[6] = (byte)(v >> 48);
            mBuffer[7] = (byte)(v >> 56);
            mStream.Write(mBuffer, 0, 8);
        }

        public void Write(ulong v)
        {
            mBuffer[0] = (byte)v;
            mBuffer[1] = (byte)(v >> 8);
            mBuffer[2] = (byte)(v >> 16);
            mBuffer[3] = (byte)(v >> 24);
            mBuffer[4] = (byte)(v >> 32);
            mBuffer[5] = (byte)(v >> 40);
            mBuffer[6] = (byte)(v >> 48);
            mBuffer[7] = (byte)(v >> 56);
            mStream.Write(mBuffer, 0, 8);
        }

        public void Write(float v)
        {
            if (BitConverter.TryWriteBytes(mBuffer, v))
                mStream.Write(mBuffer, 0, 4);
        }

        public void Write(double v)
        {
            if (BitConverter.TryWriteBytes(mBuffer, v))
                mStream.Write(mBuffer, 0, 8);
        }

        public void Write(byte[] data)
        {
            mStream.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            mStream.Write(data, index, count);
        }

        public void Write(string v)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(v);
            Write(data.Length+1);
            Write(data);
            Write((byte)0);
        }

        public long Position
        {
            get => mStream.Position;
            set => mStream.Position = value;
        }

        public long Length
        {
            get => mStream.Length;
            set => mStream.SetLength(value);
        }

        public long Seek(long offset)
        {
            return mStream.Seek(offset, SeekOrigin.Begin);
        }

        public void Close()
        {
            mStream.Close();
        }
    }

    public class BinaryStreamReader : IBinaryStreamReader
    {
        private Stream mStream;
        private byte[] mBuffer = new byte[32];

        public BinaryStreamReader(Stream stream)
        {
            mStream = stream;
        }

        public sbyte ReadInt8()
        {
            return (sbyte)mStream.ReadByte();
        }

        public byte ReadUInt8()
        {
            return (byte)mStream.ReadByte();
        }

        public short ReadInt16()
        {
            mStream.Read(mBuffer, 0, 2);
            return (short)(mBuffer[0] | (mBuffer[1] << 8));
        }

        public ushort ReadUInt16()
        {
            mStream.Read(mBuffer, 0, 2);
            return (ushort)(mBuffer[0] | (mBuffer[1] << 8));
        }

        public int ReadInt32()
        {
            mStream.Read(mBuffer, 0, 4);
            return mBuffer[0] | (mBuffer[1] << 8) | (mBuffer[2] << 16) | (mBuffer[3] << 24);
        }

        public uint ReadUInt32()
        {
            mStream.Read(mBuffer, 0, 4);
            return (uint)(mBuffer[0] | (mBuffer[1] << 8) | (mBuffer[2] << 16) | (mBuffer[3] << 24));
        }

        public long ReadInt64()
        {
            mStream.Read(mBuffer, 0, 8);
            return (long)mBuffer[0] | ((long)mBuffer[1] << 8) | ((long)mBuffer[2] << 16) | ((long)mBuffer[3] << 24) | ((long)mBuffer[4] << 32) | ((long)mBuffer[5] << 40) | ((long)mBuffer[6] << 48) | ((long)mBuffer[7] << 56);
        }

        public ulong ReadUInt64()
        {
            mStream.Read(mBuffer, 0, 8);
            return ((ulong)mBuffer[0] | ((ulong)mBuffer[1] << 8) | ((ulong)mBuffer[2] << 16) | ((ulong)mBuffer[3] << 24) | ((ulong)mBuffer[4] << 32) | ((ulong)mBuffer[5] << 40) | ((ulong)mBuffer[6] << 48) | ((ulong)mBuffer[7] << 56));
        }

        public float ReadFloat()
        {
            mStream.Read(mBuffer, 0, 4);
            return BitConverter.ToSingle(mBuffer, 0);
        }

        public double ReadDouble()
        {
            mStream.Read(mBuffer, 0, 8);
            return BitConverter.ToDouble(mBuffer, 0);
        }

        public string ReadString()
        {
            var length = ReadInt32();
            var data = new byte[length];
            Read(data, 0, length);
            return System.Text.Encoding.UTF8.GetString(data, 0, length);
        }

        public int Read(byte[] data, int index, int count)
        {
            return mStream.Read(data, index, count);
        }

        public bool SkipBytes(long numBytes)
        {
            var pos = Position + numBytes;
            Position = pos;
            return (Position - pos) == numBytes;
        }

        public long Position
        {
            get => mStream.Position;
            set => mStream.Position = value;
        }

        public long Length
        {
            get => mStream.Length;
            set => mStream.SetLength(value);
        }

        public long Seek(long offset)
        {
            return mStream.Seek(offset, SeekOrigin.Begin);
        }

        public void Close()
        {
            mStream.Close();
        }
    }

    #endregion

    #region BinaryWriter (Endian)


    public sealed class BinaryEndianWriter : IBinaryStreamWriter
    {
        #region Fields

        private readonly IEndian _endian;
        private readonly IBinaryStreamWriter _writer;
        private readonly byte[] _buffer = new byte[8];

        #endregion

        #region Constructor

        public BinaryEndianWriter(IEndian endian, IBinaryStreamWriter writer)
        {
            _endian = endian;
            _writer = writer;
        }

        #endregion

        #region IBinaryWriter Members

        public void Write(byte[] data)
        {
            _writer.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            _writer.Write(data, index, count);
        }

        public void Write(sbyte v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(byte v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(short v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(ushort v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(int v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(uint v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(long v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(ulong v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(float v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(double v)
        {
            var n = _endian.Write(v, _buffer, 0);
            Write(_buffer, 0, n);
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

        #endregion
    }

    #endregion

    public sealed class BinaryFileWriter : IBinaryWriter
    {
        private BinaryEndianWriter _binaryWriter;
        private BinaryStreamWriter _binaryStreamWriter;
        private Stream mStream;

        public void Open(Stream s, IEndian endian)
        {
            mStream = s;
            _binaryStreamWriter = new(mStream);
            _binaryWriter = new(endian, _binaryStreamWriter);
        }

        #region IBinaryWriter Members

        public void Write(byte[] data)
        {
            _binaryWriter.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            _binaryWriter.Write(data, index, count);
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
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
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

        #endregion
    }

    public class BinaryMemoryWriter : IBinaryStreamWriter
    {
        private BinaryEndianWriter mBinaryWriter;
        private BinaryStreamWriter _mBinaryStreamWriter;
        private MemoryStream mStream;

        public bool Open(MemoryStream ms, IEndian endian)
        {
            mStream = ms;
            _mBinaryStreamWriter = new(ms);
            mBinaryWriter = new(endian, _mBinaryStreamWriter);
            return true;
        }

        public void Reset()
        {
            mStream.Position = 0;
        }

        #region IBinaryWriter Members

        public void Write(byte[] data)
        {
            mBinaryWriter.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            mBinaryWriter.Write(data, index, count);
        }

        public void Write(sbyte v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(byte v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(short v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(ushort v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(int v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(uint v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(long v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(ulong v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(float v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(double v)
        {
            mBinaryWriter.Write(v);
        }

        public void Write(string s)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            Write(data.Length);
            Write(data);
            Write((byte)0);
        }

        public long Position
        {
            get => _mBinaryStreamWriter.Position;
            set => _mBinaryStreamWriter.Position = value;
        }

        public long Length
        {
            get => _mBinaryStreamWriter.Length;
            set => _mBinaryStreamWriter.Length = (value);
        }

        public long Seek(long offset)
        {
            return _mBinaryStreamWriter.Seek(offset);
        }

        public void Close()
        {
            _mBinaryStreamWriter.Close();
            mStream.Close();
        }

        #endregion
    }

    public class BinaryMemoryBlock : IBinaryDataStream
    {
        public void Setup(byte[] memory, int offset, int length, IEndian endian)
        {
            _memory = memory;
            _begin = offset;
            _end = offset + length;
            _endian = endian;
            Position = offset;
        }

        public void Setup(byte[] memory, int offset, int length)
        {
            Setup(memory, offset, length, EndianUtils.sLittleEndian);
        }

        private byte[] _memory;
        private int _begin;
        private int _end;
        private int _position;
        private IEndian _endian;

        public long Position { get => _position; set => _position = (int)value; }

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
            var v = _endian.ReadInt8(_memory, _position);
            Position += 1;
            return v;
        }

        public byte ReadUInt8()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadUInt8(_memory, _position);
            Position += 1;
            return v;
        }

        public short ReadInt16()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadInt16(_memory, _position);
            Position += 2;
            return v;
        }

        public ushort ReadUInt16()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadUInt16(_memory, _position);
            Position += 2;
            return v;
        }

        public int ReadInt32()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadInt32(_memory, _position);
            Position += 4;
            return v;
        }

        public uint ReadUInt32()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadUInt32(_memory, _position);
            Position += 4;
            return v;
        }

        public long ReadInt64()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadInt64(_memory, _position);
            Position += 8;
            return v;
        }

        public ulong ReadUInt64()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadUInt64(_memory, _position);
            Position += 8;
            return v;
        }

        public float ReadFloat()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadFloat(_memory, _position);
            Position += 4;
            return v;
        }

        public double ReadDouble()
        {
            if (Position >= _end)
                return 0;
            var v = _endian.ReadDouble(_memory, _position);
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
            _endian.Write(v, _memory, _position);
            _position += 1;
        }

        public void Write(byte v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
            _position += 1;
        }

        public void Write(short v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
            _position += 2;
        }

        public void Write(ushort v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
            _position += 2;
        }

        public void Write(int v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
            _position += 4;
        }

        public void Write(uint v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
            _position += 4;
        }

        public void Write(long v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
            _position += 8;
        }

        public void Write(ulong v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
            _position += 8;
        }

        public void Write(float v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
            _position += 4;
        }

        public void Write(double v)
        {
            if (Position >= _end)
                return;
            _endian.Write(v, _memory, _position);
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

        public void Write(string v)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(v);
            Write(data.Length + 1);
            Write(data, 0, data.Length);
            Write((byte)0);
        }
    }
}
