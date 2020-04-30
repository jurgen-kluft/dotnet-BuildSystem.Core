using System;

namespace Core
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
                foreach(byte b in data)
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

            uint len4 =  (uint)(str.Length / 4);
            for (int i = 0; i < len4; i++)
            {
                byte b1 = (byte)str[i];
                byte b2 = (byte)str[i+1];
                byte b3 = (byte)str[i+2];
                byte b4 = (byte)str[i+3];

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
}
