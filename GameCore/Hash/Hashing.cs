using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace GameCore
{
    /// <summary>
    /// Byte[] and String hashing according to the algorithm from the Red Dragon Book
    /// </summary>
    public static class Hashing
    {
        #region Private Methods

        private static void InternalHash(byte[] data, int count, ref uint hash)
        {
            // x 65599
            hash = 0;
            if (data.Length == count)
            {
                foreach (var b in data)
                    hash = hash * 65599 + b;
            }
            else
            {
                for (var i = 0; i < count; ++i)
                    hash = hash * 65599 + data[i];
            }
            hash = hash ^ (hash >> 16);
        }

        private static void InternalHash(ReadOnlySpan<byte> data, ref uint hash)
        {
            // x 65599
            hash = 0;
            foreach (var b in data)
                hash = hash * 65599 + b;
            hash = hash ^ (hash >> 16);
        }

        #endregion
        #region Public Methods

        public static uint Compute(byte[] data, int count)
        {
            uint hash = 0;
            InternalHash(data, count, ref hash);
            return hash;
        }

        public static uint Compute(byte[] data)
        {
            uint hash = 0;
            InternalHash(data, data.Length, ref hash);
            return hash;
        }

        public static uint Compute(ReadOnlySpan<byte> data)
        {
            uint hash = 0;
            InternalHash(data, ref hash);
            return hash;
        }

        public static uint Compute_ASCII(string str)
        {
            uint hash = 0;
            var data = System.Text.Encoding.Default.GetBytes(str);
            InternalHash(data, data.Length, ref hash);
            return hash;
        }

        public static uint Compute_ASCII(string str, int count)
        {
            uint hash = 0;
            var data = System.Text.Encoding.Default.GetBytes(str);
            InternalHash(data, count, ref hash);
            return hash;
        }

        public static uint Compute(byte[] data, int count, ref uint hash)
        {
            InternalHash(data, count, ref hash);
            return hash;
        }

        public static uint Compute(byte[] data, ref uint hash)
        {
            InternalHash(data, data.Length, ref hash);
            return hash;
        }

        public static uint Compute_ASCII(string str, ref uint hash)
        {
            var data = System.Text.Encoding.Default.GetBytes(str);
            InternalHash(data, data.Length, ref hash);
            return hash;
        }

        public static uint Compute_ASCII(string str, int count, ref uint hash)
        {
            var data = System.Text.Encoding.Default.GetBytes(str);
            InternalHash(data, count, ref hash);
            return hash;
        }

        #endregion
        #region UnitTest

        public static bool UnitTest()
        {
            var q = "This is the time for all good men to come to the aid of their country...";
            var qq = "xThis is the time for all good men to come to the aid of their country...";
            var qqq = "xxThis is the time for all good men to come to the aid of their country...";
            var qqqq = "xxxThis is the time for all good men to come to the aid of their country...";

            var qh = Compute_ASCII(q);
            var qqh = Compute_ASCII(qq);
            var qqqh = Compute_ASCII(qqq);
            var qqqqh = Compute_ASCII(qqqq);

            var ghs = q.GetHashCode();

            if (qh != 0x7ee23d2f)
                return false;
            if (qqh != 0x36fbe2be)
                return false;
            if (qqqh != 0x15be7473)
                return false;
            if (qqqqh != 0x75274362)
                return false;

            return true;
        }
        #endregion
    }

    public static class Hashing64
    {
        private static readonly ulong sFnv64Prime = 0x100000001b3;

        // FNV 64a: perform a 64 bit Fowler/Noll/Vo FNV-1a hash on a strung
        public static ulong ComputeFnv(string str)
        {
            var hval = sFnv64Prime;

            /*
             * FNV-1a hash each octet of the string
             */
            foreach (var s in str)
            {
                /* xor the bottom with the current octet */
                hval ^= (ulong)(byte)s;

                /* multiply by the 64 bit FNV magic prime mod 2^64 */
                hval *= sFnv64Prime;
            }

            /* return our new hash value */
            return hval;
        }

        public static uint ComputeMurmur32(string str)
        {
            const uint m = 0x5bd1e995;
            const int r = 24;
            const uint seed = 0;

            var h = (uint)(seed ^ str.Length);

            var len4 = (uint)(str.Length / 4);
            for (var i = 0; i < len4; i++)
            {
                var b1 = (byte)str[i];
                var b2 = (byte)str[i + 1];
                var b3 = (byte)str[i + 2];
                var b4 = (byte)str[i + 3];

                uint k = b1;
                k |= (uint)(b2 << 8);
                k |= (uint)(b3 << 16);
                k |= (uint)(b4 << 24);

                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
            }

            var len = (uint)(str.Length & 3);
            if (len == 3)
            {
                h ^= (uint)((byte)str[str.Length - 3] << 16);
                h ^= (uint)((byte)str[str.Length - 2] << 8);
                h ^= (uint)((byte)str[str.Length - 1]);
                h *= m;
            }
            if (len == 2)
            {
                h ^= (uint)((byte)str[str.Length - 2] << 8);
                h ^= (uint)((byte)str[str.Length - 1]);
                h *= m;
            }
            if (len == 1)
            {
                h ^= (uint)((byte)str[str.Length - 1]);
                h *= m;
            }

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }

        public static ulong ComputeMurmur64(string str)
        {
            const uint m = 0x5bd1e995;
            const int r = 24;
            const uint seed = 0;

            var len = (uint)(str.Length);

            var h1 = seed ^ len;
            uint h2 = 0;

            var i = 0;
            while (len >= 8)
            {
                var k1 = ((uint)str[i + 3] << 24);
                k1 = k1 | ((uint)str[i + 2] << 16);
                k1 = k1 | ((uint)str[i + 1] << 8);
                k1 = k1 | ((uint)str[i + 0]);
                i += 4;

                k1 *= m; k1 ^= k1 >> r; k1 *= m;
                h1 *= m; h1 ^= k1;
                len -= 4;

                var k2 = ((uint)str[i + 3] << 24);
                k2 = k2 | ((uint)str[i + 2] << 16);
                k2 = k2 | ((uint)str[i + 1] << 8);
                k2 = k2 | ((uint)str[i + 0]);
                i += 4;

                k2 *= m; k2 ^= k2 >> r; k2 *= m;
                h2 *= m; h2 ^= k2;
                len -= 4;
            }

            if (len >= 4)
            {
                var k1 = ((uint)str[i + 3] << 24);
                k1 = k1 | ((uint)str[i + 2] << 16);
                k1 = k1 | ((uint)str[i + 1] << 8);
                k1 = k1 | ((uint)str[i + 0]);
                i += 4;

                k1 *= m; k1 ^= k1 >> r; k1 *= m;
                h1 *= m; h1 ^= k1;
                len -= 4;
            }

            if (len == 3)
            {
                h2 ^= (uint)((byte)str[i + 2] << 16);
                h2 ^= (uint)((byte)str[i + 1] << 8);
                h2 ^= (uint)((byte)str[i + 0]);
                h2 *= m;
            }
            if (len == 2)
            {
                h2 ^= (uint)((byte)str[str.Length - 2] << 8);
                h2 ^= (uint)((byte)str[str.Length - 1]);
                h2 *= m;
            }
            if (len == 1)
            {
                h2 ^= (uint)((byte)str[str.Length - 1]);
                h2 *= m;
            }

            h1 ^= h2 >> 18; h1 *= m;
            h2 ^= h1 >> 22; h2 *= m;
            h1 ^= h2 >> 17; h1 *= m;
            h2 ^= h1 >> 19; h2 *= m;

            ulong h = h1;
            h = (h << 32) | h2;
            return h;
        }
    }

    public class Hash
    {
        #region Fields

        private uint _hash = 0;

        #endregion
        #region Public Methods

        public void Clear()
        {
            _hash = 0;
        }

        public uint Compute(byte[] data, int count)
        {
            return Hashing.Compute(data, count, ref _hash);
        }

        public uint Compute(byte[] data)
        {
            return Hashing.Compute(data, data.Length, ref _hash);
        }

        public uint Compute_ASCII(string str)
        {
            var data = System.Text.Encoding.Default.GetBytes(str);
            return Hashing.Compute(data, data.Length, ref _hash);
        }

        public uint Compute_ASCII(string str, int count)
        {
            var data = System.Text.Encoding.Default.GetBytes(str);
            return Hashing.Compute(data, count, ref _hash);
        }

        #endregion
    }

    public static class Sha1
    {

        public static Hash160 Compute(byte[] data)
        {
            var hash = new byte[20];
            SHA1.TryHashData(data, hash, out _);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(byte[] data, int index, int length)
        {
            var hash = new byte[20];
            SHA1.TryHashData(data.AsSpan(index, length), hash, out _);
            return Hash160.ConstructTake(hash);
        }

        public static Hash160 Compute(ReadOnlySpan<byte> data)
        {
            var hash = new byte[20];
            SHA1.TryHashData(data, hash, out _);
            return Hash160.ConstructTake(hash);
        }

        #region UnitTest

        public static Hash160 Compute_ASCII(string str)
        {
            var data = System.Text.Encoding.Default.GetBytes(str);
            return Compute(data);
        }

        public static Hash160 Compute_UTF8(string str)
        {
            var data = System.Text.Encoding.Default.GetBytes(str);
            return Compute(data);
        }

        public static bool UnitTest()
        {
            var q = "abc";
            var qq = "this is the time for all good men to come to the aid of their country...";
            var qqq = ".. this is the time for all good men to come to the aid of their country...";
            var qqqq = "... this is the time for all good men to come to the aid of their country...";

            var qh = Compute_ASCII(q);
            var qqh = Compute_ASCII(qq);
            var qqqh = Compute_ASCII(qqq);
            var qqqqh = Compute_ASCII(qqqq);

            if (qh.ToString() != "A9993E364706816ABA3E25717850C26C")
                return false;
            if (qqh.ToString() != "90BAD383FFC8EF488D2019A1C2A8DDCE")
                return false;
            if (qqqh.ToString() != "49936E9D9F0F02B0998FAEE3B311A529")
                return false;
            if (qqqqh.ToString() != "544863DF61291E5610169DD5EEEA9A8E")
                return false;

            return true;
        }
        #endregion

    }
}
