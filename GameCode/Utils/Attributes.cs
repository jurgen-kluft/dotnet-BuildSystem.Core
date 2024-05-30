using System;
using Int8 = System.SByte;
using UInt8 = System.Byte;

namespace GameData
{
    #region Comment

    public class Comment : Attribute
    {
        private readonly string mComment;

        public Comment(string comment)
        {
            mComment = comment;
        }
    }

    #endregion

    #region InPlace

    public class ArrayElementsInPlace : Attribute
    {
    }

    #endregion

    #region Range

    public abstract class Range : Attribute
    {
        public virtual bool Check(Int8 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(UInt8 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(short c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(ushort c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(int c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(uint c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(long c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(ulong c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(float c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(double c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool Check(string c, out string error)
        {
            error = string.Empty;
            return true;
        }
    }

    #endregion
    #region Integer Range

    public class IntRange : Range
    {
        protected int Min;
        protected int Max;

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        private bool Error(string valueStr, out string errorStr)
        {
            errorStr = string.Format("Value {0} out of range [{1},{2}]", valueStr, Min, Max);
            return false;
        }

        public override bool Check(Int8 c, out string errorStr)
        {
            int v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(UInt8 c, out string errorStr)
        {
            int v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(short c, out string errorStr)
        {
            int v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(ushort c, out string errorStr)
        {
            int v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(int c, out string errorStr)
        {
            var v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(uint c, out string errorStr)
        {
            var v = (int)c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(long c, out string errorStr)
        {
            var v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(ulong c, out string errorStr)
        {
            var v = (long)c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(float c, out string errorStr)
        {
            var v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(double c, out string errorStr)
        {
            var v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
    }

    #endregion
    #region Float Range

    public class FloatRange : Range
    {
        protected double Min;
        protected double Max;

        public FloatRange(double min, double max)
        {
            Min = min;
            Max = max;
        }

        private bool Error(string valueStr, out string errorStr)
        {
            errorStr = string.Format("Value {0} out of range [{1},{2}]", valueStr, Min, Max);
            return false;
        }

        public override bool Check(Int8 c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(UInt8 c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(short c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(ushort c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(int c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(uint c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(long c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(ulong c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(float c, out string errorStr)
        {
            double v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(double c, out string errorStr)
        {
            var v = c;
            if (v < Min || v > Max)
                return Error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
    }

    #endregion
    #region Fx16 Range

    public class Fx16Range : Range
    {
        private const double SMin = -1.0 * ((double)((7 * (1 << 12) + ((1 << 12) - 1))) / (double)(1 << 12));
        private const double SMax = ((double)((7 * (1 << 12) + ((1 << 12) - 1))) / (double)(1 << 12));

        private readonly double mMin = double.MinValue;
        private readonly double mMax = double.MaxValue;

        public Fx16Range(double min, double max)
        {
            mMin = min;
            mMax = max;

            // Fx16: -7.99 to +7.99
            if (mMin < SMin)
                mMin = SMin;
            if (mMax > SMax)
                mMax = SMax;
        }

        private bool Error(string valueStr, out string errorStr)
        {
            errorStr = string.Format("Value {0} out of range [{1},{2}]", valueStr, mMin, mMax);
            return false;
        }

        public override bool Check(Int8 c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(UInt8 c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(short c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(ushort c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(int c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(uint c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(long c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(ulong c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(float c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(double c, out string errorStr)
        {
            var v = (c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
    }

    #endregion
    #region Fx32 Range

    public class Fx32Range : Range
    {
        private readonly double mMin = double.MinValue;
        private readonly double mMax = double.MaxValue;

        public Fx32Range(double min, double max)
        {
            mMin = min;
            mMax = max;
        }

        private bool Error(string valueStr, out string errorStr)
        {
            errorStr = string.Format("Value {0} out of range [{1},{2}]", valueStr, mMin, mMax);
            return false;
        }

        public override bool Check(Int8 c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(UInt8 c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(short c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(ushort c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(int c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(uint c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(long c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(ulong c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(float c, out string errorStr)
        {
            var v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool Check(double c, out string errorStr)
        {
            var v = (c) / (1 << 12);
            if (v < mMin || v > mMax)
                return Error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
    }

    #endregion

    #region Export C++ enum

    public class ExportAsEnumForCpp : Attribute
    {
        private readonly string mFilename;

        public ExportAsEnumForCpp(string filename)
        {
            mFilename = filename;
        }

        public string filename
        {
            get
            {
                return mFilename;
            }
        }
    }

    #endregion
}
