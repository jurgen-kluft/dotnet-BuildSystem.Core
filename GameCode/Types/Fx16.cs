using System;

namespace GameData
{
    public struct Fx16 : IStruct
    {
        private short mValue;

        public Fx16(int v)
        {
            mValue = (short)(v << 12);
        }

        public Fx16(float v)
        {
            mValue = (short)(v * (1 << 12));
        }

        public Fx16(double v)
        {
            mValue = (short)(v * (1 << 12));
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
            var array = new Fx16[values.Length];
            var i = 0;
            foreach (var f in values)
            {
                array[i++] = Fx16.SFromFloat(f);
            }

            return array;
        }

        static public Fx16[] SArray(params double[] values)
        {
            var array = new Fx16[values.Length];
            var i = 0;
            foreach (var f in values)
            {
                array[i++] = Fx16.SFromDouble(f);
            }

            return array;
        }

        public int StructSize => sizeof(short);
        public int StructAlign => 2;
        public string StructMember => "fx16_t";
        public string[] StructCode()
        {
            return Array.Empty<string>();
        }

        public void StructWrite(IGameDataWriter writer)
        {
            GameCore.BinaryWriter.Write(writer, mValue);
        }
    }
}
