using System;
using System.IO;

namespace GameCore
{
    #region Endian Classes

    public interface IEndian
    {
        bool little { get; }
        bool big { get; }

        int GetBytes(byte v, byte[] buffer, int index);
        int GetBytes(sbyte v, byte[] buffer, int index);
        int GetBytes(Int16 v, byte[] buffer, int index);
        int GetBytes(UInt16 v, byte[] buffer, int index);
        int GetBytes(Int32 v, byte[] buffer, int index);
        int GetBytes(UInt32 v, byte[] buffer, int index);
        int GetBytes(Int64 v, byte[] buffer, int index);
        int GetBytes(UInt64 v, byte[] buffer, int index);
        int GetBytes(float v, byte[] buffer, int index);
        int GetBytes(double v, byte[] buffer, int index);

        sbyte GetInt8(byte[] buffer, int index);
        byte GetUInt8(byte[]buffer, int index);
        Int16 GetInt16(byte[] buffer, int index);
        UInt16 GetUInt16(byte[]buffer, int index);
        Int32 GetInt32(byte[]buffer, int index);
        UInt32 GetUInt32(byte[]buffer, int index);
        Int64 GetInt64(byte[]buffer, int index);
        UInt64 GetUInt64(byte[]buffer, int index);
        float GetFloat(byte[]buffer, int index);
        double GetDouble(byte[]buffer, int index);
        

    }

    public enum EEndian
    {
        Little,
        Big,
    }

    public static class EndianUtils
    {
        public static EEndian GetPlatformEndian(EPlatform platform)
        {
            return platform switch
            {
                EPlatform.Win32 => EEndian.Little,
                EPlatform.Win64 => EEndian.Little,
                EPlatform.Mac => EEndian.Little,
                EPlatform.XboxOne => EEndian.Little,
                EPlatform.XboxOneX => EEndian.Little,
                EPlatform.PS4 => EEndian.Little,
                EPlatform.PS4Pro => EEndian.Little,
                EPlatform.XboxSeriesS => EEndian.Little,
                EPlatform.XboxSeriesX => EEndian.Little,
                EPlatform.PS5 => EEndian.Little,
                EPlatform.NintendoSwitch => EEndian.Little,
                _ => EEndian.Little
            };
        }

        private static IEndian _littleEndian = new LittleEndian();
        private static IEndian _bigEndian = new BigEndian();

        private static IEndian GetEndian(EEndian endian)
        {
            return endian switch
            {
                EEndian.Little => _littleEndian,
                EEndian.Big => _bigEndian,
                _ => _littleEndian
            };
        }

        public static IEndian GetEndianForPlatform(EPlatform platform)
        {
            return GetEndian(GetPlatformEndian(platform));
        }

        public static bool IsPlatform64Bit(EPlatform platform)
        {
            return (platform & EPlatform.Arch64) != 0;
        }

        public static IDataWriter CreateDataWriter(EPlatform platform)
        {
            return new DataWriter(platform);
        }

        public static IBinaryStreamReader CreateBinaryReader(Stream s, EPlatform platform)
        {
            var bs = new StreamReader(s);
            return (new BinaryEndianReader(EndianUtils.GetEndian(EndianUtils.GetPlatformEndian(platform)), bs));
        }

        public static IBinaryStreamWriter CreateBinaryWriter(Stream s, EPlatform platform)
        {
            var bs = new StreamWriter(s);
            return (new BinaryEndianWriter(EndianUtils.GetEndian(EndianUtils.GetPlatformEndian(platform)), bs));
        }

        public static IBinaryStreamWriter CreateBinaryWriter(string filepath, EPlatform platform)
        {
            Stream s = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter bs = new(s);
            return (new BinaryEndianWriter(EndianUtils.GetEndian(EndianUtils.GetPlatformEndian(platform)), bs));
        }
    }

    public class LittleEndian : IEndian
    {
        public bool little => true;
        public bool big => false;

