using System;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    #region IBinaryWriter

    public interface IBinaryWriter
    {
        Int64 Position { get; set; }
        Int64 Length { get; }

        Int64 Write(byte[] data);
        Int64 Write(byte[] data, int index, int count);

        Int64 Write(sbyte v);
        Int64 Write(byte v);
        Int64 Write(Int16 v);
        Int64 Write(UInt16 v);
        Int64 Write(Int32 v);
        Int64 Write(UInt32 v);
        Int64 Write(Int64 v);
        Int64 Write(UInt64 v);
        Int64 Write(float v);
        Int64 Write(double v);
        Int64 Write(string v);

        bool Seek(StreamOffset offset);
        void Close();
    }

    #endregion

    #region BinaryWriter (Big Endian)

    public class BinaryWriterBigEndian : IBinaryWriter
    {
        #region Fields

        private readonly static BigEndian mEndian = new();
        private readonly BinaryWriter mWriter;

        #endregion
        #region Constructor

        public BinaryWriterBigEndian(BinaryWriter writer)
        {
            mWriter = writer;
        }

        #endregion
        #region IBinaryWriter Members

        public Int64 Write(byte[] data)
        {
            mWriter.Write(data, 0, data.Length);
            return data.Length;
        }

        public Int64 Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            mWriter.Write(data, index, count);
            return count;
        }

        public Int64 Write(sbyte v)
        {
            mWriter.Write(v);
            return 1;
        }

        public Int64 Write(byte v)
        {
            mWriter.Write(v);
            return 1;
        }

        public Int64 Write(short v)
        {
            mWriter.Write(mEndian.Convert(v));
            return 2;
        }

        public Int64 Write(ushort v)
        {
            mWriter.Write(mEndian.Convert(v));
            return 2;
        }

        public Int64 Write(int v)
        {
            mWriter.Write(mEndian.Convert(v));
            return 4;
        }

        public Int64 Write(uint v)
        {
            mWriter.Write(mEndian.Convert(v));
            return 4;
        }

        public Int64 Write(long v)
        {
            mWriter.Write(mEndian.Convert(v));
            return 8;
        }

        public Int64 Write(ulong v)
        {
            mWriter.Write(mEndian.Convert(v));
            return 8;
        }

        public Int64 Write(float v)
        {
            mWriter.Write(mEndian.Convert(v));
            return 4;
        }

        public Int64 Write(double v)
        {
            mWriter.Write(mEndian.Convert(v));
            return 8;
        }

        public Int64 Write(string s)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            Debug.Assert(Alignment.IsAligned(mWriter.BaseStream.Position, 4));
            Write(data.Length);
            Write(data);
            Write((byte)0);
            return 4 + data.Length + 1;
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
            // TODO figure out how to seek to a position larger than 32 bit
            Int64 newPos = mWriter.Seek((int)offset.value, SeekOrigin.Begin);
            return offset.value == newPos;
        }

        public void Close()
        {
            mWriter.Close();
        }

        #endregion
    }

    #endregion
    #region BinaryWriter (Little Endian)

    public class BinaryWriterLittleEndian : IBinaryWriter
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

        public Int64 Write(byte[] data)
        {
            mWriter.Write(data, 0, data.Length);
            return data.Length;
        }

        public Int64 Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            mWriter.Write(data, index, count);
            return count;
        }

        public Int64 Write(sbyte v)
        {
            mWriter.Write(v);
            return 1;
        }

        public Int64 Write(byte v)
        {
            mWriter.Write(v);
            return 1;
        }

        public Int64 Write(short v)
        {
            mWriter.Write(v);
            return 2;
        }

        public Int64 Write(ushort v)
        {
            mWriter.Write(v);
            return 2;
        }

        public Int64 Write(int v)
        {
            mWriter.Write(v);
            return 4;
        }

        public Int64 Write(uint v)
        {
            mWriter.Write(v);
            return 4;
        }

        public Int64 Write(long v)
        {
            mWriter.Write(v);
            return 8;
        }

        public Int64 Write(ulong v)
        {
            mWriter.Write(v);
            return 8;
        }

        public Int64 Write(float v)
        {
            mWriter.Write(v);
            return 4;
        }

        public Int64 Write(double v)
        {
            mWriter.Write(v);
            return 8;
        }

        public Int64 Write(string s)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            Debug.Assert(Alignment.IsAligned(mWriter.BaseStream.Position, 4));
            Write(data.Length);
            Write(data);
            Write((byte)0);
            return 4 + s.Length + 1;
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
            // TODO figure out how to seek to a position larger than 32 bit
            Int64 newPos = mWriter.Seek((int)offset.value, SeekOrigin.Begin);
            return offset.value == newPos;
        }

        public void Close()
        {
            mWriter.Close();
        }

        #endregion
    }

    public class BinaryFileWriter : IBinaryWriter
    {
        private BinaryWriterLittleEndian mBinaryWriter;
        private BinaryWriter mBinaryStreamWriter;
        private FileStream mStream;

        public bool Open(string filepath)
        {
            mStream = new (filepath, FileMode.Create);
            mBinaryStreamWriter = new (mStream);
            mBinaryWriter = new (mBinaryStreamWriter);
            return true;
        }

        #region IBinaryWriter Members

        public Int64 Write(byte[] data)
        {
            mBinaryWriter.Write(data, 0, data.Length);
            return data.Length;
        }

        public Int64 Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            mBinaryWriter.Write(data, index, count);
            return count;
        }

        public Int64 Write(sbyte v)
        {
            mBinaryWriter.Write(v);
            return 1;
        }

        public Int64 Write(byte v)
        {
            mBinaryWriter.Write(v);
            return 1;
        }

        public Int64 Write(short v)
        {
            mBinaryWriter.Write(v);
            return 2;
        }

        public Int64 Write(ushort v)
        {
            mBinaryWriter.Write(v);
            return 2;
        }

        public Int64 Write(int v)
        {
            mBinaryWriter.Write(v);
            return 4;
        }

        public Int64 Write(uint v)
        {
            mBinaryWriter.Write(v);
            return 4;
        }

        public Int64 Write(long v)
        {
            mBinaryWriter.Write(v);
            return 8;
        }

        public Int64 Write(ulong v)
        {
            mBinaryWriter.Write(v);
            return 8;
        }

        public Int64 Write(float v)
        {
            mBinaryWriter.Write(v);
            return 4;
        }

        public Int64 Write(double v)
        {
            mBinaryWriter.Write(v);
            return 8;
        }

        public Int64 Write(string s)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            Write(data.Length);
            Write(data);
            Write((byte)0);
            return 4 + data.Length + 1;
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

    public class BinaryMemoryWriter : IBinaryWriter
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

        public Int64 Write(byte[] data)
        {
            mBinaryWriter.Write(data, 0, data.Length);
            return data.Length;
        }

        public Int64 Write(byte[] data, int index, int count)
        {
            Debug.Assert((index + count) <= data.Length);
            mBinaryWriter.Write(data, index, count);
            return count;
        }

        public Int64 Write(sbyte v)
        {
            mBinaryWriter.Write(v);
            return 1;
        }

        public Int64 Write(byte v)
        {
            mBinaryWriter.Write(v);
            return 1;
        }

        public Int64 Write(short v)
        {
            mBinaryWriter.Write(v);
            return 2;
        }

        public Int64 Write(ushort v)
        {
            mBinaryWriter.Write(v);
            return 2;
        }

        public Int64 Write(int v)
        {
            mBinaryWriter.Write(v);
            return 4;
        }

        public Int64 Write(uint v)
        {
            mBinaryWriter.Write(v);
            return 4;
        }

        public Int64 Write(long v)
        {
            mBinaryWriter.Write(v);
            return 8;
        }

        public Int64 Write(ulong v)
        {
            mBinaryWriter.Write(v);
            return 8;
        }

        public Int64 Write(float v)
        {
            mBinaryWriter.Write(v);
            return 4;
        }

        public Int64 Write(double v)
        {
            mBinaryWriter.Write(v);
            return 8;
        }

        public Int64 Write(string s)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            Write(data.Length);
            Write(data);
            Write((byte)0);
            return 4 + data.Length + 1;
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
