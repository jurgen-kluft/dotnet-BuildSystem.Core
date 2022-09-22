using System;
using GameCore;

namespace DataBuildSystem
{
    public struct BigfileFile
    {
        #region Fields

        public readonly static BigfileFile Empty = new BigfileFile(Filename.Empty, 0, StreamOffset.Empty);

        private Filename mFilename;
        private Int64 mFileSize;
        private StreamOffset[] mFileOffsets;
        private Hash128 mContentHash;                                                  // Content hash

        #endregion
        #region Constructor

        public BigfileFile(BigfileFile file)
        {
            mFilename = file.filename;
            mFileSize = file.size;
            mFileOffsets = file.offsets;
            mContentHash = Hash128.Empty;
        }

        public BigfileFile(Filename filename, Int64 size)
        {
            mFilename = filename;
            mFileSize = size;
            mFileOffsets = new StreamOffset[] { StreamOffset.Empty };
            mContentHash = Hash128.Empty;
        }

        public BigfileFile(Filename filename, Int64 size, StreamOffset offset)
        {
            mFilename = filename;
            mFileSize = size;
            mFileOffsets = new StreamOffset[] { offset };
            mContentHash = Hash128.Empty;
        }

        public BigfileFile(Filename filename, Int64 size, Hash128 contentHash)
        {
            mFilename = filename;
            mFileSize = size;
            mFileOffsets = new StreamOffset[] { StreamOffset.Empty };
            mContentHash = contentHash;
        }

        public BigfileFile(Filename filename, Int64 size, StreamOffset offset, Hash128 contentHash)
        {
            mFilename = filename;
            mFileSize = size;
            mFileOffsets = new StreamOffset[] { offset };
            mContentHash = contentHash;
        }
        
        public BigfileFile(Filename filename, Int64 size, StreamOffset[] offsets)
        {
            mFilename = filename;
            mFileSize = size;
            mFileOffsets = new StreamOffset[offsets.Length];
            offsets.CopyTo(mFileOffsets, 0);
            mContentHash = Hash128.Empty;
        }

        public BigfileFile(Filename filename, Int64 size, StreamOffset[] offsets, Hash128 contentHash)
        {
            mFilename = filename;
            mFileSize = size;
            mFileOffsets = new StreamOffset[offsets.Length];
            offsets.CopyTo(mFileOffsets, 0);
            mContentHash = contentHash;
        }

        #endregion
        #region Properties

        public bool IsEmpty
        {
            get
            {
                return mFileSize == 0 && mFileOffsets[0].isEmpty && mFilename.IsEmpty;
            }
        }

        public Filename filename
        {
            set
            {
                mFilename = value;
            }
            get
            {
                return mFilename;
            }
        }

        public Int32 size32
        {
            get
            {
                return (Int32)mFileSize;
            }
        }

        public Int64 size
        {
            get
            {
                return mFileSize;
            }
            set
            {
                mFileSize = value;
            }
        }

        public StreamOffset offset
        {
            set
            {
                mFileOffsets = new StreamOffset[1];
                mFileOffsets[0] = value;
            }
            get
            {
                return mFileOffsets[0];
            }
        }


        public StreamOffset[] offsets
        {
            set
            {
                mFileOffsets = new StreamOffset[value.Length];
                value.CopyTo(mFileOffsets, 0);
            }
            get
            {
                return mFileOffsets;
            }
        }

        public Hash128 contenthash
        {
            get
            {
                return mContentHash;
            }
        }

        #endregion
        #region Operators

        public static bool operator ==(BigfileFile a, BigfileFile b)
        {
            if (a.mFileSize == b.mFileSize && a.mFilename == b.mFilename && a.offsets.Length == b.offsets.Length)
            {
                for (int i = 0; i < a.offsets.Length; ++i)
                    if (a.offsets[i] != b.offsets[i])
                        return false;

                return true;
            }
            return false;
        }
        public static bool operator !=(BigfileFile a, BigfileFile b)
        {
            if (a.mFileSize != b.mFileSize || a.mFilename != b.mFilename || a.offsets.Length != b.offsets.Length)
                return false;

            for (int i = 0; i<a.offsets.Length; ++i)
                if (a.offsets[i] != b.offsets[i])
                    return true;

            return false;
        }

        #endregion
        #region Comparer (IEqualityComparer)

        public override bool Equals(object o)
        {
            if (o is BigfileFile)
            {
                BigfileFile other = (BigfileFile)o;
                return this == other;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return mFileOffsets[0].value32;
        }

        #endregion
    }
}
