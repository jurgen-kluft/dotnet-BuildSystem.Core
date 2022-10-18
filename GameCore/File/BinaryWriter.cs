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

        void Write(byte[] data);
        void Write(byte[] data, int index, int count);

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
        void Write(string v);

        bool Seek(StreamOffset offset);
        void Close();
    }

    #endregion

    #region BinaryWriter (Big Endian)

    public sealed class BinaryWriterBigEndian : IBinaryWriter
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
            mWriter.Write(mEndian.Convert(v));
        }

        public void Write(ushort v)
        {
            mWriter.Write(mEndian.Convert(v));
        }

        public void Write(int v)
        {
            mWriter.Write(mEndian.Convert(v));
        }

        public void Write(uint v)
        {
            mWriter.Write(mEndian.Convert(v));
        }

        public void Write(long v)
        {
            mWriter.Write(mEndian.Convert(v));
        }

        public void Write(ulong v)
        {
            mWriter.Write(mEndian.Convert(v));
        }

        public void Write(float v)
        {
            mWriter.Write(mEndian.Convert(v));
        }

        public void Write(double v)
        {
            mWriter.Write(mEndian.Convert(v));
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
            // TODO figure out how to seek to a position larger than 32 bit
            Int64 newPos = mWriter.Seek((int)offset.Offset, SeekOrigin.Begin);
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

    public sealed class BinaryWriterLittleEndian : IBinaryWriter
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
            // TODO figure out how to seek to a position larger than 32 bit
            Int64 newPos = mWriter.Seek((int)offset.Offset, SeekOrigin.Begin);
            return offset.Offset == newPos;
        }

        public void Close()
        {
            mWriter.Close();
        }

        #endregion
    }

    public sealed class BinaryFileWriter : IBinaryWriter
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
