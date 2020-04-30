using System;

namespace Game.Data
{
    public struct Fx32 : IAtom
    {
        private Int32 mValue;

        public Fx32(Int32 v)
        {
            mValue = (Int32)(v << 12);
        }
        public Fx32(float v)
        {
            mValue = (Int32)(v * (1 << 12));
        }
        public Fx32(double v)
        {
            mValue = (Int32)(v * (1 << 12));
        }

        static public Fx32 sFromFloat(float v)
        {
            return new Fx32(v);
        }
        static public Fx32 sFromDouble(double v)
        {
            return new Fx32(v);
        }

        static public Fx32[] sArray(params Fx32[] values)
        {
            return values;
        }
        static public Fx32[] sArray(params float[] values)
        {
            Fx32[] array = new Fx32[values.Length];
            int i=0;
            foreach (float f in values)
            {
                array[i++] = Fx32.sFromFloat(f);
            }
            return array;
        }
        static public Fx32[] sArray(params double[] values)
        {
            Fx32[] array = new Fx32[values.Length];
            int i = 0;
            foreach (double f in values)
            {
                array[i++] = Fx32.sFromDouble(f);
            }
            return array;
        }

        public object Value { get { return mValue; } }
    }
}
