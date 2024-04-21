using System;
using System.IO;

namespace GameCore
{
    #region Endian Classes

    public interface IEndian
    {
        bool little { get; }
        bool big { get; }

        int Write(byte v, byte[] buffer, int offset);
        int Write(sbyte v, byte[] buffer, int offset);
        int Write(Int16 v, byte[] buffer, int offset);
        int Write(UInt16 v, byte[] buffer, int offset);
        int Write(Int32 v, byte[] buffer, int offset);
        int Write(UInt32 v, byte[] buffer, int offset);
        int Write(Int64 v, byte[] buffer, int offset);
        int Write(UInt64 v, byte[] buffer, int offset);
        int Write(float v, byte[] buffer, int offset);
        int Write(double v, byte[] buffer, int offset);

        sbyte ReadInt8(byte[] buffer, int index);
        byte ReadUInt8(byte[] buffer, int index);
        Int16 ReadInt16(byte[] buffer, int index);
        UInt16 ReadUInt16(byte[] buffer, int index);
        Int32 ReadInt32(byte[] buffer, int index);
        UInt32 ReadUInt32(byte[] buffer, int index);
        Int64 ReadInt64(byte[] buffer, int index);
        UInt64 ReadUInt64(byte[] buffer, int index);
        float ReadFloat(byte[] buffer, int index);
        double ReadDouble(byte[] buffer, int index);
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

        public static readonly IEndian sLittleEndian = new LittleEndian();
        public static readonly IEndian sBigEndian = new BigEndian();

        public static IEndian GetEndian(EEndian endian)
        {
            return endian switch
            {
                EEndian.Little => sLittleEndian,
                EEndian.Big => sBigEndian,
                _ => sLittleEndian
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

        public static IBinaryStreamReader CreateBinaryReader(Stream s, EPlatform platform)
        {
            var bs = new BinaryStreamReader(s);
            return (new BinaryEndianReader(EndianUtils.GetEndian(EndianUtils.GetPlatformEndian(platform)), bs));
        }

        public static IBinaryStreamWriter CreateBinaryWriter(Stream s, EPlatform platform)
        {
            var bs = new BinaryStreamWriter(s);
            return (new BinaryEndianWriter(EndianUtils.GetEndian(EndianUtils.GetPlatformEndian(platform)), bs));
        }

        public static IBinaryStreamWriter CreateBinaryWriter(string filepath, EPlatform platform)
        {
            Stream s = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            BinaryStreamWriter bs = new(s);
            return (new BinaryEndianWriter(EndianUtils.GetEndian(EndianUtils.GetPlatformEndian(platform)), bs));
        }
    }

    public class LittleEndian : IEndian
    {
        public bool little => true;
        public bool big => false;

        public int Write(sbyte v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 1;
        }

        public int Write(byte v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 1;
        }

        public int Write(Int16 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 2;
        }

        public int Write(UInt16 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 2;
        }

        public int Write(Int32 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 4;
        }

        public int Write(UInt32 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 4;
        }

        public int Write(Int64 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 8;
        }

        public int Write(UInt64 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 8;
        }

        public int Write(float v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 4;
        }

        public int Write(double v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 8;
        }

        public sbyte ReadInt8(byte[] buffer, int index)
        {
            return (sbyte)buffer[index];
        }

        public byte ReadUInt8(byte[] buffer, int index)
        {
            return buffer[index];
        }

        public Int16 ReadInt16(byte[] buffer, int index)
        {
            return BitConverter.ToInt16(buffer, index);
        }

        public UInt16 ReadUInt16(byte[] buffer, int index)
        {
            return BitConverter.ToUInt16(buffer, index);
        }

        public Int32 ReadInt32(byte[] buffer, int index)
        {
            return BitConverter.ToInt32(buffer, index);
        }

        public UInt32 ReadUInt32(byte[] buffer, int index)
        {
            return BitConverter.ToUInt32(buffer, index);
        }

        public Int64 ReadInt64(byte[] buffer, int index)
        {
            return BitConverter.ToInt64(buffer, index);
        }

        public UInt64 ReadUInt64(byte[] buffer, int index)
        {
            return BitConverter.ToUInt64(buffer, index);
        }

        public float ReadFloat(byte[] buffer, int index)
        {
            return BitConverter.ToSingle(buffer, index);
        }

        public double ReadDouble(byte[] buffer, int index)
        {
            return BitConverter.ToDouble(buffer, index);
        }

    }

    public class BigEndian : IEndian
    {
        public bool little => false;
        public bool big => true;

        public int Write(sbyte v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 1;
        }

        public int Write(byte v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 1;
        }

        public int Write(Int16 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
            return 2;
        }

        public int Write(UInt16 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
            return 2;
        }

        public int Write(Int32 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return 4;
        }

        public int Write(UInt32 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return 4;
        }

        public int Write(Int64 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return 8;
        }

        public int Write(UInt64 v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return 8;
        }

        public int Write(float v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return 4;
        }

        public int Write(double v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return 8;
        }

        public sbyte ReadInt8(byte[] buffer, int index)
        {
            return (sbyte)buffer[index];
        }

        public byte ReadUInt8(byte[] buffer, int index)
        {
            return buffer[index];
        }

        public Int16 ReadInt16(byte[] buffer, int index)
        {
            Swap(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        public UInt16 ReadUInt16(byte[] buffer, int index)
        {
            Swap(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public Int32 ReadInt32(byte[] buffer, int index)
        {
            Swap(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public UInt32 ReadUInt32(byte[] buffer, int index)
        {
            Swap(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public Int64 ReadInt64(byte[] buffer, int index)
        {
            Swap(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }

        public UInt64 ReadUInt64(byte[] buffer, int index)
        {
            Swap(buffer, 0, 8);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public float ReadFloat(byte[] buffer, int index)
        {
            Swap(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }

        public double ReadDouble(byte[] buffer, int index)
        {
            Swap(buffer, 0, 8);
            return BitConverter.ToDouble(buffer, 0);
        }

        private static void Swap(byte[] b, int s, int l)
        {
            var n = l - 1;
            var h = l / 2;
            for (var i = 0; i < h; i++)
            {
                (b[i], b[n - i]) = (b[n - i], b[i]);
            }
        }
    }

    #endregion
}
