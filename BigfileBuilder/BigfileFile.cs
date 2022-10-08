using System;
using GameCore;

namespace DataBuildSystem
{
    public sealed class BigfileFile
    {
        #region Fields

        public readonly static BigfileFile Empty = new(string.Empty);

        #endregion
        #region Constructor

        public BigfileFile(string filename)
        {
            Filename = filename;
        }

        #endregion
        #region Properties

        public string Filename { get; private set; }
        public Int32 FileSize { get; set; } = 0;
        public StreamOffset FileOffset { get; set; } = StreamOffset.Empty;
        public Int64 FileId { get; set; } = -1;
        public Hash160 FileContentHash { get; set; } = Hash160.Empty;
        public List<BigfileFile> Children { get; set; } = new ();

        #endregion
        #region Operators

        public static bool operator ==(BigfileFile a, BigfileFile b)
        {
            return (a.FileSize == b.FileSize && a.Filename == b.Filename && a.FileOffset == b.FileOffset);
        }
        public static bool operator !=(BigfileFile a, BigfileFile b)
        {
            if (a.FileSize != b.FileSize || a.Filename != b.Filename || a.FileOffset != b.FileOffset)
                return true;
            return false;
        }

        #endregion
        #region Comparer (IEqualityComparer)

        public override bool Equals(object o)
        {
            if (o is BigfileFile other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Filename.GetHashCode();
        }

        #endregion
    }
}
