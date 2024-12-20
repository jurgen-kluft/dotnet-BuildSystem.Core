using System.Diagnostics;
using System.Security.Cryptography;

namespace GameCore
{
    public struct Hash160
    {
        private static readonly byte[] s_hashNull = new byte[20]
        {
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00
        };

        private static readonly byte[] s_hashError = new byte[20]
        {
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF
        };

        public static Hash160 Null => new(s_hashNull, 0);

        public static Hash160 Empty => new(s_hashNull, 0);

        private static Hash160 Error => new(s_hashError, 0);

        public ulong AsHash64()
        {
            ulong hash = 0;
            for (var i = 0; i < 8; i++)
            {
                hash = (hash << 8) | HashData[i];
            }
            return hash;
        }

        public ulong AsHash32()
        {
            ulong hash = 0;
            for (var i = 0; i < 4; i++)
            {
                hash = (hash << 8) | HashData[i];
            }
            return hash;
        }

        public bool IsErrorHash()
        {
            var equal = (HashData[0] == s_hashError[0]);
            for (var j = 1; j < Size && equal; j++)
                equal = (HashData[j] == s_hashError[j]);
            return equal;
        }

        public bool IsNullHash()
        {
            var equal = (HashData[0] == s_hashNull[0]);
            for (var j = 1; j < Size && equal; j++)
                equal = (HashData[j] == s_hashNull[j]);
            return equal;
        }

        private Hash160(byte[] hash)
        {
            HashData = hash;
        }

        public Hash160()
        {
            HashData = new byte[Size];
            CopyFrom(s_hashNull, 0);
        }

        private Hash160(Hash160 other)
        {
            HashData = new byte[Size];
            CopyFrom(other.HashData, 0);
        }

        private Hash160(byte[] array, int start)
        {
            HashData = new byte[Size];
            CopyFrom(array, start);
        }

        public const int Size = 20;

        public static Hash160 ConstructTake(byte[] hash)
        {
            return new Hash160(hash);
        }

        public static Hash160 ConstructCopy(byte[] hash)
        {
            return new Hash160(hash, 0);
        }

        public static Hash160 ConstructCopy(byte[] hash, int start)
        {
            return new Hash160(hash, start);
        }

        private byte[] HashData { get; set; }

        public byte[] Release()
        {
            var h = HashData;
            HashData = new byte[Size];
            CopyFrom(s_hashNull, 0);
            return h;
        }

        public override int GetHashCode()
        {
            var hashcode = BitConverter.ToInt32(HashData, Size - 4);
            return hashcode;
        }

        public override string ToString()
        {
            var length = HashData.Length;
            var chars = new char[length * 2];
            for (var n = 0; n < length; n++)
            {
                var i = n * 2;
                var value = HashData[n];
                var bh = (byte)(value >> 4);
                chars[i] = (char)((bh < 10) ? ('0' + bh) : ('A' + bh - 10));
                var bl = (byte)(value & 0xF);
                chars[i + 1] = (char)((bl < 10) ? ('0' + bl) : ('A' + bl - 10));
            }
            var str = new string(chars);
            while (str.Length < (Size*2))
                str = "0" + str;
            return str;
        }

        private static byte[] FromStringN(string hashStr, int sizeInBytes)
        {
            var hash = new byte[sizeInBytes];

            var nc = 2 * sizeInBytes;
            var str = hashStr;
            while (str.Length < nc)
                str = "0" + str;

            for (var i = 0; i < nc; i += 2)
            {
                var b = 0;
                for (var j = 0; j < 2; ++j)
                {
                    var c = str[i + j];
                    Debug.Assert(char.IsLetterOrDigit(c) && !char.IsLower(c));

                    var n = 0;
                    if (c is >= 'A' and <= 'F')
                        n = (byte)(10 + (c - 'A'));
                    else if (c is >= 'a' and <= 'f')
                        n = (byte)(10 + (c - 'a'));
                    else if (c is >= '0' and <= '9')
                        n = (byte)(0 + (c - '0'));

                    Debug.Assert(n is >= 0 and <= 15);
                    b = (byte)((b << 4) | n);
                }

                hash[i / 2] = (byte)b;
            }
            return hash;
        }

        public static Hash160 FromString(string hashStr)
        {
            return ConstructTake(FromStringN(hashStr, Size));
        }

        public static Hash160 FromDateTime(DateTime dt)
        {
            return FromString($"00000000000000{dt.Ticks:X16}");
        }

        public int Copy(Hash160 other)
        {
            return CopyFrom(other.HashData, 0);
        }

        private int CopyFrom(byte[] hash, int offset)
        {
            for (var j = 0; j < Size; j++)
                HashData[j] = hash[offset + j];
            return Size;
        }

        public int CopyTo(byte[] header)
        {
            return CopyTo(header, 0);
        }

        private int CopyTo(byte[] header, int index)
        {
            for (var j = 0; j < Size; j++)
                header[j + index] = HashData[j];
            return Size;
        }

        public void WriteTo(IBinaryWriter writer)
        {
            writer.Write(HashData, 0, Size);
        }

        public static Hash160 ReadFrom(IBinaryReader reader)
        {
            var hash = new byte[Hash160.Size];
            reader.Read(hash, 0, hash.Length);
            return ConstructTake(hash);
        }

        public static bool operator ==(Hash160 b1, Hash160 b2)
        {
            var equal = Equals(b1.HashData, 0, b2.HashData, 0);
            return equal;
        }

