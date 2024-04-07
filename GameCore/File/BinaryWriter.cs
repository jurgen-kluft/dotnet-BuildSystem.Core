using System;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    #region IBinaryWriter and IBinaryStream
    
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


    public interface IStreamWriter
    {
        void WriteStream(byte[] data, int index, int count);

        Int64 Position { get; set; }
        Int64 Length { get; set; }

        Int64 Seek(Int64 offset);
        void Close();
    }

    public class StreamWriter : IStreamWriter
    {
        private Stream mStream;

        public StreamWriter(Stream stream)
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

        public Int64 Seek(Int64 offset)
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
    
        
    public interface IBinaryStreamWriter : IStreamWriter, IBinaryWriter
    {
    }


    public sealed class BinaryEndianWriter : IBinaryStreamWriter
    {
        #region Fields

        private readonly IEndian _endian;
        private readonly IStreamWriter _writer;

        #endregion

        #region Constructor

        public BinaryEndianWriter(IEndian endian, IStreamWriter writer)
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
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(byte v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        private byte[] _buffer = new byte[32];

        public void Write(short v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(ushort v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(int v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(uint v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(long v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(ulong v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(float v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
            Write(_buffer, 0, n);
        }

        public void Write(double v)
        {
            var n = _endian.GetBytes(v, _buffer, 0);
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

        public Int64 Seek(Int64 offset)
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
        private StreamWriter _streamWriter;
        private Stream _stream;

        public void Open(Stream s, IEndian endian)
        {
            _stream = s;
            _streamWriter = new(_stream);
            _binaryWriter = new(endian, _streamWriter);
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

        public Int64 Position
        {
            get { return _streamWriter.Position; }
            set { _streamWriter.Position = value; }
        }

        public Int64 Length
        {
            get { return _streamWriter.Length; }
            set { _streamWriter.Length = value; }
        }

        public Int64 Seek(Int64 offset)
        {
            return _streamWriter.Seek(offset);
        }

        public void Close()
        {
            _streamWriter.Close();
            _stream.Close();
        }

        #endregion
    }

    public class BinaryMemoryWriter : IBinaryStreamWriter
    {
        private BinaryEndianWriter mBinaryWriter;
        private StreamWriter _mStreamWriter;
        private MemoryStream mStream;

        public bool Open(MemoryStream ms, IEndian endian)
        {
            mStream = ms;
            _mStreamWriter = new(ms);
            mBinaryWriter = new(endian, _mStreamWriter);
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
            get { return _mStreamWriter.Position; }
            set { _mStreamWriter.Position = value; }
        }

        public Int64 Length
        {
            get { return _mStreamWriter.Length; }
            set { _mStreamWriter.Length = (value); }
        }

        public Int64 Seek(Int64 offset)
        {
            return _mStreamWriter.Seek(offset);
        }

        public void Close()
        {
            _mStreamWriter.Close();
            mStream.Close();
        }

        #endregion
    }

}
