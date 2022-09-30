using System;
using System.IO;

namespace GameCore
{
    #region IBinaryReader 

    public interface IBinaryReader
    {
        Int64 Position { get; }
        Int64 Length { get; }

        bool SkipBytes(Int64 size);
        byte[] ReadBytes(int size);
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

        void Close();
    }

    #endregion

    #region BinaryReader (Big Endian)

    public class BinaryReaderBigEndian : IBinaryReader
    {
        #region Fields

        private static BigEndian mEndian = new ();
        private readonly BinaryReader mReader;

        #endregion
        #region Constructor

        public BinaryReaderBigEndian(BinaryReader reader)
        {
            mReader = reader;
        }

        #endregion
        #region IBinaryReader Members

        public Int64 Position { get { return mReader.BaseStream.Position; } }
        public Int64 Length { get { return mReader.BaseStream.Length; } }

        public bool SkipBytes(Int64 numbytes)
        {
            Int64 curpos = mReader.BaseStream.Position;
            Int64 newpos = mReader.BaseStream.Seek(numbytes, SeekOrigin.Current);
            return (newpos - curpos) == numbytes;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] data = new byte[count];
            mReader.Read(data, 0, count);
            return data;
        }

        public sbyte ReadInt8()
        {
            return mReader.ReadSByte();
        }

        public byte ReadUInt8()
        {
            return mReader.ReadByte();
        }

        public short ReadInt16()
        {
            return mEndian.Convert(mReader.ReadInt16());
        }

        public ushort ReadUInt16()
        {
            return mEndian.Convert(mReader.ReadUInt16());
        }

        public int ReadInt32()
        {
            return mEndian.Convert(mReader.ReadInt32());
        }

        public uint ReadUInt32()
        {
            return mEndian.Convert(mReader.ReadUInt32());
        }

        public long ReadInt64()
        {
            return mEndian.Convert(mReader.ReadInt64());
        }

        public ulong ReadUInt64()
        {
            return mEndian.Convert(mReader.ReadUInt64());
        }

        public float ReadFloat()
        {
            mEndian.Convert(mReader.ReadBytes(4), out float v);
            return v;
        }

        public double ReadDouble()
        {
            mEndian.Convert(mReader.ReadBytes(8), out double v);
            return v;
        }

        public string ReadString()
        {
            Int32 len = ReadInt32();
            byte[] data = ReadBytes(len);
            string s = System.Text.Encoding.UTF8.GetString(data);
            return s;
        }

        public void Close()
        {
            mReader.Close();
        }

        #endregion
    }

    #endregion
    #region BinaryReader (Little Endian)

    public class BinaryReaderLittleEndian : IBinaryReader
    {
        #region Fields

        private readonly BinaryReader mReader;

        #endregion
        #region Constructor

        public BinaryReaderLittleEndian(BinaryReader reader)
        {
            mReader = reader;
        }

        #endregion
        #region IBinaryReader Members

        public Int64 Position { get { return mReader.BaseStream.Position; } }
        public Int64 Length { get { return mReader.BaseStream.Length; } }

        public bool SkipBytes(Int64 numbytes)
		{
            Int64 curpos = mReader.BaseStream.Position;
            Int64 newpos = mReader.BaseStream.Seek(numbytes, SeekOrigin.Current);
            return (newpos - curpos)  == numbytes;
		}

        public byte[] ReadBytes(int count)
        {
            byte[] data = new byte[count];
            mReader.Read(data, 0, count);
            return data;
        }

        public sbyte ReadInt8()
        {
            return mReader.ReadSByte();
        }

        public byte ReadUInt8()
        {
            return mReader.ReadByte();
        }

        public short ReadInt16()
        {
            return mReader.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            return mReader.ReadUInt16();
        }

        public int ReadInt32()
        {
            return mReader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return mReader.ReadUInt32();
        }

        public long ReadInt64()
        {
            return mReader.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            return mReader.ReadUInt64();
        }

        public float ReadFloat()
        {
            return mReader.ReadSingle();
        }

        public double ReadDouble()
        {
            return mReader.ReadDouble();
        }

        public string ReadString()
        {
            Int32 len = ReadInt32();
            byte[] data = ReadBytes(len);
            string s = System.Text.Encoding.UTF8.GetString(data);
            return s;
        }

        public void Close()
        {
            mReader.Close();
        }

        #endregion
    }

    public class BinaryFileReader : IBinaryReader
    {
        private IBinaryReader mBinaryReader;
        private BinaryReader mBinaryFileReader;
        private FileStream mFileStream;

        public bool Open(string filepath)
        {
            if (File.Exists(filepath))
            {
                mFileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                mBinaryFileReader = new BinaryReader(mFileStream);
                mBinaryReader = new BinaryReaderLittleEndian(mBinaryFileReader);
                return true;
            }
            return false;
        }

        public void Close()
        {
            if (mFileStream != null)
            {
                mBinaryReader.Close();
                mFileStream.Close();
            }
        }

        public Int64 Position { get { return mBinaryReader.Position; } }
        public Int64 Length { get { return mBinaryReader.Length; } }

        public bool SkipBytes(Int64 numbytes)
        {
            return mBinaryReader.SkipBytes(numbytes);
        }

        public byte[] ReadBytes(int size)
        {
            return mBinaryReader.ReadBytes(size);
        }
        public sbyte ReadInt8()
        {
            return mBinaryReader.ReadInt8();
        }
        public byte ReadUInt8()
        {
            return mBinaryReader.ReadUInt8();
        }
        public Int16 ReadInt16()
        {
            return mBinaryReader.ReadInt16();
        }
        public UInt16 ReadUInt16()
        {
            return mBinaryReader.ReadUInt16();
        }
        public Int32 ReadInt32()
        {
            return mBinaryReader.ReadInt32();
        }
        public UInt32 ReadUInt32()
        {
            return mBinaryReader.ReadUInt32();
        }
        public Int64 ReadInt64()
        {
            return mBinaryReader.ReadInt64();
        }
        public UInt64 ReadUInt64()
        {
            return mBinaryReader.ReadUInt64();
        }
        public float ReadFloat()
        {
            return mBinaryReader.ReadFloat();
        }
        public double ReadDouble()
        {
            return mBinaryReader.ReadDouble();
        }
        public string ReadString()
        {
            return mBinaryReader.ReadString();
        }

    }

    #endregion
}
