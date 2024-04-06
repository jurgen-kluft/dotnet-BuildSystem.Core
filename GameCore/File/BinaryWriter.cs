using System;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    public interface IBinaryWriter
    {
        void Write(sbyte v);
        void Write(byte v);
        void Write(Int16 v);
        void Write(UInt16 v);
        void Write(Int32 v);
        void Write(UInt32 v);
        void Write(Int64 v);
        void Write(UInt64 v);
        void Write(float v);
        void Write(double v);
        void Write(byte[] data);
        void Write(byte[] data, int index, int count);
        void Write(string v);
    }

    #region IBinaryStream

    public interface IBinaryStream
    {
        void WriteStream(byte[] data, int index, int count);

        Int64 Position { get; set; }
        Int64 Length { get; set; }

        bool Seek(StreamOffset offset);
        void Close();
    }

    public interface IBinaryStreamWriter : IBinaryStream, IBinaryWriter
    {
    }

    public interface IBinaryStreamReader : IBinaryStream, IBinaryReader
    {
    }

    public class BinaryStream : IBinaryStream
    {
        private Stream mStream;

        public BinaryStream(Stream stream)
        {
            mStream = stream;
        }

        public void WriteStream(byte[] data, int index, int count)
        {
            mStream.Write(data, index, count);
        }

        public Int64 Position
        {
            get { return mStream.Position; }
            set { mStream.Position = value; }
        }

        public Int64 Length
        {
            get { return mStream.Length; }
            set { mStream.SetLength(value); }
        }

        public bool Seek(StreamOffset offset)
        {
            return mStream.Seek(offset.Offset, SeekOrigin.Begin) == offset.Offset;
        }

        public void Close()
        {
            mStream.Close();
        }
    }

    #endregion

    #region BinaryWriter (Big and Little Endian)

    public sealed class BinaryEndianWriter : IBinaryStreamWriter
    {
        #region Fields

        private readonly IEndian _endian;
        private readonly IBinaryStream _writer;

        #endregion

        #region Constructor

        public BinaryEndianWriter(IEndian endian, IBinaryStream writer)
        {
            _endian = endian;
            _writer = writer;
        }

        #endregion

        #region IBinaryWriter Members

        public void Write(byte[] data)
        {
            _writer.WriteStream(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            _writer.WriteStream(data, index, count);
        }

        public void Write(sbyte v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(byte v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        private byte[] _buffer = new byte[32];

        public void Write(short v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(ushort v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(int v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(uint v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(long v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(ulong v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(float v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(double v)
        {
            var n = _endian.GetBytes(v, _buffer);
            Write(_buffer, 0, n);
        }

        public void Write(string s)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            Write(data.Length);
            Write(data);
            Write((byte)0);
        }

        public void WriteStream(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            _writer.WriteStream(data, index, count);
        }

        public Int64 Position
        {
            get { return _writer.Position; }
            set { _writer.Position = value; }
        }

        public Int64 Length
        {
            get { return _writer.Length; }
            set { _writer.Length = value; }
        }

        public bool Seek(StreamOffset offset)
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
        private BinaryEndianWriter mBinaryWriter;
        private BinaryStream mBinaryStream;
        private Stream mStream;

        public bool Open(Stream s, IEndian endian)
        {
            mStream = s;
            mBinaryStream = new(mStream);
            mBinaryWriter = new(endian, mBinaryStream);
            return true;
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

        public Int64 Position
        {
            get { return mBinaryStream.Position; }
            set { mBinaryStream.Position = value; }
        }

        public Int64 Length
        {
            get { return mBinaryStream.Length; }
            set { mBinaryStream.Length = value; }
        }

        public bool Seek(StreamOffset offset)
        {
            return mBinaryStream.Seek(offset);
        }

        public void Close()
        {
            mBinaryStream.Close();
            mStream.Close();
        }

        #endregion
    }

    public class BinaryMemoryWriter : IBinaryStreamWriter
    {
        private BinaryEndianWriter mBinaryWriter;
        private BinaryStream mBinaryStream;
        private MemoryStream mStream;

        public bool Open(MemoryStream ms, IEndian endian)
        {
            mStream = ms;
            mBinaryStream = new(ms);
            mBinaryWriter = new(endian, mBinaryStream);
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

        public void WriteStream(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            mBinaryWriter.WriteStream(data, index, count);
        }

        public Int64 Position
        {
            get { return mBinaryStream.Position; }
            set { mBinaryStream.Position = value; }
        }

        public Int64 Length
        {
            get { return mBinaryStream.Length; }
            set { mBinaryStream.Length = (value); }
        }

        public bool Seek(StreamOffset offset)
        {
            return mBinaryStream.Seek(offset);
        }

        public void Close()
        {
            mBinaryStream.Close();
            mStream.Close();
        }

        #endregion
    }

}
