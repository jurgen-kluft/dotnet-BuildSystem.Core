using System;
using System.IO;

namespace GameCore
{
    #region IBinaryReader and IStreamReader

    public interface IBinaryReader
    {
        bool SkipBytes(Int64 size);
        int ReadBytes(byte[] buffer, int offset, int count);
        sbyte ReadInt8();
        byte ReadUInt8();
        Int16 ReadInt16();
        UInt16 ReadUInt16();
        Int32 ReadInt32();
        UInt32 ReadUInt32();
        Int64 ReadInt64();
        UInt64 ReadUInt64();
        float ReadFloat();
        double ReadDouble();
        string ReadString();
    }
    
    public interface IStreamReader
    {
        int ReadStream(byte[] data, int index, int count);

        Int64 Position { get; set; }
        Int64 Length { get; set; }

        Int64 Seek(Int64 offset);
        void Close();
    }    

    public class StreamReader : IStreamReader
    {
        private Stream mStream;

        public StreamReader(Stream stream)
        {
            mStream = stream;
        }

        public int ReadStream(byte[] data, int index, int count)
        {
            return mStream.Read(data, index, count);
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

    #region BinaryReader (Endian)
    
        
    public interface IBinaryStreamReader : IStreamReader, IBinaryReader
    {
    }


    public sealed class BinaryEndianReader : IBinaryStreamReader
    {
        #region Fields

        private readonly IEndian _endian;
        private readonly IStreamReader mReader;

        #endregion

        #region Constructor

        public BinaryEndianReader(IEndian endian, IStreamReader reader)
        {
            _endian = endian;
            mReader = reader;
        }

        #endregion

        #region IBinaryReader Members

        public Int64 Position
        {
            get { return mReader.Position; }
            set { mReader.Position = value; }
        }

        public Int64 Length
        {
            get { return mReader.Length; }
            set { mReader.Length = value; }
        }
        
        public Int64 Seek(Int64 offset)
        {
            return mReader.Seek(offset);
        }        

        public bool SkipBytes(Int64 numbytes)
        {
            Int64 curpos = mReader.Position;
            Int64 newpos = mReader.Seek(curpos + numbytes);
            return (newpos - curpos) == numbytes;
        }

        public int ReadStream(byte[] buffer, int offset, int count)
        {
            return mReader.ReadStream(buffer, offset, count);
        }
        
        public int ReadBytes(byte[] buffer, int offset, int count)
        {
            return mReader.ReadStream(buffer, offset, count);
        }

        private byte[] _buffer = new byte[8];

        public sbyte ReadInt8()
        {
            var n = mReader.ReadStream(_buffer, 0, 1);
            return _endian.GetInt8(_buffer, 0);
        }

        public byte ReadUInt8()
        {
            var n = mReader.ReadStream(_buffer, 0, 1);
            return _endian.GetUInt8(_buffer, 0);
        }

        public short ReadInt16()
        {
            var n = mReader.ReadStream(_buffer, 0, 1);
            return _endian.GetInt16(_buffer, 0);
        }

        public ushort ReadUInt16()
        {
            var n = mReader.ReadStream(_buffer, 0, 1);
            return _endian.GetUInt16(_buffer, 0);
        }

        public int ReadInt32()
        {
            var n = mReader.ReadStream(_buffer, 0, 1);
            return _endian.GetInt32(_buffer, 0);
        }

        public uint ReadUInt32()
        {
            var n = mReader.ReadStream(_buffer, 0, 1);
            return _endian.GetUInt32(_buffer, 0);
        }

        public long ReadInt64()
        {
            var n = mReader.ReadStream(_buffer, 0, 1);
            return _endian.GetInt64(_buffer, 0);
        }

        public ulong ReadUInt64()
        {
            var n = mReader.ReadStream(_buffer, 0, 1);
            return _endian.GetUInt64(_buffer, 0);
        }

        public float ReadFloat()
        {
            var n = mReader.ReadStream(_buffer, 0, 4);
            return _endian.GetFloat(_buffer, 0);
        }

        public double ReadDouble()
        {
            var n = mReader.ReadStream(_buffer, 0, 8);
            return _endian.GetDouble(_buffer, 0);
        }

        public string ReadString()
        {
            Int32 len = ReadInt32();
            byte[] data = new byte[len + 1];
            ReadBytes(data, 0, len + 1);
            string s = System.Text.Encoding.UTF8.GetString(data, 0, len);
            return s;
        }

        public void Close()
        {
            mReader.Close();
        }

        #endregion
    }

    #endregion


    #region BinaryFileReader

    public sealed class BinaryFileReader : IBinaryStreamReader
    {
        private BinaryEndianReader _binaryReader;
        private StreamReader _streamReader;
        private Stream _stream;

        public void Open(Stream s, IEndian endian)
        {
            _stream = s;
            _streamReader = new (_stream);
            _binaryReader = new (endian, _streamReader);
        }
        
        public bool Open(string filename)
        {
            if (!File.Exists(filename))
                return false;
            var endian = new LittleEndian();
            _stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _streamReader = new StreamReader(_stream);
            _binaryReader = new BinaryEndianReader(endian, _streamReader);
            return false;
        }

        public void Close()
        {
            if (_stream != null)
            {
                _binaryReader.Close();
                _stream.Close();
            }
        }

        public Int64 Position
        {
            get { return _streamReader.Position; }
            set { _streamReader.Position = value; }
        }

        public Int64 Length
        {
            get { return _streamReader.Length; }
            set { _streamReader.Length = value; }
        }
        
        public Int64 Seek(Int64 offset)
        {
            return _streamReader.Seek(offset);
        }

        public bool SkipBytes(Int64 numbytes)
        {
            return _binaryReader.SkipBytes(numbytes);
        }

        public int ReadStream(byte[] data, int offset, int size)
        {
            return _binaryReader.ReadBytes(data, offset, size);
        }
        
        public int ReadBytes(byte[] data, int offset, int size)
        {
            return _binaryReader.ReadBytes(data, offset, size);
        }

        public sbyte ReadInt8()
        {
            return _binaryReader.ReadInt8();
        }

        public byte ReadUInt8()
        {
            return _binaryReader.ReadUInt8();
        }

        public Int16 ReadInt16()
        {
            return _binaryReader.ReadInt16();
        }

        public UInt16 ReadUInt16()
        {
            return _binaryReader.ReadUInt16();
        }

        public Int32 ReadInt32()
        {
            return _binaryReader.ReadInt32();
        }

        public UInt32 ReadUInt32()
        {
            return _binaryReader.ReadUInt32();
        }

        public Int64 ReadInt64()
        {
            return _binaryReader.ReadInt64();
        }

        public UInt64 ReadUInt64()
        {
            return _binaryReader.ReadUInt64();
        }

        public float ReadFloat()
        {
            return _binaryReader.ReadFloat();
        }

        public double ReadDouble()
        {
            return _binaryReader.ReadDouble();
        }

        public string ReadString()
        {
            return _binaryReader.ReadString();
        }
    }

    #endregion
}
