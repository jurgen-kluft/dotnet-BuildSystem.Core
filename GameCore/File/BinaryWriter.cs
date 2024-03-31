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

    public interface IBinaryStream : IBinaryWriter
    {
        Int64 Position { get; set; }
        Int64 Length { get; }

        bool Seek(StreamOffset offset);
        void Close();
    }

    #endregion

    #region BinaryWriter (Big Endian)

    public sealed class BinaryWriterBigEndian : IBinaryStream
    {
        #region Fields

        private static readonly BigEndian sEndian = new();
        private readonly BinaryWriter mWriter;

        #endregion
        #region Constructor

        public BinaryWriterBigEndian(BinaryWriter writer)
        {
            mWriter = writer;
        }

        #endregion
        #region IBinaryWriter Members

        public void Write(byte[] data)
        {
            mWriter.Write(data, 0, data.Length);
        }

        public void Write(ByteSpan span)
        {
            mWriter.Write(span.Buffer, span.Start, span.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            mWriter.Write(data, index, count);
        }

        public void Write(sbyte v)
        {
            mWriter.Write(v);
        }

        public void Write(byte v)
        {
            mWriter.Write(v);
        }

        public void Write(short v)
        {
            Write(sEndian.GetBytes(v));
        }

        public void Write(ushort v)
        {
            Write(sEndian.GetBytes(v));
        }

        public void Write(int v)
        {
            Write(sEndian.GetBytes(v));
        }

        public void Write(uint v)
        {
            Write(sEndian.GetBytes(v));
        }

        public void Write(long v)
        {
            Write(sEndian.GetBytes(v));
        }

        public void Write(ulong v)
        {
            Write(sEndian.GetBytes(v));
        }

        public void Write(float v)
        {
            Write(sEndian.GetBytes(v));
        }

        public void Write(double v)
        {
            Write(sEndian.GetBytes(v));
        }

        public void Write(string s)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            Debug.Assert(CMath.IsAligned(mWriter.BaseStream.Position, 4));
            Write(data.Length);
            Write(data);
            Write((byte)0);
        }

        public Int64 Position
        {
            get
            {
                return mWriter.BaseStream.Position;
            }
            set
            {
                mWriter.BaseStream.Position = value;
            }
        }

        public Int64 Length
        {
            get
            {
                return mWriter.BaseStream.Length;
            }
        }

        public bool Seek(StreamOffset offset)
        {
            Int64 newPos = mWriter.BaseStream.Seek(offset.Offset, SeekOrigin.Begin);
            return offset.Offset == newPos;
        }

        public void Close()
        {
            mWriter.Close();
        }

        #endregion
    }

    #endregion
    #region BinaryWriter (Little Endian)

    public sealed class BinaryWriterLittleEndian : IBinaryStream
    {
        #region Fields

        private readonly BinaryWriter mWriter;

        #endregion
        #region Constructor

        public BinaryWriterLittleEndian(BinaryWriter writer)
        {
            mWriter = writer;
        }

        #endregion
        #region IBinaryWriter Members

        public void Write(byte[] data)
        {
            mWriter.Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            mWriter.Write(data, index, count);
        }

        public void Write(sbyte v)
        {
            mWriter.Write(v);
        }

        public void Write(byte v)
        {
            mWriter.Write(v);
        }

        public void Write(short v)
        {
            mWriter.Write(v);
        }

        public void Write(ushort v)
        {
            mWriter.Write(v);
        }

        public void Write(int v)
        {
            mWriter.Write(v);
        }

        public void Write(uint v)
        {
            mWriter.Write(v);
        }

        public void Write(long v)
        {
            mWriter.Write(v);
        }

        public void Write(ulong v)
        {
            mWriter.Write(v);
        }

        public void Write(float v)
        {
            mWriter.Write(v);
        }

        public void Write(double v)
        {
            mWriter.Write(v);
        }

        public void Write(string s)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            Debug.Assert(CMath.IsAligned(mWriter.BaseStream.Position, 4));
            Write(data.Length);
            Write(data);
            Write((byte)0);
        }

        public Int64 Position
        {
            get
            {
                return mWriter.BaseStream.Position;
            }
            set
            {
                mWriter.BaseStream.Position = value;
            }
        }

        public Int64 Length
        {
            get
            {
                return mWriter.BaseStream.Length;
            }
        }

        public bool Seek(StreamOffset offset)
        {
            Int64 newPos = mWriter.BaseStream.Seek(offset.Offset, SeekOrigin.Begin);
            return offset.Offset == newPos;
        }

        public void Close()
        {
            mWriter.Close();
        }

        #endregion
    }

    public sealed class BinaryFileWriter : IBinaryStream
    {
        private BinaryWriterLittleEndian mBinaryWriter;
        private BinaryWriter mBinaryStreamWriter;
        private Stream mStream;

        public bool Open(Stream s)
        {
            mStream = s;
            mBinaryStreamWriter = new (mStream);
            mBinaryWriter = new (mBinaryStreamWriter);
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
            get
            {
                return mBinaryStreamWriter.BaseStream.Position;
            }
            set
            {
                mBinaryStreamWriter.BaseStream.Position = value;
            }
        }

        public Int64 Length
        {
            get
            {
                return mBinaryStreamWriter.BaseStream.Length;
            }
        }

        public bool Seek(StreamOffset offset)
        {
            return mBinaryWriter.Seek(offset);
        }

        public void Close()
        {
            mBinaryWriter.Close();
            mStream.Close();
        }

        #endregion
    }

    public class BinaryMemoryWriter : IBinaryStream
    {
        private BinaryWriterLittleEndian mBinaryWriter;
        private BinaryWriter mBinaryStreamWriter;
        private MemoryStream mStream;

        public bool Open(MemoryStream ms)
        {
            mStream = ms;
            mBinaryStreamWriter = new(ms);
            mBinaryWriter = new(mBinaryStreamWriter);
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

        public Int64 Position
        {
            get
            {
                return mBinaryStreamWriter.BaseStream.Position;
            }
            set
            {
                mBinaryStreamWriter.BaseStream.Position = value;
            }
        }

        public Int64 Length
        {
            get
            {
                return mBinaryStreamWriter.BaseStream.Length;
            }
        }

        public bool Seek(StreamOffset offset)
        {
            return mBinaryWriter.Seek(offset);
        }

        public void Close()
        {
            mBinaryWriter.Close();
            mStream.Close();
        }

        #endregion
    }

    #endregion
}
