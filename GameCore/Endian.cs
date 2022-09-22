using System;
using System.IO;

namespace GameCore
{
    #region Endian Classes

    public interface IEndian
    {
        bool little { get; }
        bool big { get; }

        Int16 Convert(Int16 v);
        UInt16 Convert(UInt16 v);
        Int32 Convert(Int32 v);
        UInt32 Convert(UInt32 v);
        Int64 Convert(Int64 v);
        UInt64 Convert(UInt64 v);
        
        byte[] Convert(float v);
        byte[] Convert(double v);
        
        void Convert(byte[] b, out float v);
        void Convert(byte[] b, out double v);
    }

    public enum EEndian
    {
        LITTLE,
        BIG,
    }

    public static class EndianUtils
    {
        public static EEndian GetPlatformEndian(EPlatform platform)
        {
            switch (platform)
            {
                case EPlatform.PC: 
                case EPlatform.MAC: 
                case EPlatform.XBOX_ONE: 
                case EPlatform.XBOX_ONE_X: 
                case EPlatform.PS4: 
                case EPlatform.PS4_PRO: 
                default: break;
            }
            return EEndian.LITTLE;
        }

        public static IBinaryWriter CreateBinaryWriter(Stream s, EEndian endian)
        {
            BinaryWriter bw = new BinaryWriter(s);
            if (endian == EEndian.LITTLE)
                return new BinaryWriterLittleEndian(bw);
            return new BinaryWriterBigEndian(bw);
        }

        public static IDataWriter CreateDataWriter(EEndian endian)
        {
            return new DataWriter(endian);
        }

        public static IBinaryReader CreateBinaryReader(Stream s, EEndian endian)
        {
            BinaryReader br = new BinaryReader(s);
            if (endian == EEndian.LITTLE)
                return (new BinaryReaderLittleEndian(br));
            return (new BinaryReaderBigEndian(br));
        }
    }

    public class LittleEndian : IEndian
    {
        public bool little { get { return true; } }
        public bool big { get { return false; } }

        public Int16 Convert(Int16 v) { return v; }
        public UInt16 Convert(UInt16 v) { return v; }
        public Int32 Convert(Int32 v) { return v; }
        public UInt32 Convert(UInt32 v) { return v; }
        public Int64 Convert(Int64 v) { return v; }
        public UInt64 Convert(UInt64 v) { return v; }
        public byte[] Convert(float v) { byte[] b = BitConverter.GetBytes(v); return b; }
        public byte[] Convert(double v) { byte[] b = BitConverter.GetBytes(v); return b; }
        public void Convert(byte[] b, out float v) { v = BitConverter.ToSingle(b, 0); }
        public void Convert(byte[] b, out double v) { v = BitConverter.ToDouble(b, 0); }
    }

    public class BigEndian : IEndian
    {
        public bool little { get { return false; } }
        public bool big { get { return true; } }

        public Int16 Convert(Int16 v) { byte[] b = BitConverter.GetBytes(v); Swap(b); return BitConverter.ToInt16(b, 0); }
        public UInt16 Convert(UInt16 v) { byte[] b = BitConverter.GetBytes(v); Swap(b); return BitConverter.ToUInt16(b, 0); }
        public Int32 Convert(Int32 v) { byte[] b = BitConverter.GetBytes(v); Swap(b); return BitConverter.ToInt32(b, 0); }
        public UInt32 Convert(UInt32 v) { byte[] b = BitConverter.GetBytes(v); Swap(b); return BitConverter.ToUInt32(b, 0); }
        public Int64 Convert(Int64 v) { byte[] b = BitConverter.GetBytes(v); Swap(b); return BitConverter.ToInt64(b, 0); }
        public UInt64 Convert(UInt64 v) { byte[] b = BitConverter.GetBytes(v); Swap(b); return BitConverter.ToUInt64(b, 0); }
        public byte[] Convert(float v) { byte[] b = BitConverter.GetBytes(v); Swap(b); return b; }
        public byte[] Convert(double v) { byte[] b = BitConverter.GetBytes(v); Swap(b); return b; }
        public void Convert(byte[] b, out float v) { Swap(b); v = BitConverter.ToSingle(b, 0); }
        public void Convert(byte[] b, out double v) { Swap(b); v = BitConverter.ToDouble(b, 0); }

        private static void Swap(byte[] b)
        {
            int n = b.Length - 1;
            int h = b.Length / 2;
            for (int i = 0; i < h; i++)
            {
                byte t = b[i];
                b[i] = b[n - i];
                b[n - i] = t;
            }
        }
    }

    #endregion
}
