using System.Globalization;
using System;

namespace Core
{
    /// <summary>
    /// A unique ID
    /// </summary>
    public struct GUID
    {
        private UInt64 mHigh;
        private UInt64 mLow;

        public static readonly GUID Empty = new GUID(0, 0);

        public GUID(UInt64 h, UInt64 l)
        {
            mHigh = h;
            mLow = l;
        }

        public override Int32 GetHashCode()
        {
            return mLow.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            GUID x = (GUID)obj;
            return mHigh == x.mHigh && mLow == x.mLow;
        }

        public static bool Validate(string str)
        {
            if (str.Length == 32)
            {
                if (StringTools.IsHexadecimalNumber(str, false))
                    return true;
            }
            return false;
        }

        public static GUID FromString(string str)
        {
            if (Validate(str))
            {
                string hs = str.Substring(0, 16);
                string ls = str.Substring(16, 16);
                UInt64 h;
                if (UInt64.TryParse(hs, NumberStyles.HexNumber, null, out h))
                {
                    UInt64 l;
                    if (UInt64.TryParse(ls, NumberStyles.HexNumber, null, out l))
                    {
                        return new GUID(h, l);
                    }
                }
            }

            return Empty;
        }

        public override string ToString()
        {
            return String.Format("{0:X16}{1:X16}", mHigh, mLow);
        }

        public UInt64 High { get { return mHigh; } set { mHigh = value; } }
        public UInt64 Low { get { return mLow; } set { mLow = value; } }

        public bool IsEqualTo(GUID b)
        {
            return mHigh == b.mHigh && mLow == b.mLow;
        }

        public static bool operator ==(GUID a, GUID b) { return a.High == b.High && a.Low == b.Low; }
        public static bool operator !=(GUID a, GUID b) { return a.High != b.High || a.Low != b.Low; }

        public void Write(IBinaryWriter writer)
        {
            byte[] h = BitConverter.GetBytes(mHigh);
            writer.Write(h);
            byte[] l = BitConverter.GetBytes(mLow);
            writer.Write(l);
        }
    }

}
