using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Core
{
    public class Hash128
    {
        public static readonly byte[] hash_null_ = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] hash_error_ = new byte[16] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        public static Hash128 Null { get { return new Hash128(hash_null_, 0); } }
        public static Hash128 Empty { get { return new Hash128(hash_null_, 0); } }
        public static Hash128 Error { get { return new Hash128(hash_error_, 0); } }

        public bool IsErrorHash()
        {
            bool equal = (hash_[0] == hash_error_[0]);
            for (int j = 1; j < Size && equal; j++)
                equal = (hash_[j] == hash_error_[j]);
            return equal;
        }

        public bool IsNullHash()
        {
            bool equal = (hash_[0] == hash_null_[0]);
            for (int j = 1; j < Size && equal; j++)
                equal = (hash_[j] == hash_null_[j]);
            return equal;
        }

        private byte[] hash_;
        private Hash128(byte[] _hash)
        {
            hash_ = _hash;
        }

        public Hash128()
        {
            hash_ = new byte[Size];
            CopyFrom(hash_null_, 0);
        }

        public Hash128(Hash128 _other)
        {
            hash_ = new byte[Size];
            CopyFrom(_other.Data, 0);
        }

        private Hash128(byte[] _array, int _start)
        {
            hash_ = new byte[Size];
            CopyFrom(_array, _start);
        }

        public static readonly int Size = 16;

        public static Hash128 ConstructTake(byte[] _hash)
        {
            return new Hash128(_hash);
        }
        public static Hash128 ConstructCopy(byte[] _hash)
        {
            return new Hash128(_hash, 0);
        }
        public static Hash128 ConstructCopy(byte[] _hash, int start)
        {
            return new Hash128(_hash, start);
        }

        public byte[] Data { get { return hash_; } }

        public byte[] Release()
        {
            byte[] h = hash_;
            hash_ = new byte[Size];
            CopyFrom(hash_null_, 0);
            return h;
        }
        public override int GetHashCode()
        {
            Int32 hashcode = BitConverter.ToInt32(hash_, Size - 4);
            return hashcode;
        }

        public override string ToString()
        {
            int length = hash_.Length;
            char[] chars = new char[length * 2];
            for (int n = 0; n < length; n++)
            {
                int i = n * 2;
                byte value = hash_[n];
                byte bh = (byte)(value >> 4);
                chars[i] = (char)((bh < 10) ? ('0' + bh) : ('A' + bh - 10));
                byte bl = (byte)(value & 0xF);
                chars[i+1] = (char)((bl < 10) ? ('0' + bl) : ('A' + bl - 10));
            }
            string str = new string(chars);
            while (str.Length < 32)
                str = "0" + str;
            return str;
        }

        public static byte[] FromStringN(string _hashstr, int _size_in_bytes)
        {
            byte[] hash = new byte[_size_in_bytes];

            int nc = 2 * _size_in_bytes;
            string str = _hashstr;
            while (str.Length < nc)
                str = "0" + str;

            for (int i = 0; i < nc; i += 2)
            {
                int b = 0;
                for (int j = 0; j < 2; ++j)
                {
                    char c = str[i + j];
                    Debug.Assert(Char.IsLetterOrDigit(c) && !Char.IsLower(c));

                    int n = 0;
                    if (c >= 'A' && c <= 'F')
                        n = (byte)((int)10 + ((int)c - (int)'A'));
                    else if (c >= 'a' && c <= 'f')
                        n = (byte)((int)10 + ((int)c - (int)'a'));
                    else if (c >= '0' && c <= '9')
                        n = (byte)((int)0 + ((int)c - (int)'0'));

                    Debug.Assert(n >= 0 && n <= 15);
                    b = (byte)((b << 4) | n);
                }

                hash[i / 2] = (byte)b;
            }
            return hash;
        }

        public static Hash128 FromString(string _hashstr)
        {
            return ConstructTake(FromStringN(_hashstr, 32));
        }

        public static Hash128 FromDateTime(DateTime dt)
        {
            return FromString(String.Format("00000000000000{0:X16}", dt.Ticks));
        }

        public int Copy(Hash128 other)
        {
            return CopyFrom(other.Data, 0);
        }

        public int CopyFrom(byte[] _hash, int _offset)
        {
            for (int j = 0; j < Size; j++)
                hash_[j] = _hash[_offset + j];
            return Size;
        }

        public int CopyTo(byte[] _header)
        {
            return CopyTo(_header, 0);
        }

        public int CopyTo(byte[] _header, int _index)
        {
            for (int j = 0; j < Size; j++)
                _header[j + _index] = hash_[j];
            return Size;
        }

        public void WriteTo(BinaryWriter _writer)
        {
            _writer.Write(hash_);
        }

        public static bool operator ==(Hash128 b1, Hash128 b2)
        {
            bool equal = (b1.hash_[0] == b2.hash_[0]);
            for (int j = 1; j < Size && equal; j++)
                equal = (b1.hash_[j] == b2.hash_[j]);
            return equal;
        }
        public static bool operator !=(Hash128 b1, Hash128 b2)
        {
            bool equal = (b1.hash_[0] == b2.hash_[0]);
            for (int j = 1; j < Size && equal; j++)
                equal = (b1.hash_[j] == b2.hash_[j]);
            return equal == false;
        }

        public override bool Equals(object obj)
        {
            Hash128 h = (Hash128)obj;
            return Compare(h) == 0;
        }

        public int Compare(Hash128 _other)
        {
            return Compare(_other.Data, 0);
        }
        public static int Compare(Hash128 a, Hash128 b)
        {
            return a.Compare(b);
        }

        public int Compare(byte[] _other, int _start)
        {
            for (int j = 0; j < Size; j++)
            {
                byte m = hash_[j];
                byte o = _other[_start + j];
                if (m < o) return -1;
                else if (m > o) return 1;
            }
            return 0;
        }

        #region Comparer (IEqualityComparer)

        public class Comparer : IEqualityComparer<Hash128>
        {
            public bool Equals(Hash128 a, Hash128 b)
            {
                return Hash128.Compare(a, b) == 0;
            }

            public int GetHashCode(Hash128 r)
            {
                return r.GetHashCode();
            }
        }

        #endregion

        public static void UnitTest()
        {
            Hash128 h1 = new Hash128();
            Hash128 h2 = new Hash128();

            Hash128 h3 = new Hash128(Hash128.Error);
            Hash128 h4 = new Hash128(Hash128.Error);

            Debug.Assert(h1 == Hash128.Null);
            Debug.Assert(h2 == Hash128.Null);
            Debug.Assert(h1 != Hash128.Error);
            Debug.Assert(h2 != Hash128.Error);
            Debug.Assert(h1 == h2);
            Debug.Assert(h1.Compare(h2) == 0);
            Debug.Assert(h1.GetHashCode() == h2.GetHashCode());

            Debug.Assert(h3 != Hash128.Null);
            Debug.Assert(h4 != Hash128.Null);
            Debug.Assert(h3 == Hash128.Error);
            Debug.Assert(h4 == Hash128.Error);
            Debug.Assert(h3 == h4);
            Debug.Assert(h3.Compare(h4) == 0);
            Debug.Assert(h3.GetHashCode() == h4.GetHashCode());

            Debug.Assert(h1 != h3);
            Debug.Assert(h2 != h4);

            Debug.Assert(Hash128.Error != Hash128.Null);
            Debug.Assert(Hash128.Error.Compare(Hash128.Null) == 1);
            Debug.Assert(Hash128.Null.Compare(Hash128.Error) == -1);
            Debug.Assert(Hash128.Error.GetHashCode() != Hash128.Null.GetHashCode());
        }
    };

    static public class HashUtility
    {
        #region Methods

        public static Hash128 compute(string str)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(str);
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(data);

            return Hash128.ConstructTake(md5);
        }

        public static Hash128 compute(FileInfo s)
        {
            if (!s.Exists)
                return Hash128.Null;

            try
            {
                using (FileStream stream = new FileStream(s.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(stream);
                    stream.Close();
                    return Hash128.ConstructTake(md5);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[HashUtility:EXCEPTION]{0}", e);
                return Hash128.Null;
            }
        }
        public static Hash128 compute(FileStream s)
        {
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(s);
            return Hash128.ConstructTake(md5);
        }
        public static Hash128 compute(MemoryStream s)
        {
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(s);
            return Hash128.ConstructTake(md5);
        }
        public static Hash128 compute(bool[] v)
        {
            byte[] buffer = new byte[v.Length];
            for (int i = 0; i < v.Length; i++)
                buffer[i] = v[i] ? (byte)1 : (byte)0;
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(buffer);
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
        }
        public static Hash128 compute(byte[] v, int index, int count)
        {
            byte[] md5 = System.Security.Cryptography.MD5.Create().ComputeHash(v, index, count);
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
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
            return Hash128.ConstructTake(md5);
        }

        #endregion
    }
}
