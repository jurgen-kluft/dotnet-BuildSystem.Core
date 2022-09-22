using System;

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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  */
    /* SHA-1 (FIPS 180-4) implementation                                                              */
    /*                                                                                   MIT Licence  */
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  */
    public static class SHA1
    {
        private static readonly UInt32[] K = new uint[4] { 0x5a827999, 0x6ed9eba1, 0x8f1bbcdc, 0xca62c1d6 };

        private static uint ReadUInt32(byte[] msg, uint offset)
        {
            uint u = msg[offset];
            for (uint k = 1; k < 4; k++)
            {
                u = (u << 8) | msg[offset + k];
            }
            return u;
        }
        private static void WriteUInt32(byte[] msg, uint offset, uint value)
        {
            msg[offset + 0] = (byte)(value >> 24);
            msg[offset + 1] = (byte)(value >> 16);
            msg[offset + 2] = (byte)(value >> 8);
            msg[offset + 3] = (byte)(value >> 0);
        }

        private static void Iterate(byte[] msg, uint msg_offset, uint[] W, uint[] H)
        {
            // 1 - prepare message schedule 'W'
            for (uint j = 0; j < 16; j++)
            {
                W[j] = ReadUInt32(msg, msg_offset + (j * 4));
            }

            for (uint t = 16; t < 80; t++)
            {
                W[t] = SHA1.ROTL(W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16], 1);
            }

            // 2 - initialise five working variables a, b, c, d, e with previous hash value
            uint a = H[0], b = H[1], c = H[2], d = H[3], e = H[4];

            // 3 - main loop
            for (uint s = 0; s < 4; s++)
            {
                for (uint t = 0; t < 20; t++)
                {
                    uint aa = (SHA1.ROTL(a, 5) + SHA1.F(s, b, c, d) + e + K[s] + W[t + (s * 20)]);
                    e = d;
                    d = c;
                    c = SHA1.ROTL(b, 30);
                    b = a;
                    a = aa;
                }
            }

            // 4 - compute the new intermediate hash value
            H[0] = (H[0] + a);
            H[1] = (H[1] + b);
            H[2] = (H[2] + c);
            H[3] = (H[3] + d);
            H[4] = (H[4] + e);
        }

        public static Hash128 Compute(byte[] msg)
        {
            UInt32[] H = new UInt32[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476, 0xC3D2E1F0 };
            uint[] W = new uint[80];
            byte[] Block = new byte[64];

            uint L = (uint)msg.Length;
            uint N = L / 64;
            for (uint i = 0; i < N; i++)
            {
                Iterate(msg, i * 64, W, H);
            }

            L = (uint)(L + 1) - (N * 64) + 8; // +8 is the bytes used that contain the length in bits of the message
            while (L != 0)
            {
                for (uint i = 0; i < 64; i++)
                {
                    uint msg_index = (N * 64) + i;
                    if (msg_index < msg.Length)
                    {
                        Block[i] = msg[msg_index];
                    }
                    else if (msg_index == msg.Length)
                    {
                        Block[i++] = 0x80;
                        for (; i < 64; i++)
                            Block[i] = 0;
                    }
                    else
                    {
                        for (; i < 64; i++)
                            Block[i] = 0;
                    }
                }

                if (L <= 64)
                {
                    UInt64 bitlen = (UInt64)msg.Length * 8;
                    WriteUInt32(Block, 56, (uint)(bitlen >> 32));
                    WriteUInt32(Block, 60, (uint)(bitlen & 0xffffffff));
                    L = 0;
                }
                else
                {
                    L -= 64;
                }

                Iterate(Block, 0, W, H);
            }

            byte[] data = new byte[16];
            for (uint i = 0; i < 4; ++i)
            {
                WriteUInt32(data, i * 4, H[i]);
            }

            return Hash128.ConstructTake(data);
        }

        static uint F(uint s, uint x, uint y, uint z)
        {
            switch (s)
            {
                case 0: return (x & y) ^ (~x & z);          // Ch()
                case 1: return x ^ y ^ z;                   // Parity()
                case 2: return (x & y) ^ (x & z) ^ (y & z); // Maj()
                case 3: return x ^ y ^ z;                   // Parity()
            }
            return 0;
        }

        static uint ROTL(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        #region UnitTest
        public static Hash128 Compute_ASCII(string str)
        {
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(str);
            return Compute(data);
        }

        public static bool UnitTest()
        {
            string q = "abc";
            string qq = "this is the time for all good men to come to the aid of their country...";
            string qqq = ".. this is the time for all good men to come to the aid of their country...";
            string qqqq = "... this is the time for all good men to come to the aid of their country...";

            Hash128 qh = Compute_ASCII(q);
            Hash128 qqh = Compute_ASCII(qq);
            Hash128 qqqh = Compute_ASCII(qqq);
            Hash128 qqqqh = Compute_ASCII(qqqq);

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