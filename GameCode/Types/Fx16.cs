using System;

namespace GameData
{
    public struct Fx16 : IStruct
    {
        private Int16 mValue;

        public Fx16(Int32 v)
        {
            mValue = (Int16)(v << 12);
        }
        public Fx16(float v)
        {
            mValue = (Int16)(v * (1 << 12));
        }
        public Fx16(double v)
        {
            mValue = (Int16)(v * (1 << 12));
        }

        static public Fx16 SFromFloat(float v)
        {
            return new Fx16(v);
        }
        static public Fx16 SFromDouble(double v)
        {
            return new Fx16(v);
        }

        static public Fx16[] SArray(params Fx16[] values)
        {
            return values;
        }
        static public Fx16[] SArray(params float[] values)
        {
            Fx16[] array = new Fx16[values.Length];
            int i=0;
            foreach (float f in values)
            {
                array[i++] = Fx16.SFromFloat(f);
            }
            return array;
        }
        static public Fx16[] SArray(params double[] values)
        {
            Fx16[] array = new Fx16[values.Length];
            int i = 0;
            foreach (double f in values)
            {
                array[i++] = Fx16.SFromDouble(f);
            }
            return array;
        }

        public int StructSize => sizeof(Int16);
        public int StructAlign => 2;
        public string StructName => "fx16_t";
        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(mValue);
        }
    }
}
