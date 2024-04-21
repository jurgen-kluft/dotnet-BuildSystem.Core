using System;
using System.IO;

namespace GameCore
{
    #region Endian Classes

    public interface IArchitecture
    {
        bool IsLittle{ get; }
        bool Is64Bit { get; }

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

    public static class ArchitectureUtils
    {
        public static IArchitecture GetPlatformArchitecture(EPlatform platform)
        {
            return platform switch
            {
                EPlatform.Win32 => LittleArchitecture,
                EPlatform.Win64 => LittleArchitecture,
                EPlatform.Mac => LittleArchitecture,
                EPlatform.XboxOne => LittleArchitecture,
                EPlatform.XboxOneX => LittleArchitecture,
                EPlatform.PS4 => LittleArchitecture,
                EPlatform.PS4Pro => LittleArchitecture,
                EPlatform.XboxSeriesS => LittleArchitecture,
                EPlatform.XboxSeriesX => LittleArchitecture,
                EPlatform.PS5 => LittleArchitecture,
                EPlatform.NintendoSwitch => LittleArchitecture,
                _ => LittleArchitecture
            };
        }

        public static readonly IArchitecture LittleArchitecture = new LittleArchitecture64();
        public static readonly IArchitecture BigArchitecture = new BigArchitecture64();

        public static IArchitecture GetLittleEndianArchitecture()
        {
            return LittleArchitecture;
        }

        public static IArchitecture GetBigEndianArchitecture()
        {
            return BigArchitecture;
        }

        public static IArchitecture GetEndianForPlatform(EPlatform platform)
        {
            return GetPlatformArchitecture(platform);
        }

        public static bool IsPlatform64Bit(EPlatform platform)
        {
            return (platform & EPlatform.Arch64) != 0;
        }

        public static IBinaryStreamReader CreateBinaryReader(Stream s, EPlatform platform)
        {
            var bs = new BinaryStreamReader(s);
            return new BinaryEndianReader(ArchitectureUtils.GetPlatformArchitecture(platform), bs);
        }

        public static IBinaryStreamWriter CreateBinaryWriter(Stream s, EPlatform platform)
        {
            var bs = new BinaryStreamWriter(s);
            return new BinaryEndianWriter(ArchitectureUtils.GetPlatformArchitecture(platform), bs);
        }

        public static IBinaryStreamWriter CreateBinaryWriter(string filepath, EPlatform platform)
        {
            Stream s = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            BinaryStreamWriter bs = new(s);
            return new BinaryEndianWriter(ArchitectureUtils.GetPlatformArchitecture(platform), bs);
        }
    }

    public class LittleArchitecture64 : IArchitecture
    {
        public bool IsLittle => true;
        public bool Is64Bit => true;

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

    public class BigArchitecture64 : IArchitecture
    {
        public bool IsLittle => false;
        public bool Is64Bit => true;

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
