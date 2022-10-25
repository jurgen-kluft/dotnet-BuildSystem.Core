using System;
using System.IO;

namespace GameCore
{
    #region Endian Classes

    public struct ByteSpan
    {
        public byte[] Buffer { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }

        public static ByteSpan AsSpan(byte[] buffer, int length)
        {
            return new ByteSpan() { Buffer = buffer, Start = 0, Length = length};
        }
        public static ByteSpan AsSpan(byte[] buffer, int start, int length)
        {
            return new ByteSpan() { Buffer = buffer, Start = start, Length = length};
        }
    }

    public interface IEndian
    {
        bool little { get; }
        bool big { get; }

        ByteSpan GetBytes(Int16 v);
        ByteSpan GetBytes(UInt16 v);
        ByteSpan GetBytes(Int32 v);
        ByteSpan GetBytes(UInt32 v);
        ByteSpan GetBytes(Int64 v);
        ByteSpan GetBytes(UInt64 v);
        ByteSpan GetBytes(float v);
        ByteSpan GetBytes(double v);

        Int16 Convert(Int16 v);
        UInt16 Convert(UInt16 v);
        Int32 Convert(Int32 v);
        UInt32 Convert(UInt32 v);
        Int64 Convert(Int64 v);
        UInt64 Convert(UInt64 v);

        float ConvertFloat(ByteSpan bytes);
        double ConvertDouble(ByteSpan bytes);
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

        public static bool IsPlatform64Bit(EPlatform platform)
        {
            return (platform & EPlatform.Arch64) != 0;
        }

        public static IBinaryStream CreateBinaryStream(Stream s, EPlatform platform)
        {
            BinaryWriter bw = new BinaryWriter(s);
            if (EndianUtils.GetPlatformEndian(platform) == EEndian.Little)
                return new BinaryWriterLittleEndian(bw);
            return new BinaryWriterBigEndian(bw);
        }

        public static IDataWriter CreateDataWriter(EPlatform platform)
        {
            return new DataWriter(platform);
        }

        public static IBinaryReader CreateBinaryReader(Stream s, EPlatform platform)
        {
            BinaryReader br = new BinaryReader(s);
            if (EndianUtils.GetPlatformEndian(platform) == EEndian.Little)
                return (new BinaryReaderLittleEndian(br));
            return (new BinaryReaderBigEndian(br));
        }
    }

    public class LittleEndian : IEndian
    {
        private byte[] Buffer = new byte[8];
        public bool little => true;
        public bool big => false;

        public ByteSpan GetBytes(Int16 v) { BitConverter.TryWriteBytes(Buffer, v); return ByteSpan.AsSpan(Buffer, 0, 2); }
        public ByteSpan GetBytes(UInt16 v) { BitConverter.TryWriteBytes(Buffer, v); return ByteSpan.AsSpan(Buffer, 0, 2); }
        public ByteSpan GetBytes(Int32 v) { BitConverter.TryWriteBytes(Buffer, v); return ByteSpan.AsSpan(Buffer, 0, 4); }
        public ByteSpan GetBytes(UInt32 v) { BitConverter.TryWriteBytes(Buffer, v); return ByteSpan.AsSpan(Buffer, 0, 4); }
        public ByteSpan GetBytes(Int64 v) { BitConverter.TryWriteBytes(Buffer, v); return ByteSpan.AsSpan(Buffer, 0, 8); }
        public ByteSpan GetBytes(UInt64 v) { BitConverter.TryWriteBytes(Buffer, v); return ByteSpan.AsSpan(Buffer, 0, 8); }
        public ByteSpan GetBytes(float v) { BitConverter.TryWriteBytes(Buffer, v); return ByteSpan.AsSpan(Buffer, 0, 4); }
        public ByteSpan GetBytes(double v) { BitConverter.TryWriteBytes(Buffer, v); return ByteSpan.AsSpan(Buffer, 0, 8); }

        public Int16 Convert(Int16 v) { return v;}
        public UInt16 Convert(UInt16 v) { return v;}
        public Int32 Convert(Int32 v) { return v;}
        public UInt32 Convert(UInt32 v) { return v;}
        public Int64 Convert(Int64 v) { return v;}
        public UInt64 Convert(UInt64 v) { return v;}
        public float ConvertFloat(ByteSpan bytes) { return BitConverter.ToSingle(bytes.Buffer, bytes.Start); }
        public double ConvertDouble(ByteSpan bytes) { return BitConverter.ToDouble(bytes.Buffer, bytes.Start); }

    }

    public class BigEndian : IEndian
    {
        private byte[] Buffer = new byte[8];
        public bool little => false;
        public bool big => true;

        public ByteSpan GetBytes(Int16 v) { BitConverter.TryWriteBytes(Buffer, v); Swap(Buffer, 0, 2); return ByteSpan.AsSpan(Buffer, 0,2); }
        public ByteSpan GetBytes(UInt16 v) { BitConverter.TryWriteBytes(Buffer, v); Swap(Buffer, 0, 2); return ByteSpan.AsSpan(Buffer, 0,2); }
        public ByteSpan GetBytes(Int32 v) { BitConverter.TryWriteBytes(Buffer, v); Swap(Buffer, 0, 4);  return ByteSpan.AsSpan(Buffer, 0,4); }
        public ByteSpan GetBytes(UInt32 v) { BitConverter.TryWriteBytes(Buffer, v); Swap(Buffer, 0, 4); return ByteSpan.AsSpan(Buffer, 0,4); }
        public ByteSpan GetBytes(Int64 v) { BitConverter.TryWriteBytes(Buffer, v); Swap(Buffer, 0, 8);  return ByteSpan.AsSpan(Buffer, 0,8); }
        public ByteSpan GetBytes(UInt64 v) { BitConverter.TryWriteBytes(Buffer, v); Swap(Buffer, 0, 8); return ByteSpan.AsSpan(Buffer, 0,8); }
        public ByteSpan GetBytes(float v) { BitConverter.TryWriteBytes(Buffer, v); Swap(Buffer, 0, 4);  return ByteSpan.AsSpan(Buffer, 0,4); }
        public ByteSpan GetBytes(double v) { BitConverter.TryWriteBytes(Buffer, v); Swap(Buffer, 0, 8); return ByteSpan.AsSpan(Buffer, 0,8); }


        public Int16 Convert(Int16 v) { var bytes = GetBytes(v); return BitConverter.ToInt16(bytes.Buffer);}
        public UInt16 Convert(UInt16 v) { var bytes = GetBytes(v); return BitConverter.ToUInt16(bytes.Buffer);}
        public Int32 Convert(Int32 v) { var bytes = GetBytes(v); return BitConverter.ToInt32(bytes.Buffer);}
        public UInt32 Convert(UInt32 v) { var bytes = GetBytes(v); return BitConverter.ToUInt32(bytes.Buffer);}
        public Int64 Convert(Int64 v) { var bytes = GetBytes(v); return BitConverter.ToInt64(bytes.Buffer);}
        public UInt64 Convert(UInt64 v) { var bytes = GetBytes(v); return BitConverter.ToUInt64(bytes.Buffer);}
        public float ConvertFloat(ByteSpan bytes) { Swap(bytes.Buffer, bytes.Start, 4); return BitConverter.ToSingle(bytes.Buffer, bytes.Start); }
        public double ConvertDouble(ByteSpan bytes) { Swap(bytes.Buffer, bytes.Start, 4); return BitConverter.ToDouble(bytes.Buffer, bytes.Start); }

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
