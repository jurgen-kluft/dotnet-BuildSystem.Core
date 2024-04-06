using System;
using System.IO;

namespace GameCore
{
    #region Endian Classes

    public interface IEndian
    {
        bool little { get; }
        bool big { get; }

        int GetBytes(byte v, byte[] buffer);
        int GetBytes(sbyte v, byte[] buffer);
        int GetBytes(Int16 v, byte[] buffer);
        int GetBytes(UInt16 v, byte[] buffer);
        int GetBytes(Int32 v, byte[] buffer);
        int GetBytes(UInt32 v, byte[] buffer);
        int GetBytes(Int64 v, byte[] buffer);
        int GetBytes(UInt64 v, byte[] buffer);
        int GetBytes(float v, byte[] buffer);
        int GetBytes(double v, byte[] buffer);

        Int16 Convert(Int16 v);
        UInt16 Convert(UInt16 v);
        Int32 Convert(Int32 v);
        UInt32 Convert(UInt32 v);
        Int64 Convert(Int64 v);
        UInt64 Convert(UInt64 v);

        float ConvertFloat(byte[] bytes, int index);
        double ConvertDouble(byte[] bytes, int index);
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

        public static IBinaryStream CreateBinaryStream2(Stream s, EPlatform platform)
        {
            BinaryStream bs = new BinaryStream(s);
            return bs;
        }

        public static IDataWriter CreateDataWriter(EPlatform platform)
        {
            return new DataWriter(platform);
        }

        public static IBinaryStreamReader CreateBinaryReader(Stream s, EPlatform platform)
        {
            BinaryReader br = new BinaryReader(s);
            if (EndianUtils.GetPlatformEndian(platform) == EEndian.Little)
                return (new BinaryReaderLittleEndian(br));
            return (new BinaryReaderBigEndian(br));
        }

        public static IBinaryStreamWriter CreateBinaryWriter(Stream s, EPlatform platform)
        {
            var bs = new BinaryStream(s);
            return (new BinaryEndianWriter(EndianUtils.GetEndian(EndianUtils.GetPlatformEndian(platform)), bs));
        }

        public static IBinaryStreamWriter CreateBinaryWriter(string filepath, EPlatform platform)
        {
            Stream s = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            BinaryStream bs = new(s);
            return (new BinaryEndianWriter(EndianUtils.GetEndian(EndianUtils.GetPlatformEndian(platform)), bs));
        }
    }

    public class LittleEndian : IEndian
    {
        public bool little => true;
        public bool big => false;

        public int GetBytes(sbyte v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 1;
        }

        public int GetBytes(byte v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 1;
        }

        public int GetBytes(Int16 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 2;
        }

        public int GetBytes(UInt16 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 2;
        }

        public int GetBytes(Int32 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 4;
        }

        public int GetBytes(UInt32 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 4;
        }

        public int GetBytes(Int64 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 8;
        }

        public int GetBytes(UInt64 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 8;
        }

        public int GetBytes(float v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 4;
        }

        public int GetBytes(double v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 8;
        }

        public Int16 Convert(Int16 v)
        {
            return v;
        }

        public UInt16 Convert(UInt16 v)
        {
            return v;
        }

        public Int32 Convert(Int32 v)
        {
            return v;
        }

        public UInt32 Convert(UInt32 v)
        {
            return v;
        }

        public Int64 Convert(Int64 v)
        {
            return v;
        }

        public UInt64 Convert(UInt64 v)
        {
            return v;
        }

        public float ConvertFloat(byte[] bytes, int index)
        {
            return BitConverter.ToSingle(bytes, index);
        }

        public double ConvertDouble(byte[] bytes, int index)
        {
            return BitConverter.ToDouble(bytes, index);
        }
    }

    public class BigEndian : IEndian
    {
        public bool little => false;
        public bool big => true;

        public int GetBytes(sbyte v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 1;
        }

        public int GetBytes(byte v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            return 1;
        }

        public int GetBytes(Int16 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            Swap(buffer, 0, 2);
            return 2;
        }

        public int GetBytes(UInt16 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            Swap(buffer, 0, 2);
            return 2;
        }

        public int GetBytes(Int32 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            Swap(buffer, 0, 4);
            return 4;
        }

        public int GetBytes(UInt32 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            Swap(buffer, 0, 4);
            return 4;
        }

        public int GetBytes(Int64 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            Swap(buffer, 0, 8);
            return 8;
        }

        public int GetBytes(UInt64 v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            Swap(buffer, 0, 8);
            return 8;
        }

        public int GetBytes(float v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            Swap(buffer, 0, 4);
            return 4;
        }

        public int GetBytes(double v, byte[] buffer)
        {
            BitConverter.TryWriteBytes(buffer, v);
            Swap(buffer, 0, 8);
            return 8;
        }


        private byte[] _buffer = new byte[8];

        public Int16 Convert(Int16 v)
        {
            var bytes = GetBytes(v, _buffer);
            return BitConverter.ToInt16(_buffer);
        }

        public UInt16 Convert(UInt16 v)
        {
            var bytes = GetBytes(v, _buffer);
            return BitConverter.ToUInt16(_buffer);
        }

        public Int32 Convert(Int32 v)
        {
            var bytes = GetBytes(v, _buffer);
            return BitConverter.ToInt32(_buffer);
        }

        public UInt32 Convert(UInt32 v)
        {
            var bytes = GetBytes(v, _buffer);
            return BitConverter.ToUInt32(_buffer);
        }

        public Int64 Convert(Int64 v)
        {
            var bytes = GetBytes(v, _buffer);
            return BitConverter.ToInt64(_buffer);
        }

        public UInt64 Convert(UInt64 v)
        {
            var bytes = GetBytes(v, _buffer);
            return BitConverter.ToUInt64(_buffer);
        }

        public float ConvertFloat(byte[] bytes, int index)
        {
            Swap(bytes, index, 4);
            return BitConverter.ToSingle(bytes, index);
        }

        public double ConvertDouble(byte[] bytes, int index)
        {
            Swap(bytes, index, 4);
            return BitConverter.ToDouble(bytes, index);
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
