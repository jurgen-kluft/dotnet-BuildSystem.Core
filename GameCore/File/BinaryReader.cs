using System;
using System.IO;

namespace GameCore
{
    #region IBinaryReader 

    public interface IBinaryReader
    {
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

        private static IEndian mEndian = new BigEndian();
        private readonly BinaryReader mReader;

        #endregion
        #region Constructor

        public BinaryReaderBigEndian(BinaryReader reader)
        {
            mReader = reader;
        }

        #endregion
        #region IBinaryReader Members

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
            float v;
            mEndian.Convert(mReader.ReadBytes(4), out v);
            return v;
        }

        public double ReadDouble()
        {
            double v;
            mEndian.Convert(mReader.ReadBytes(8), out v);
            return v;
        }

        public string ReadString()
        {
            string s = string.Empty;
            while (true)
            {
                byte b = mReader.ReadByte();
                if (b == 0)
                    break;
                s = s + (char)b;
            }
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
            string s = string.Empty;
            while (true)
            {
                byte b = mReader.ReadByte();
                if (b == 0)
                    break;
                s = s + (char)b;
            }
            return s;
        }

        public void Close()
        {
            mReader.Close();
        }

        #endregion
    }

    #endregion
}