        public int GetBytes(sbyte v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 1;
        }

        public int GetBytes(byte v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 1;
        }

        public int GetBytes(Int16 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 2;
        }

        public int GetBytes(UInt16 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 2;
        }

        public int GetBytes(Int32 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 4;
        }

        public int GetBytes(UInt32 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 4;
        }

        public int GetBytes(Int64 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 8;
        }

        public int GetBytes(UInt64 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 8;
        }

        public int GetBytes(float v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 4;
        }

        public int GetBytes(double v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 8;
        }


        private byte[] _buffer = new byte[8];

        public sbyte GetInt8(byte[] buffer, int index)
        {
            return (sbyte)buffer[index];
        }

        public byte GetUInt8(byte[] buffer, int index)
        {
            return buffer[index];
        }
        
        public Int16 GetInt16(byte[] buffer, int index)
        {
            return BitConverter.ToInt16(_buffer, index);
        }

        public UInt16 GetUInt16(byte[] buffer, int index)
        {
            return BitConverter.ToUInt16(_buffer, index);
        }

        public Int32 GetInt32(byte[] buffer, int index)
        {
            return BitConverter.ToInt32(_buffer, index);
        }

        public UInt32 GetUInt32(byte[] buffer, int index)
        {
            return BitConverter.ToUInt32(_buffer);
        }

        public Int64 GetInt64(byte[] buffer, int index)
        {
            return BitConverter.ToInt64(_buffer);
        }

        public UInt64 GetUInt64(byte[] buffer, int index)
        {
            return BitConverter.ToUInt64(_buffer);
        }

        public float GetFloat(byte[] bytes, int index)
        {
            return BitConverter.ToSingle(bytes, index);
        }

        public double GetDouble(byte[] bytes, int index)
        {
            return BitConverter.ToDouble(bytes, index);
        }
        
    }

    public class BigEndian : IEndian
    {
        public bool little => false;
        public bool big => true;

        public int GetBytes(sbyte v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 1;
        }

        public int GetBytes(byte v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            return 1;
        }

        public int GetBytes(Int16 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
            return 2;
        }

        public int GetBytes(UInt16 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
            return 2;
        }

        public int GetBytes(Int32 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return 4;
        }

        public int GetBytes(UInt32 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return 4;
        }

        public int GetBytes(Int64 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return 8;
        }

        public int GetBytes(UInt64 v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return 8;
        }

        public int GetBytes(float v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return 4;
        }

        public int GetBytes(double v, byte[] buffer, int index)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(index), v);
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return 8;
        }


        private byte[] _buffer = new byte[8];

        public sbyte GetInt8(byte[] buffer, int index)
        {
            return (sbyte)buffer[index];
        }

        public byte GetUInt8(byte[] buffer, int index)
        {
            return buffer[index];
        }
        
        public Int16 GetInt16(byte[] buffer, int index)
        {
            (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
            return BitConverter.ToInt16(_buffer, index);
        }

        public UInt16 GetUInt16(byte[] buffer, int index)
        {
            (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
            return BitConverter.ToUInt16(_buffer, index);
        }

        public Int32 GetInt32(byte[] buffer, int index)
        {
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 3], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return BitConverter.ToInt32(_buffer, index);
        }

        public UInt32 GetUInt32(byte[] buffer, int index)
        {
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return BitConverter.ToUInt32(_buffer);
        }

        public Int64 GetInt64(byte[] buffer, int index)
        {
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return BitConverter.ToInt64(_buffer);
        }

        public UInt64 GetUInt64(byte[] buffer, int index)
        {
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return BitConverter.ToUInt64(_buffer);
        }

        public float GetFloat(byte[] buffer, int index)
        {
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return BitConverter.ToSingle(buffer, index);
        }

        public double GetDouble(byte[] buffer, int index)
        {
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return BitConverter.ToDouble(buffer, index);
        }
    }

    #endregion
}
