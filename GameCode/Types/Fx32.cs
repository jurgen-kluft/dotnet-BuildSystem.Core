using System;

namespace GameData
{
    public struct Fx32 : IStruct
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

        static public Fx32 SFromFloat(float v)
        {
            return new Fx32(v);
        }
        static public Fx32 SFromDouble(double v)
        {
            return new Fx32(v);
        }

        static public Fx32[] SArray(params Fx32[] values)
        {
            return values;
        }
        static public Fx32[] SArray(params float[] values)
        {
            Fx32[] array = new Fx32[values.Length];
            int i=0;
            foreach (float f in values)
            {
                array[i++] = Fx32.SFromFloat(f);
            }
            return array;
        }
        static public Fx32[] SArray(params double[] values)
        {
            Fx32[] array = new Fx32[values.Length];
            int i = 0;
            foreach (double f in values)
            {
                array[i++] = Fx32.SFromDouble(f);
            }
            return array;
        }

        public int StructSize => sizeof(Int32);
        public int StructAlign => 4;
        public string StructName => "fx32_t";
        public void StructWrite(IBinaryWriter writer)
        {
            writer.Write(mValue);
        }
    }
}