        public static bool operator !=(Hash160 b1, Hash160 b2)
        {
            var equal = Equals(b1.HashData, 0, b2.HashData, 0);
            return equal == false;
        }

        public override bool Equals(object obj)
        {
            return obj is Hash160 other && Equals(this.HashData, 0, other.HashData, 0);
        }

        private int Compare(Hash160 other)
        {
            return Compare(this.HashData, 0, other.HashData, 0);
        }

        public static int Compare(Hash160 a, Hash160 b)
        {
            return Compare(a.HashData, 0, b.HashData, 0);
        }

        private static bool Equals(byte[] that, int thatStart, byte[] other, int otherStart)
        {
            for (var j = 0; j < Size; j++)
            {
                var m = that[thatStart++];
                var o = other[otherStart++];
                if (m != o)
                    return false;
            }
            return true;
        }

        private static int Compare(byte[] that, int thatStart, byte[] other, int otherStart)
        {
            for (var j = 0; j < Size; j++)
            {
                var m = that[thatStart++];
                var o = other[otherStart++];
                if (m != o)
                {
                    if (m < o)
                        return -1;
                    return 1;
                }
            }
            return 0;
        }

        #region Comparer (IEqualityComparer)

        public class Comparer : IEqualityComparer<Hash160>
        {
            public bool Equals(Hash160 a, Hash160 b)
            {
                return Hash160.Compare(a, b) == 0;
            }

            public int GetHashCode(Hash160 r)
            {
                return r.GetHashCode();
            }
        }

        #endregion

        public static void UnitTest()
        {
            Hash160 h1 = new ();
            Hash160 h2 = new ();

            Hash160 h3 = new (Hash160.Error);
            Hash160 h4 = new (Hash160.Error);

            Debug.Assert(h1 == Hash160.Null);
            Debug.Assert(h2 == Hash160.Null);
            Debug.Assert(h1 != Hash160.Error);
            Debug.Assert(h2 != Hash160.Error);
            Debug.Assert(h1 == h2);
            Debug.Assert(h1.Compare(h2) == 0);
            Debug.Assert(h1.GetHashCode() == h2.GetHashCode());

            Debug.Assert(h3 != Hash160.Null);
            Debug.Assert(h4 != Hash160.Null);
            Debug.Assert(h3 == Hash160.Error);
            Debug.Assert(h4 == Hash160.Error);
            Debug.Assert(h3 == h4);
            Debug.Assert(h3.Compare(h4) == 0);
            Debug.Assert(h3.GetHashCode() == h4.GetHashCode());

            Debug.Assert(h1 != h3);
            Debug.Assert(h2 != h4);

            Debug.Assert(Hash160.Error != Hash160.Null);
            Debug.Assert(Hash160.Error.Compare(Hash160.Null) == 1);
            Debug.Assert(Hash160.Null.Compare(Hash160.Error) == -1);
            Debug.Assert(Hash160.Error.GetHashCode() != Hash160.Null.GetHashCode());
        }
    };

    public static class HashUtility
    {
        #region Methods

        private static readonly SHA1 s_sha1 = SHA1.Create();

        public static Hash160 Random()
        {
            var hash = new byte[Hash160.Size];
            var l1 = (long)0;
            for (uint i = 0; i < Hash160.Size; i += 8)
            {
                if ((i & 7) == 0)
                {
                    l1 = System.Random.Shared.NextInt64();
                }
                hash[i] = (byte)l1;
                l1 >>= 8;
            }
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(byte[] data)
        {
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash(data);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(byte[] data, int start, int length)
        {
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash(data, start, length);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(Type type)
        {
            var data = System.Text.Encoding.ASCII.GetBytes(type.Name);
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash(data);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute_ASCII(string str)
        {
            var data = System.Text.Encoding.ASCII.GetBytes(str);
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash(data);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute_UTF8(string str)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(str);
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash(data);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(FileInfo s)
        {
            if (!s.Exists)
                return Hash160.Null;

            try
            {
                var block = new byte[256 * 1024];

                using FileStream fs = new(s.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var total = fs.Length;
                long offset = 0;
                var stride = (256 * 1024);
                while (offset < total)
                {
                    if ((offset + stride) > total)
                    {
                        stride = (int)(total - offset);
                    }
                    var read = fs.Read(block, 0, stride);
                    offset += read;
                    s_sha1.TransformBlock(block, 0, read, null, 0);
                }
                s_sha1.TransformFinalBlock(block, 0, 0);
                return Hash160.ConstructTake(s_sha1.Hash);
            }
            catch (Exception e)
            {
                Console.WriteLine("[HashUtility:EXCEPTION]{0}", e);
                return Hash160.Null;
            }
        }

        private static Hash160 Compute(MemoryStream ms)
        {
            var data = ms.GetBuffer();
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash(data, 0, (int)ms.Length);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(ReadOnlySpan<byte> data)
        {
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash(data.ToArray());
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(sbyte[] v)
        {
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash((byte[])(Array)v);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(sbyte[] v, int index, int count)
        {
            s_sha1.Initialize();
            var hash = s_sha1.ComputeHash((byte[])(Array)v, index, count);
            return Hash160.ConstructTake(hash);
        }

        public static void Begin()
        {
            s_sha1.Initialize();
        }

        public static void Update(byte[] data, int start, int length)
        {
            s_sha1.TransformBlock(data, start, length, null, 0);
        }

        public static Hash160 End()
        {
            s_sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            var hash = s_sha1.Hash;
            return Hash160.ConstructTake(hash);
        }

        #endregion
    }
}
