using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Core
{
    public struct Hash128
    {
        #region Fields

        private readonly static string sEmptyHashString = "00000000000000000000000000000000";
        public readonly static Hash128 Empty = new Hash128(sEmptyHashString);
        private const string sFormat = "{0:X16}{1:X16}";

        private string mHash;

        #endregion
        #region Constructor

        Hash128(string hash)
        {
            if (String.IsNullOrEmpty(hash))
                mHash = sEmptyHashString;
            else
                mHash = hash.ToUpper();
        }

        public Hash128(Hash128 other)
        {
            Debug.Assert(!String.IsNullOrEmpty(other.mHash));
            mHash = other.mHash;
        }

        #endregion
        #region Operators

        public static bool operator ==(Hash128 a, Hash128 b)
        {
            Debug.Assert((object)a != null && (object)b != null);               // Never compare to null, create a MD5.Empty object as default!
            int c = String.Compare(a.mHash, b.mHash);
            return c == 0;
        }
        public static bool operator !=(Hash128 a, Hash128 b)
        {
            Debug.Assert((object)a != null && (object)b != null);               // Never compare to null, create a MD5.Empty object as default!
            int c = String.Compare(a.mHash, b.mHash);
            return c != 0;
        }
        public static bool operator <(Hash128 a, Hash128 b)
        {
            Debug.Assert((object)a != null && (object)b != null);               // Never compare to null, create a MD5.Empty object as default!
            int c = String.Compare(a.mHash, b.mHash);
            return c == -1;
        }
        public static bool operator <=(Hash128 a, Hash128 b)
        {
            Debug.Assert((object)a != null && (object)b != null);               // Never compare to null, create a MD5.Empty object as default!
            int c = String.Compare(a.mHash, b.mHash);
            return c==0 || c==-1;
        }
        public static bool operator >(Hash128 a, Hash128 b)
        {
            Debug.Assert((object)a != null && (object)b != null);               // Never compare to null, create a MD5.Empty object as default!
            int c = String.Compare(a.mHash, b.mHash);
            return c == 1;
        }
        public static bool operator >=(Hash128 a, Hash128 b)
        {
            Debug.Assert((object)a != null && (object)b != null);               // Never compare to null, create a MD5.Empty object as default!
            int c = String.Compare(a.mHash, b.mHash);
            return c == 1 || c ==0;
        }

        #endregion
        #region Comparer (IEqualityComparer)

        public class Comparer : IEqualityComparer<Hash128>
        {
            public bool Equals(Hash128 a, Hash128 b)
            {
                return a.mHash == b.mHash;
            }

            public int GetHashCode(Hash128 r)
            {
                return r.GetHashCode();
            }
        }

        public static int Compare(Hash128 x, Hash128 y)
        {
            return String.Compare(x.mHash, y.mHash);
        }

        #endregion
        #region Object Methods

        public override bool Equals(object obj)
        {
            Hash128 h = (Hash128)obj;
            return h.mHash == mHash;
        }
        
        public override int GetHashCode()
        {
            return mHash.GetHashCode();
        }

        public override string ToString()
        {
            return mHash;
        }

        #endregion
        #region Static From/To Methods

        public static Hash128 FromLastFileWriteTime(DateTime dt)
        {
            if (dt.Ticks == 0x0701ce5a309a4000)
                return Hash128.Empty;
            return FromDateTime(dt);
        }

        public static Hash128 FromDateTime(DateTime dt)
        {
            return FromString(String.Format("00000000000000{0:X16}", dt.Ticks));
        }

        public static Hash128 FromString(string s)
        {
            if (StringTools.IsHexadecimalNumber(s, false))
            {
                Hash128 h = new Hash128(s);
                return h;
            }
            else
            {
                return Hash128.Empty;
            }
        }

        public static string ToString(Hash128 h)
        {
            return h.mHash;
        }

        public static byte[] ToBinary(Hash128 hash)
        {
            string s = hash.mHash;
            byte[] h = new byte[16];
            for (int i = 0; i < 16; ++i)
            {
                byte b = StringTools.HexToNibble(s[i * 2]);
                b = (byte)(b << 4);
                b |= StringTools.HexToNibble(s[i * 2 + 1]);
                h[i] = b;
            }
            return h;
        }

        public static Hash128 FromBinary(byte[] hash)
        {
            string str = string.Empty;
            for (int i = 0; i < 16; ++i)
            {
                byte b = hash[i];
                str += StringTools.NibbleToHex((byte)(b & 0xF));
                b = (byte)(b >> 4);
                str += StringTools.NibbleToHex((byte)(b & 0xF));
            }
            return new Hash128(str);
        }

        #endregion
    }

    static public class HashUtility
    {
        #region Methods

        public static Hash128 compute(string str)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(str);
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(data);

            return Hash128.FromBinary(md5);
        }

        public static Hash128 compute(FileInfo s)
        {
            if (!s.Exists)
                return Hash128.Empty;

            try
            {
                using (FileStream stream = new FileStream(s.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(stream);
                    stream.Close();
                    return Hash128.FromBinary(md5);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[HashUtility:EXCEPTION]{0}", e);
                return Hash128.Empty;
            }
        }
        public static Hash128 compute(FileStream s)
        {
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(s);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(MemoryStream s)
        {
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(s);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(bool[] v)
        {
            byte[] buffer = new byte[v.Length];
            for (int i = 0; i < v.Length; i++)
                buffer[i] = v[i] ? (byte)1 : (byte)0;
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(byte[] v)
        {
            return compute(v, 0, v.Length);
        }
        public static Hash128 compute(byte[] v1, int v1Length, byte[] v2, int v2Length)
        {
            System.Security.Cryptography.MD5 md5Provider = System.Security.Cryptography.MD5.Create();
            md5Provider.TransformBlock(v1, 0, v1Length, v1, 0);
            md5Provider.TransformFinalBlock(v2, 0, v2Length);
            byte[] md5 = md5Provider.Hash;
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(byte[] v, int index, int count)
        {
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(v, index, count);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(sbyte[] v)
        {
            return compute(v, 0, v.Length);
        }
        public static Hash128 compute(sbyte[] v, int index, int count)
        {
            byte[] buffer = new byte[count];
            for (int i = 0; i < count; i++)
                buffer[i] = (byte)v[index + i];
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(short[] v)
        {
            byte[] buffer = new byte[v.Length * 2];
            for (int i = 0; i < v.Length; i++)
            {
                buffer[i * 2 + 0] = (byte)(v[i] >> 8);
                buffer[i * 2 + 1] = (byte)(v[i] & 0xFF);
            }
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(ushort[] v)
        {
            byte[] buffer = new byte[v.Length * 2];
            for (int i = 0; i < v.Length; i++)
            {
                buffer[i * 2 + 0] = (byte)(v[i] >> 8);
                buffer[i * 2 + 1] = (byte)(v[i] & 0xFF);
            }
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(int[] v)
        {
            byte[] buffer = new byte[v.Length * 4];
            for (int i = 0; i < v.Length; i++)
            {
                buffer[i * 4 + 0] = (byte)(v[i] >> 24);
                buffer[i * 4 + 1] = (byte)(v[i] >> 16);
                buffer[i * 4 + 2] = (byte)(v[i] >> 8);
                buffer[i * 4 + 3] = (byte)(v[i] & 0xFF);
            }
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(uint[] v)
        {
            byte[] buffer = new byte[v.Length * 4];
            for (int i = 0; i < v.Length; i++)
            {
                buffer[i * 4 + 0] = (byte)(v[i] >> 24);
                buffer[i * 4 + 1] = (byte)(v[i] >> 16);
                buffer[i * 4 + 2] = (byte)(v[i] >> 8);
                buffer[i * 4 + 3] = (byte)(v[i] & 0xFF);
            }
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(Int64[] v)
        {
            byte[] buffer = new byte[v.Length * 8];
            for (int i = 0; i < v.Length; i++)
            {
                buffer[i * 8 + 0] = (byte)(v[i] >> 56);
                buffer[i * 8 + 1] = (byte)(v[i] >> 48);
                buffer[i * 8 + 2] = (byte)(v[i] >> 40);
                buffer[i * 8 + 3] = (byte)(v[i] >> 32);
                buffer[i * 8 + 4] = (byte)(v[i] >> 24);
                buffer[i * 8 + 5] = (byte)(v[i] >> 16);
                buffer[i * 8 + 6] = (byte)(v[i] >> 8);
                buffer[i * 8 + 7] = (byte)(v[i] & 0xFF);
            }
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(UInt64[] v)
        {
            byte[] buffer = new byte[v.Length * 8];
            for (int i = 0; i < v.Length; i++)
            {
                buffer[i * 8 + 0] = (byte)(v[i] >> 56);
                buffer[i * 8 + 1] = (byte)(v[i] >> 48);
                buffer[i * 8 + 2] = (byte)(v[i] >> 40);
                buffer[i * 8 + 3] = (byte)(v[i] >> 32);
                buffer[i * 8 + 4] = (byte)(v[i] >> 24);
                buffer[i * 8 + 5] = (byte)(v[i] >> 16);
                buffer[i * 8 + 6] = (byte)(v[i] >> 8);
                buffer[i * 8 + 7] = (byte)(v[i] & 0xFF);
            }
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }
        public static Hash128 compute(float[] v)
        {
            byte[] buffer = new byte[v.Length * 4];
            for (int i = 0; i < v.Length; i++)
            {
                byte[] floatBytes = BitConverter.GetBytes(v[i]);
                for (int j = 0; j < floatBytes.Length; j++)
                    buffer[i * 4 + j] = floatBytes[j];
            }
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.FromBinary(md5);
        }

        #endregion
    }
}
