using System;
using System.Diagnostics;
using System.IO;

namespace Core
{
    #region IBinaryWriter

    public interface IBinaryWriter
    {
        Int64 Position { get; }
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

        private static IEndian mEndian = new xBigEndian();
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
            byte[] data = System.Text.Encoding.ASCII.GetBytes(s);
            Write(data);
            Write((byte)0);
            return s.Length + 1;
        }

        public Int64 Write(string s, bool ascii)
        {
            if (ascii)
            {
                return Write(s);
            }
            else
            {
                // Unicode
                throw new NotImplementedException();
            }
        }

        public Int64 Position
        {
            get
            {
                return mWriter.BaseStream.Position;
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
            Int64 newPos = mWriter.Seek(offset.value32, SeekOrigin.Begin);
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
            byte[] data = System.Text.Encoding.ASCII.GetBytes(s);
            Write(data);
            Write((byte)0);
            return s.Length + 1;
        }

        public Int64 Write(string s, bool ascii)
        {
            if (ascii)
            {
                return Write(s);
            }
            else
            {
                // Unicode
                throw new NotImplementedException();
            }
        }
        
        public Int64 Position
        {
            get
            {
                return mWriter.BaseStream.Position;
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
            Int64 newPos = mWriter.Seek(offset.value32, SeekOrigin.Begin);
            return offset.value == newPos;
        }

        public void Close()
        {
            mWriter.Close();
        }

        #endregion
    }

    #endregion
}
