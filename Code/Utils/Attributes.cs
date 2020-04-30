using System;
using Int8 = System.SByte;
using UInt8 = System.Byte;

namespace Game.Data
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

    #region Range

    public abstract class Range : Attribute
    {
        public virtual bool check(Int8 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(UInt8 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(Int16 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(UInt16 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(Int32 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(UInt32 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(Int64 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(UInt64 c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(float c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(double c, out string error)
        {
            error = string.Empty;
            return true;
        }
        public virtual bool check(string c, out string error)
        {
            error = string.Empty;
            return true;
        }
    }

    #endregion
    #region Integer Range

    public class IntRange : Range
    {
        protected int mMin;
        protected int mMax;

        public IntRange(int min, int max)
        {
            mMin = min;
            mMax = max;
        }

        private bool error(string valueStr, out string errorStr)
        {
            errorStr = String.Format("Value {0} out of range [{1},{2}]", valueStr, mMin, mMax);
            return false;
        }

        public override bool check(Int8 c, out string errorStr)
        {
            int v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt8 c, out string errorStr)
        {
            int v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int16 c, out string errorStr)
        {
            int v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt16 c, out string errorStr)
        {
            int v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int32 c, out string errorStr)
        {
            int v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt32 c, out string errorStr)
        {
            int v = (int)c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int64 c, out string errorStr)
        {
            Int64 v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt64 c, out string errorStr)
        {
            Int64 v = (Int64)c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(float c, out string errorStr)
        {
            float v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(double c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
    }

    #endregion
    #region Float Range

    public class FloatRange : Range
    {
        protected double mMin;
        protected double mMax;

        public FloatRange(double min, double max)
        {
            mMin = min;
            mMax = max;
        }

        private bool error(string valueStr, out string errorStr)
        {
            errorStr = String.Format("Value {0} out of range [{1},{2}]", valueStr, mMin, mMax);
            return false;
        }

        public override bool check(Int8 c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt8 c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int16 c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt16 c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int32 c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt32 c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int64 c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt64 c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(float c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(double c, out string errorStr)
        {
            double v = c;
            if (v < mMin || v > mMax)
                return error(c.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
    }

    #endregion
    #region Fx16 Range

    public class Fx16Range : Range
    {
        private const double sMin = -1.0 * ((double)((7 * (1 << 12) + ((1 << 12) - 1))) / (double)(1 << 12));
        private const double sMax = ((double)((7 * (1 << 12) + ((1 << 12) - 1))) / (double)(1 << 12));

        private readonly double mMin = Double.MinValue;
        private readonly double mMax = Double.MaxValue;

        public Fx16Range(double min, double max)
        {
            mMin = min;
            mMax = max;

            // Fx16: -7.99 to +7.99
            if (mMin < sMin)
                mMin = sMin;
            if (mMax > sMax)
                mMax = sMax;
        }

        private bool error(string valueStr, out string errorStr)
        {
            errorStr = String.Format("Value {0} out of range [{1},{2}]", valueStr, mMin, mMax);
            return false;
        }

        public override bool check(Int8 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt8 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int16 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt16 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int32 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt32 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int64 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt64 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(float c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(double c, out string errorStr)
        {
            double v = (c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
    }

    #endregion
    #region Fx32 Range

    public class Fx32Range : Range
    {
        private readonly double mMin = Double.MinValue;
        private readonly double mMax = Double.MaxValue;

        public Fx32Range(double min, double max)
        {
            mMin = min;
            mMax = max;
        }

        private bool error(string valueStr, out string errorStr)
        {
            errorStr = String.Format("Value {0} out of range [{1},{2}]", valueStr, mMin, mMax);
            return false;
        }

        public override bool check(Int8 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt8 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int16 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt16 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int32 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt32 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(Int64 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(UInt64 c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(float c, out string errorStr)
        {
            double v = ((double)c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
            errorStr = string.Empty;
            return true;
        }
        public override bool check(double c, out string errorStr)
        {
            double v = (c) / (1 << 12);
            if (v < mMin || v > mMax)
                return error(v.ToString(), out errorStr);
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
