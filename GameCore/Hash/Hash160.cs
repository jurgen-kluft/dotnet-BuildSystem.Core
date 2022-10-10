using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    public struct Hash160
    {
        public static readonly byte[] hash_null_ = new byte[20]
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
        public static readonly byte[] hash_error_ = new byte[20]
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

        public static Hash160 Null
        {
            get { return new Hash160(hash_null_, 0); }
        }
        public static Hash160 Empty
        {
            get { return new Hash160(hash_null_, 0); }
        }
        public static Hash160 Error
        {
            get { return new Hash160(hash_error_, 0); }
        }

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

        private Hash160(byte[] _hash)
        {
            hash_ = _hash;
        }

        public Hash160()
        {
            hash_ = new byte[Size];
            CopyFrom(hash_null_, 0);
        }

        private Hash160(Hash160 _other)
        {
            hash_ = new byte[Size];
            CopyFrom(_other.Data, 0);
        }

        private Hash160(byte[] _array, int _start)
        {
            hash_ = new byte[Size];
            CopyFrom(_array, _start);
        }

        public static readonly int Size = 20;

        public static Hash160 ConstructTake(byte[] _hash)
        {
            return new Hash160(_hash);
        }

        public static Hash160 ConstructCopy(byte[] _hash)
        {
            return new Hash160(_hash, 0);
        }

        public static Hash160 ConstructCopy(byte[] _hash, int start)
        {
            return new Hash160(_hash, start);
        }

        public byte[] Data
        {
            get { return hash_; }
        }

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
                chars[i + 1] = (char)((bl < 10) ? ('0' + bl) : ('A' + bl - 10));
            }
            string str = new string(chars);
            while (str.Length < (Size*2))
                str = "0" + str;
            return str;
        }

        private static byte[] FromStringN(string _hashstr, int _size_in_bytes)
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

        public static Hash160 FromString(string _hashstr)
        {
            return ConstructTake(FromStringN(_hashstr, Size));
        }

        public static Hash160 FromDateTime(DateTime dt)
        {
            return FromString(String.Format("00000000000000{0:X16}", dt.Ticks));
        }

        public int Copy(Hash160 other)
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

        public void WriteTo(IBinaryWriter _writer)
        {
            _writer.Write(hash_, 0, Size);
        }

        public static Hash160 ReadFrom(IBinaryReader _reader)
        {
            byte[] hash = _reader.ReadBytes(Size);
            return ConstructTake(hash);
        }

        public static bool operator ==(Hash160 b1, Hash160 b2)
        {
            bool equal = Equals(b1.hash_, 0, b2.hash_, 0);
            return equal;
        }

        public static bool operator !=(Hash160 b1, Hash160 b2)
        {
            bool equal = Equals(b1.hash_, 0, b2.hash_, 0);
            return equal == false;
        }

        public override bool Equals(object obj)
        {
            Hash160 _other = (Hash160)obj;
            return Equals(this.hash_, 0, _other.hash_, 0);
        }

        public int Compare(Hash160 _other)
        {
            return Compare(this.hash_, 0, _other.hash_, 0);
        }

        public static int Compare(Hash160 a, Hash160 b)
        {
            return Compare(a.hash_, 0, b.hash_, 0);
        }

        private static bool Equals(byte[] _this, int _thisstart, byte[] _other, int _otherstart)
        {
            for (int j = 0; j < Size; j++)
            {
                byte m = _this[_thisstart++];
                byte o = _other[_otherstart++];
                if (m != o)
                    return false;
            }
            return true;
        }

        private static int Compare(byte[] _this, int _thisstart, byte[] _other, int _otherstart)
        {
            for (int j = 0; j < Size; j++)
            {
                byte m = _this[_thisstart++];
                byte o = _other[_otherstart++];
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

    static public class HashUtility
    {
        #region Methods

        public static Hash160 Compute(byte[] data)
        {
            return SHA1.Compute(data);
        }
        public static Hash160 Compute(byte[] data, int start, int length)
        {
            return SHA1.Compute(data, start, length);
        }

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

        public static Hash160 compute(FileInfo s)
        {
            if (!s.Exists)
                return Hash160.Null;

            try
            {
                SHA1.SHA1Hash Sha1Hasher = new();
                Sha1Hasher.Init();
                byte[] block = new byte[256 * 1024];

                using (
                    FileStream fs = new(s.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
                )
                {
                    Int64 total = fs.Length;
                    Int64 offset = 0;
                    int stride = (256 * 1024);
                    while (offset < total)
                    {
                        if ((offset + stride) > total)
                        {
                            stride = (int)(total - offset);
                        }
                        fs.Read(block, 0, stride);
                        offset += stride;
                        Sha1Hasher.Update(block, 0, stride);
                    }
                    return Sha1Hasher.Finalize();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[HashUtility:EXCEPTION]{0}", e);
                return Hash160.Null;
            }
        }

        public static Hash160 compute(MemoryStream ms)
        {
            byte[] data = ms.GetBuffer();
            return SHA1.Compute(data);
        }

        public static Hash160 compute(bool[] values)
        {
            MemoryStream ms = new MemoryStream();
            foreach (var v in values)
            {
                ms.WriteByte(v ? (byte)1 : (byte)0);
            }
            return compute(ms);
        }

        public static Hash160 compute(byte[] v)
        {
            return SHA1.Compute(v, 0, v.Length);
        }

        public static Hash160 compute(byte[] v1, int v1Length, byte[] v2, int v2Length)
        {
            SHA1.SHA1Hash Sha1Hasher = new();
            Sha1Hasher.Init();
            Sha1Hasher.Compute(v1, 0, v1Length);
            Sha1Hasher.Compute(v2, 0, v2Length);
            return Sha1Hasher.Finalize();
        }

        public static Hash160 compute(byte[] v, int index, int count)
        {
            return SHA1.Compute(v, index, count);
        }

        public static Hash160 compute(sbyte[] v)
        {
            return SHA1.Compute((byte[])(Array)v, 0, v.Length);
        }

        public static Hash160 compute(sbyte[] v, int index, int count)
        {
            return SHA1.Compute((byte[])(Array)v, index, count);
        }

        public static Hash160 compute(short[] values)
        {
            byte[] bytes = new byte[2*values.Length];
            int count = 0;
            for (int j=0; j<values.Length; j++)
            {
                short v = values[j];
                for (int i = 0; i < 2; ++i)
                {
                    bytes[count++] = (byte)(v);
                    v = (short)(v >> 8);
                }
            }
            return compute(bytes);
        }

        public static Hash160 compute(ushort[] values)
        {
            byte[] bytes = new byte[2*values.Length];
            int count = 0;
            for (int j=0; j<values.Length; j++)
            {
                ushort v = values[j];
                for (int i = 0; i < 2; ++i)
                {
                    bytes[count++] = (byte)(v);
                    v = (ushort)(v >> 8);
                }
            }
            return compute(bytes);
        }

        public static Hash160 compute(int[] values)
        {
            byte[] bytes = new byte[4*values.Length];
            int count = 0;
            for (int j=0; j<values.Length; j++)
            {
                int v = values[j];
                for (int i = 0; i < 4; ++i)
                {
                    bytes[count++] = (byte)(v);
                    v = (int)(v >> 8);
                }
            }
            return compute(bytes);
        }

        public static Hash160 compute(uint[] values)
        {
            byte[] bytes = new byte[4*values.Length];
            int count = 0;
            for (int j=0; j<values.Length; j++)
            {
                uint v = values[j];
                for (int i = 0; i < 4; ++i)
                {
                    bytes[count++] = (byte)(v);
                    v = (uint)(v >> 8);
                }
            }
            return compute(bytes);
        }

        public static Hash160 compute(Int64[] values)
        {
            byte[] bytes = new byte[8*values.Length];
            int count = 0;
            for (int j=0; j<values.Length; j++)
            {
                Int64 v = values[j];
                for (int i = 0; i < 8; ++i)
                {
                    bytes[count++] = (byte)(v);
                    v = (Int64)(v >> 8);
                }
            }
            return compute(bytes);
        }

        public static Hash160 compute(UInt64[] values)
        {
            byte[] bytes = new byte[8*values.Length];
            int count = 0;
            for (int j=0; j<values.Length; j++)
            {
                UInt64 v = values[j];
                for (int i = 0; i < 8; ++i)
                {
                    bytes[count++] = (byte)(v);
                    v = (UInt64)(v >> 8);
                }
            }
            return compute(bytes);
        }

        public static Hash160 compute(float[] values)
        {
            byte[] bytes = new byte[4*values.Length];
            int count = 0;
            for (int j=0; j<values.Length; j++)
            {
                float v = values[j];
                byte[] vb = BitConverter.GetBytes(v);
                for (int i = 0; i < 4; ++i)
                {
                    bytes[count++] = vb[i];
                }
            }
            return compute(bytes);
        }

        #endregion
    }
}
