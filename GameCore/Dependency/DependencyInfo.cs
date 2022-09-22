using System;
using System.IO;

namespace GameCore
{
    public class DepInfo
    {
        #region Fields

        public static readonly DepInfo Empty = new DepInfo();

        public enum EStatus
        {
            UNINITIALIZED,      /// Not initialized
            NOT_FOUND,          /// File doesn't exist
            CHANGED,            /// File state has changed since previous check
            UNCHANGED,          /// File state is identical to previous check
        }

        public enum EDepRule
        {
            ON_CHANGE,
            MUST_EXIST,
        }

        public enum EMethod
        {
            TIMESTAMP,
            MD5,
        }

        private Filename mFilename;
        private Dirname mFolder;
        private Hash160 mHash = Hash160.Empty;
        private EStatus mStatus = EStatus.UNINITIALIZED;
        private EMethod mMethod = EMethod.TIMESTAMP;
        private EDepRule mDepRule = EDepRule.ON_CHANGE;

        // Caching
        private string mFullFilename = string.Empty;

        #endregion
        #region Constructors

        private DepInfo()
            : this(Filename.Empty, Dirname.Empty, EMethod.TIMESTAMP)
        {
        }

        public DepInfo(Filename filename, Dirname folder)
            : this(filename, folder, EMethod.TIMESTAMP)
        {
        }

        public DepInfo(Filename filename, Dirname folder, EMethod method)
            : this(filename, folder, method, Hash160.Empty)
        {
        }
        public DepInfo(Filename filename, Dirname folder, EMethod method, Hash160 hash)
        {
            mFilename = filename;
            mFolder = folder;
            mFullFilename = mFolder + mFilename;
            mHash = hash;
            mMethod = method;
        }

        #endregion
        #region Properties

        public Filename filename
        {
            get
            {
                return mFilename;
            }
        }

        public Dirname folder
        {
            get
            {
                return mFolder;
            }
        }

        public string full
        {
            get
            {
                return mFullFilename;
            }

        }

        public Hash160 hash
        {
            set
            {
                mHash = value;
            }
            get
            {
                return mHash;
            }
        }

        public EStatus status
        {
            set
            {
                mStatus = value;
            }
            get
            {
                return mStatus;
            }
        }

        public EMethod method
        {
            set
            {
                mMethod = value;
            }
            get
            {
                return mMethod;
            }
        }

        public EDepRule deprule
        {
            set
            {
                mDepRule = value;
            }
            get
            {
                return mDepRule;
            }
        }

        #endregion
        #region Methods

        internal void init()
        {
            hash = Hash160.Empty;
            status = EStatus.UNINITIALIZED;

            update();

            // The update told us that the file is there so we can modify it
            // to the initial state 'UNCHANGED'.
            if (status == EStatus.CHANGED)
                status = EStatus.UNCHANGED;
        }

        private static Hash160 computeHash(string fullFilename, EMethod method)
        {
            switch (method)
            {
                case DepInfo.EMethod.MD5:
                    {
                        FileInfo fileInfo = new FileInfo(fullFilename);
                        return fileInfo.Exists ? HashUtility.compute(fileInfo) : Hash160.Empty;
                    }

                case DepInfo.EMethod.TIMESTAMP:
                    {
                        Hash160 hash = Hash160.Empty;
                        if (File.Exists(fullFilename))
                            hash = Hash160.FromDateTime(File.GetLastWriteTime(fullFilename));
                        return hash;
                    }

                default:
                    return Hash160.Empty;
            }
        }

        internal void update()
        {
            Hash160 newHash = computeHash(mFullFilename, mMethod);
            if (mDepRule == EDepRule.MUST_EXIST)
            {
                if (newHash == Hash160.Empty || newHash != hash)
                {
                    status = EStatus.CHANGED;
                    hash = newHash;
                }
                else
                {
                    status = EStatus.UNCHANGED;
                }
            }
            else
            {
                if (hash == newHash)
                {
                    status = EStatus.UNCHANGED;
                }
                else
                {
                    status = EStatus.CHANGED;
                    hash = newHash;
                }
            }
        }

        public void setFilenameAndFolder(Filename filename, Dirname folder)
        {
            mFilename = filename;
            mFolder = folder;
            mFullFilename = folder + filename;
        }

        public bool checkifEqual(DepInfo other)
        {
            return (mFullFilename == other.mFullFilename);
        }

        #endregion
        #region Operators

        public static bool operator ==(DepInfo a, DepInfo b)
        {
            if ((object)a == null && (object)b == null)
                return true;
            if ((object)a == null || (object)b == null)
                return false;

            return a.mFullFilename == b.mFullFilename && a.mHash == b.mHash;
        }
        public static bool operator !=(DepInfo a, DepInfo b)
        {
            return !(a == b);
        }

        #endregion
        #region Equals, GetHashCode

        public override bool Equals(object obj)
        {
            DepInfo depInfo = (DepInfo)obj;
            return depInfo.mFullFilename == mFullFilename && depInfo.mHash == mHash;
        }

        public override int GetHashCode()
        {
            return mFullFilename.GetHashCode();
        }

        #endregion
    }
}