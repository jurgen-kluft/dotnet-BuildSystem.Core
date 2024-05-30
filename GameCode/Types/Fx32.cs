using System;

namespace GameData
{
    public struct Fx32 : IStruct
    {
        private int mValue;

        public Fx32(int v)
        {
            mValue = (int)(v << 12);
        }

        public Fx32(float v)
        {
            mValue = (int)(v * (1 << 12));
        }

        public Fx32(double v)
        {
            mValue = (int)(v * (1 << 12));
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
            var array = new Fx32[values.Length];
            var i = 0;
            foreach (var f in values)
            {
                array[i++] = Fx32.SFromFloat(f);
            }

            return array;
        }

        static public Fx32[] SArray(params double[] values)
        {
            var array = new Fx32[values.Length];
            var i = 0;
            foreach (var f in values)
            {
                array[i++] = Fx32.SFromDouble(f);
            }

            return array;
        }

        public bool StructIsValueType => true;
        public int StructSize => sizeof(uint);
        public int StructAlign => 4;
        public string StructName => "fx32_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(mValue);
        }
    }
}
