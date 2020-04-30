using System;

namespace Core
{
    public struct Indent
    {
        private string mIndent;

        public Indent(int i)
        {
            mIndent = string.Empty;
            while (--i >= 0)
            {
                mIndent += "\t";
            }
        }
        public Indent(Indent i)
        {
            mIndent = i.mIndent;
        }

        public override string ToString()
        {
            return mIndent;
        }

        public static Indent operator ++(Indent i)
        {
            Indent ni = new Indent(i);
            ni.mIndent += "\t";
            return ni;
        }
        public static Indent operator --(Indent i)
        {
            Indent ni = new Indent(i);
            ni.mIndent = ni.mIndent.Substring(0, ni.mIndent.Length - 1);
            return ni;
        }
    }
}
