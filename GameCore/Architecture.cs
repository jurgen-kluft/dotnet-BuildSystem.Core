using System;
using System.IO;

namespace GameCore
{
    #region Endian Classes

    public interface IArchitecture
    {
        bool IsLittle { get; }
        bool Is64Bit { get; }

        int Write(byte v, byte[] buffer, int offset);
        int Write(sbyte v, byte[] buffer, int offset);
        int Write(short v, byte[] buffer, int offset);
        int Write(ushort v, byte[] buffer, int offset);
        int Write(int v, byte[] buffer, int offset);
        int Write(uint v, byte[] buffer, int offset);
        int Write(long v, byte[] buffer, int offset);
        int Write(ulong v, byte[] buffer, int offset);
        int Write(float v, byte[] buffer, int offset);
        int Write(double v, byte[] buffer, int offset);

        sbyte ReadInt8(byte[] buffer, int index);
        byte ReadUInt8(byte[] buffer, int index);
        short ReadInt16(byte[] buffer, int index);
        ushort ReadUInt16(byte[] buffer, int index);
        int ReadInt32(byte[] buffer, int index);
        uint ReadUInt32(byte[] buffer, int index);
        long ReadInt64(byte[] buffer, int index);
        ulong ReadUInt64(byte[] buffer, int index);
        float ReadFloat(byte[] buffer, int index);
        double ReadDouble(byte[] buffer, int index);
    }

    public static class ArchitectureUtils
    {
        public static IArchitecture GetPlatformArchitecture(EPlatform platform)
        {
            return platform switch
            {
                EPlatform.Win32 => LittleArchitecture64,
                EPlatform.Win64 => LittleArchitecture64,
                EPlatform.Mac => LittleArchitecture64,
                EPlatform.XboxOne => LittleArchitecture64,
                EPlatform.XboxOneX => LittleArchitecture64,
                EPlatform.Playstation4 => LittleArchitecture64,
                EPlatform.Playstation4Pro => LittleArchitecture64,
                EPlatform.XboxSeriesS => LittleArchitecture64,
                EPlatform.XboxSeriesX => LittleArchitecture64,
                EPlatform.Playstation5 => LittleArchitecture64,
                EPlatform.NintendoSwitch => LittleArchitecture64,
                _ => LittleArchitecture64
            };
        }

        public static readonly IArchitecture LittleArchitecture64 = new LittleArchitecture64();
        public static readonly IArchitecture BigArchitecture64 = new BigArchitecture64();

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
            buffer[offset] = (byte)v;
            return 1;
        }

        public int Write(byte v, byte[] buffer, int offset)
        {
            buffer[offset] = v;
            return 1;
        }

        public int Write(short v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 2;
        }

        public int Write(ushort v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 2;
        }

        public int Write(int v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 4;
        }

        public int Write(uint v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 4;
        }

        public int Write(long v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            return 8;
        }

        public int Write(ulong v, byte[] buffer, int offset)
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

        public short ReadInt16(byte[] buffer, int index)
        {
            return BitConverter.ToInt16(buffer, index);
        }

        public ushort ReadUInt16(byte[] buffer, int index)
        {
            return BitConverter.ToUInt16(buffer, index);
        }

        public int ReadInt32(byte[] buffer, int index)
        {
            return BitConverter.ToInt32(buffer, index);
        }

        public uint ReadUInt32(byte[] buffer, int index)
        {
            return BitConverter.ToUInt32(buffer, index);
        }

        public long ReadInt64(byte[] buffer, int index)
        {
            return BitConverter.ToInt64(buffer, index);
        }

        public ulong ReadUInt64(byte[] buffer, int index)
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
            buffer[offset] = (byte)v;
            return 1;
        }

        public int Write(byte v, byte[] buffer, int offset)
        {
            buffer[offset] = v;
            return 1;
        }

        public int Write(short v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
            return 2;
        }

        public int Write(ushort v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
            return 2;
        }

        public int Write(int v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return 4;
        }

        public int Write(uint v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[3 - 0]) = (buffer[3 - 0], buffer[0]);
            (buffer[1], buffer[3 - 1]) = (buffer[3 - 1], buffer[1]);
            return 4;
        }

        public int Write(long v, byte[] buffer, int offset)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), v);
            (buffer[0], buffer[7 - 0]) = (buffer[7 - 0], buffer[0]);
            (buffer[1], buffer[7 - 1]) = (buffer[7 - 1], buffer[1]);
            (buffer[2], buffer[7 - 2]) = (buffer[7 - 2], buffer[2]);
            (buffer[3], buffer[7 - 3]) = (buffer[7 - 3], buffer[3]);
            return 8;
        }

        public int Write(ulong v, byte[] buffer, int offset)
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

        public short ReadInt16(byte[] buffer, int index)
        {
            Swap(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        public ushort ReadUInt16(byte[] buffer, int index)
        {
            Swap(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public int ReadInt32(byte[] buffer, int index)
        {
            Swap(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public uint ReadUInt32(byte[] buffer, int index)
        {
            Swap(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public long ReadInt64(byte[] buffer, int index)
        {
            Swap(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }

        public ulong ReadUInt64(byte[] buffer, int index)
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
