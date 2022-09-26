using System;
using System.Runtime.CompilerServices;

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
                foreach (byte b in data)
                    hash = hash * 65599 + b;
            }
            else
            {
                for (int i = 0; i < count; ++i)
                    hash = hash * 65599 + data[i];
            }
            hash = hash ^ (hash >> 16);
        }

        #endregion
        #region Public Methods

        public static UInt32 Compute(byte[] data, int count)
        {
            uint hash = 0;
            InternalHash(data, count, ref hash);
            return hash;
        }

        public static UInt32 Compute(byte[] data)
        {
            uint hash = 0;
            InternalHash(data, data.Length, ref hash);
            return hash;
        }

        public static UInt32 Compute_ASCII(string str)
        {
            uint hash = 0;
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(str);
            InternalHash(data, data.Length, ref hash);
            return hash;
        }

        public static UInt32 Compute_ASCII(string str, int count)
        {
            uint hash = 0;
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(str);
            InternalHash(data, count, ref hash);
            return hash;
        }

        public static UInt32 Compute(byte[] data, int count, ref uint hash)
        {
            InternalHash(data, count, ref hash);
            return hash;
        }

        public static UInt32 Compute(byte[] data, ref uint hash)
        {
            InternalHash(data, data.Length, ref hash);
            return hash;
        }

        public static UInt32 Compute_ASCII(string str, ref uint hash)
        {
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(str);
            InternalHash(data, data.Length, ref hash);
            return hash;
        }

        public static UInt32 Compute_ASCII(string str, int count, ref uint hash)
        {
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(str);
            InternalHash(data, count, ref hash);
            return hash;
        }

        #endregion
        #region UnitTest

        public static bool UnitTest()
        {
            string q = "This is the time for all good men to come to the aid of their country...";
            string qq = "xThis is the time for all good men to come to the aid of their country...";
            string qqq = "xxThis is the time for all good men to come to the aid of their country...";
            string qqqq = "xxxThis is the time for all good men to come to the aid of their country...";

            UInt32 qh = Compute_ASCII(q);
            UInt32 qqh = Compute_ASCII(qq);
            UInt32 qqqh = Compute_ASCII(qqq);
            UInt32 qqqqh = Compute_ASCII(qqqq);

            Int32 ghs = q.GetHashCode();

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
        private static readonly UInt64 FNV_64_PRIME = 0x100000001b3;

        // FNV 64a: perform a 64 bit Fowler/Noll/Vo FNV-1a hash on a strung
        public static UInt64 ComputeFNV(string str)
        {
            UInt64 hval = FNV_64_PRIME;

            /*
             * FNV-1a hash each octet of the string
             */
            foreach (char s in str)
            {
                /* xor the bottom with the current octet */
                hval ^= (UInt64)(byte)s;

                /* multiply by the 64 bit FNV magic prime mod 2^64 */
                hval *= FNV_64_PRIME;
            }

            /* return our new hash value */
            return hval;
        }

        public static UInt32 ComputeMurmur32(string str)
        {
            const uint m = 0x5bd1e995;
            const int r = 24;
            const uint seed = 0;

            uint h = (uint)(seed ^ str.Length);

            uint len4 = (uint)(str.Length / 4);
            for (int i = 0; i < len4; i++)
            {
                byte b1 = (byte)str[i];
                byte b2 = (byte)str[i + 1];
                byte b3 = (byte)str[i + 2];
                byte b4 = (byte)str[i + 3];

                uint k;
                k = b1;
                k |= (uint)(b2 << 8);
                k |= (uint)(b3 << 16);
                k |= (uint)(b4 << 24);

                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
            }

            uint len = (uint)(str.Length & 3);
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

        public static UInt64 ComputeMurmur64(string str)
        {
            const uint m = 0x5bd1e995;
            const int r = 24;
            const uint seed = 0;

            uint len = (uint)(str.Length);

            uint h1 = seed ^ len;
            uint h2 = 0;

            int i = 0;
            while (len >= 8)
            {
                uint k1 = ((uint)str[i + 3] << 24);
                k1 = k1 | ((uint)str[i + 2] << 16);
                k1 = k1 | ((uint)str[i + 1] << 8);
                k1 = k1 | ((uint)str[i + 0]);
                i += 4;

                k1 *= m; k1 ^= k1 >> r; k1 *= m;
                h1 *= m; h1 ^= k1;
                len -= 4;

                uint k2 = ((uint)str[i + 3] << 24);
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
                uint k1 = ((uint)str[i + 3] << 24);
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

            UInt64 h = h1;
            h = (h << 32) | h2;
            return h;
        }
    }

    public class Hash
    {
        #region Fields

        private uint mHash = 0;

        #endregion
        #region Public Methods

        public void Clear()
        {
            mHash = 0;
        }

        public UInt32 Compute(byte[] data, int count)
        {
            return Hashing.Compute(data, count, ref mHash);
        }

        public UInt32 Compute(byte[] data)
        {
            return Hashing.Compute(data, data.Length, ref mHash);
        }

        public UInt32 Compute_ASCII(string str)
        {
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(str);
            return Hashing.Compute(data, data.Length, ref mHash);
        }

        public UInt32 Compute_ASCII(string str, int count)
        {
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(str);
            return Hashing.Compute(data, count, ref mHash);
        }

        #endregion
    }

    public static class SHA1
    {
        public sealed class SHA1Hash
        {
            private SHA1Context ctx;

            public SHA1Hash()
            {
                ctx = new();
            }

            public void Init()
            {
                ctx.Init();
            }

            public Hash160 Compute(byte[] buf, int index, int len)
            {
                ctx.Init();
                SHA1Imp.Update(ctx, buf, index, len);
                return SHA1Imp.Finalize(ctx);
            }

            public void Update(byte[] buf, int index, int len) =>
                SHA1Imp.Update(ctx, buf, index, len);

            public Hash160 Finalize() => SHA1Imp.Finalize(ctx);
        }

        sealed class SHA1Context
        {
            public uint[] State;
            public uint[] Count;
            public byte[] Buffer;

            public SHA1Context()
            {
                State = new uint[5];
                Count = new uint[2];
                Buffer = new byte[64];
                Init();
            }

            public void Init()
            {
                State[0] = 0x67452301;
                State[1] = 0xEFCDAB89;
                State[2] = 0x98BADCFE;
                State[3] = 0x10325476;
                State[4] = 0xC3D2E1F0;
                Count[0] = Count[1] = 0;
            }
        }

        static class SHA1Imp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint Rol(uint value, int bits) =>
                ((value) << (bits)) | ((value) >> (32 - (bits)));

            // LITTLE_ENDIAN
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint Blk0(int i, uint[] block) =>
                (block[i] = (Rol(block[i], 24) & 0xFF00FF00) | (Rol(block[i], 8) & 0x00FF00FF));

            // BIG_ENDIAN
            /*
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint Blk0(int i, uint[] block) =>
                block[i];
             */

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint Blk(int i, uint[] block) =>
                (block[i & 15] = Rol(block[(i + 13) & 15] ^ block[(i + 8) & 15] ^ block[(i + 2) & 15] ^ block[i & 15], 1));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void R0(uint v, ref uint w, uint x, uint y, ref uint z, int i, uint[] block)
            {
                z += ((w & (x ^ y)) ^ y) + Blk0(i, block) + 0x5A827999 + Rol(v, 5); w = Rol(w, 30);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void R1(uint v, ref uint w, uint x, uint y, ref uint z, int i, uint[] block)
            {
                z += ((w & (x ^ y)) ^ y) + Blk(i, block) + 0x5A827999 + Rol(v, 5); w = Rol(w, 30);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void R2(uint v, ref uint w, uint x, uint y, ref uint z, int i, uint[] block)
            {
                z += (w ^ x ^ y) + Blk(i, block) + 0x6ED9EBA1 + Rol(v, 5); w = Rol(w, 30);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void R3(uint v, ref uint w, uint x, uint y, ref uint z, int i, uint[] block)
            {
                z += (((w | x) & y) | (w & x)) + Blk(i, block) + 0x8F1BBCDC + Rol(v, 5); w = Rol(w, 30);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void R4(uint v, ref uint w, uint x, uint y, ref uint z, int i, uint[] block)
            {
                z += (w ^ x ^ y) + Blk(i, block) + 0xCA62C1D6 + Rol(v, 5); w = Rol(w, 30);
            }

            private static void Copy(uint[] block, byte[] buffer, int ofs)
            {
                int i = 0;
                int j = 0;
                uint x = 0x0;

                while (i < 64)
                {
                    x = x >> 8;
                    x = x | ((uint)(buffer[ofs++] << 24));
                    if ((i & 0x3) == 3)
                    {
                        block[j++] = x;
                        x = 0x0;
                    }
                    i++;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Copy(byte[] dest, int destOfs, byte[] src, int srcOfs, int count)
            {
                for (int i = 0; i < count; i++)
                    dest[destOfs++] = src[srcOfs++];
            }

            public static void Update(SHA1Context context, byte[] data, int index, int len)
            {
                uint i;

                uint j;

                j = context.Count[0];
                if ((context.Count[0] += (uint)len << 3) < j)
                    context.Count[1]++;
                context.Count[1] += (uint)len >> 29;
                j = (j >> 3) & 63;
                if ((j + len) > 63)
                {
                    i = 64 - j;

                    Copy(context.Buffer, (int)j, data, index, (int)i);
                    Transform(context.State, context.Buffer, 0);
                    for (; i + 63 < len; i += 64)
                    {
                        Transform(context.State, data, index + (int)i);
                    }
                    j = 0;
                }
                else
                {
                    i = 0;
                }
                Copy(context.Buffer, (int)j, data, index + (int)i, (int)(len - i));
            }

            private static void Transform(uint[] state, byte[] buffer, int ofs)
            {

                uint a, b, c, d, e;
                uint[] block = new uint[16];

                Copy(block, buffer, ofs);

                /* Copy context->state[] to working vars */
                a = state[0];
                b = state[1];
                c = state[2];
                d = state[3];
                e = state[4];
                /* 4 rounds of 20 operations each. Loop unrolled. */
                R0(a, ref b, c, d, ref e, 0, block);
                R0(e, ref a, b, c, ref d, 1, block);
                R0(d, ref e, a, b, ref c, 2, block);
                R0(c, ref d, e, a, ref b, 3, block);
                R0(b, ref c, d, e, ref a, 4, block);
                R0(a, ref b, c, d, ref e, 5, block);
                R0(e, ref a, b, c, ref d, 6, block);
                R0(d, ref e, a, b, ref c, 7, block);
                R0(c, ref d, e, a, ref b, 8, block);
                R0(b, ref c, d, e, ref a, 9, block);
                R0(a, ref b, c, d, ref e, 10, block);
                R0(e, ref a, b, c, ref d, 11, block);
                R0(d, ref e, a, b, ref c, 12, block);
                R0(c, ref d, e, a, ref b, 13, block);
                R0(b, ref c, d, e, ref a, 14, block);
                R0(a, ref b, c, d, ref e, 15, block);
                R1(e, ref a, b, c, ref d, 16, block);
                R1(d, ref e, a, b, ref c, 17, block);
                R1(c, ref d, e, a, ref b, 18, block);
                R1(b, ref c, d, e, ref a, 19, block);
                R2(a, ref b, c, d, ref e, 20, block);
                R2(e, ref a, b, c, ref d, 21, block);
                R2(d, ref e, a, b, ref c, 22, block);
                R2(c, ref d, e, a, ref b, 23, block);
                R2(b, ref c, d, e, ref a, 24, block);
                R2(a, ref b, c, d, ref e, 25, block);
                R2(e, ref a, b, c, ref d, 26, block);
                R2(d, ref e, a, b, ref c, 27, block);
                R2(c, ref d, e, a, ref b, 28, block);
                R2(b, ref c, d, e, ref a, 29, block);
                R2(a, ref b, c, d, ref e, 30, block);
                R2(e, ref a, b, c, ref d, 31, block);
                R2(d, ref e, a, b, ref c, 32, block);
                R2(c, ref d, e, a, ref b, 33, block);
                R2(b, ref c, d, e, ref a, 34, block);
                R2(a, ref b, c, d, ref e, 35, block);
                R2(e, ref a, b, c, ref d, 36, block);
                R2(d, ref e, a, b, ref c, 37, block);
                R2(c, ref d, e, a, ref b, 38, block);
                R2(b, ref c, d, e, ref a, 39, block);
                R3(a, ref b, c, d, ref e, 40, block);
                R3(e, ref a, b, c, ref d, 41, block);
                R3(d, ref e, a, b, ref c, 42, block);
                R3(c, ref d, e, a, ref b, 43, block);
                R3(b, ref c, d, e, ref a, 44, block);
                R3(a, ref b, c, d, ref e, 45, block);
                R3(e, ref a, b, c, ref d, 46, block);
                R3(d, ref e, a, b, ref c, 47, block);
                R3(c, ref d, e, a, ref b, 48, block);
                R3(b, ref c, d, e, ref a, 49, block);
                R3(a, ref b, c, d, ref e, 50, block);
                R3(e, ref a, b, c, ref d, 51, block);
                R3(d, ref e, a, b, ref c, 52, block);
                R3(c, ref d, e, a, ref b, 53, block);
                R3(b, ref c, d, e, ref a, 54, block);
                R3(a, ref b, c, d, ref e, 55, block);
                R3(e, ref a, b, c, ref d, 56, block);
                R3(d, ref e, a, b, ref c, 57, block);
                R3(c, ref d, e, a, ref b, 58, block);
                R3(b, ref c, d, e, ref a, 59, block);
                R4(a, ref b, c, d, ref e, 60, block);
                R4(e, ref a, b, c, ref d, 61, block);
                R4(d, ref e, a, b, ref c, 62, block);
                R4(c, ref d, e, a, ref b, 63, block);
                R4(b, ref c, d, e, ref a, 64, block);
                R4(a, ref b, c, d, ref e, 65, block);
                R4(e, ref a, b, c, ref d, 66, block);
                R4(d, ref e, a, b, ref c, 67, block);
                R4(c, ref d, e, a, ref b, 68, block);
                R4(b, ref c, d, e, ref a, 69, block);
                R4(a, ref b, c, d, ref e, 70, block);
                R4(e, ref a, b, c, ref d, 71, block);
                R4(d, ref e, a, b, ref c, 72, block);
                R4(c, ref d, e, a, ref b, 73, block);
                R4(b, ref c, d, e, ref a, 74, block);
                R4(a, ref b, c, d, ref e, 75, block);
                R4(e, ref a, b, c, ref d, 76, block);
                R4(d, ref e, a, b, ref c, 77, block);
                R4(c, ref d, e, a, ref b, 78, block);
                R4(b, ref c, d, e, ref a, 79, block);
                /* Add the working vars back into context.state[] */
                state[0] += a;
                state[1] += b;
                state[2] += c;
                state[3] += d;
                state[4] += e;
            }

            public static Hash160 Finalize(SHA1Context context)
            {
                int i;
                var finalcount = new byte[8];
                var c = new byte[1];

                for (i = 0; i < 8; i++)
                {
                    finalcount[i] = (byte)((context.Count[(i >= 4 ? 0 : 1)] >> ((3 - (i & 3)) * 8)) & 255);      /* Endian independent */
                }
                c[0] = 0x80;
                Update(context, c, 0, 1);
                while ((context.Count[0] & 504) != 448)
                {
                    c[0] = 0x00;
                    Update(context, c, 0, 1);
                }
                Update(context, finalcount, 0, 8); /* Should cause a SHA1Transform() */
                var hash = new byte[20];
                for (i = 0; i < 20; i++)
                {
                    hash[i] = (byte)((context.State[i >> 2] >> ((3 - (i & 3)) * 8)) & 255);
                }
                return Hash160.ConstructTake(hash);
            }
        }

        public static Hash160 Compute(byte[] data)
        {
            return Compute(data, 0, data.Length);
        }
        public static Hash160 Compute(byte[] data, int index, int length)
        {
            SHA1Context ctx = new();
            ctx.Init();
            SHA1Imp.Update(ctx, data, index, length);
            return SHA1Imp.Finalize(ctx);
        }

        #region UnitTest
        public static Hash160 Compute_ASCII(string str)
        {
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(str);
            return Compute(data);
        }

        public static Hash160 Compute_UTF8(string str)
        {
            byte[] data = System.Text.UTF8Encoding.Default.GetBytes(str);
            return Compute(data);
        }

        public static bool UnitTest()
        {
            string q = "abc";
            string qq = "this is the time for all good men to come to the aid of their country...";
            string qqq = ".. this is the time for all good men to come to the aid of their country...";
            string qqqq = "... this is the time for all good men to come to the aid of their country...";

            Hash160 qh = Compute_ASCII(q);
            Hash160 qqh = Compute_ASCII(qq);
            Hash160 qqqh = Compute_ASCII(qqq);
            Hash160 qqqqh = Compute_ASCII(qqqq);

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