using System;
using GameCore;

namespace DataBuildSystem
{
    public sealed class BigfileFile
    {
        #region Fields

        public readonly static BigfileFile Empty = new(string.Empty, 0, StreamOffset.Empty);

        #endregion
        #region Constructor

        public BigfileFile(BigfileFile other)
        {
            Filename = other.Filename;
            FileSize = other.FileSize;
            FileOffset = new StreamOffset(other.FileOffset.value);
            FileId = other.FileId;
            FileContentHash = other.FileContentHash;
        }

        public BigfileFile(string filename)
        {
            Filename = filename;
            FileSize = -1;
            FileOffset = StreamOffset.Zero;
            FileId = Int64.MaxValue;
            FileContentHash = Hash160.Empty;
        }

        public BigfileFile(string filename, Int32 size)
        {
            Filename = filename;
            FileSize = size;
            FileOffset = StreamOffset.Empty;
            FileId = Int64.MaxValue;
            FileContentHash = Hash160.Empty;
        }

        public BigfileFile(string filename, Int32 size, StreamOffset offset)
        {
            Filename = filename;
            FileSize = size;
            FileOffset = offset;
            FileId = Int64.MaxValue;
            FileContentHash = Hash160.Empty;
        }

        public BigfileFile(string filename, Int32 size, Int64 fileId)
        {
            Filename = filename;
            FileSize = size;
            FileOffset = StreamOffset.Empty;
            FileId = fileId;
            FileContentHash = Hash160.Empty;
        }

        public BigfileFile(string filename, Int32 size, Int64 fileId, Hash160 contentHash)
        {
            Filename = filename;
            FileSize = size;
            FileOffset = StreamOffset.Empty;
            FileId = fileId;
            FileContentHash = contentHash;
        }

        public BigfileFile(string filename, Int32 size, StreamOffset offset, Int64 fileId)
        {
            Filename = filename;
            FileSize = size;
            FileOffset = offset;
            FileId = fileId;
            FileContentHash = Hash160.Empty;
        }

        public BigfileFile(string filename, Int32 size, StreamOffset offset, Int64 fileId, Hash160 contentHash)
        {
            Filename = filename;
            FileSize = size;
            FileOffset = offset;
            FileId = fileId;
            FileContentHash = contentHash;
        }

        #endregion
        #region Properties

        public bool IsEmpty
        {
            get
            {
                return FileSize == 0 && FileOffset.isEmpty && Filename == string.Empty;
            }
        }

        public string Filename { get; set; }
        public Int32 FileSize { get; set; }
        public StreamOffset FileOffset { get; set; }
        public Int64 FileId { get; set; }
        public Hash160 FileContentHash { get; set; }
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
            return FileOffset.value.GetHashCode();
        }

        #endregion
    }
}
