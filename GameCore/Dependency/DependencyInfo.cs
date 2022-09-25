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

        public enum EDepMethod
        {
            TIMESTAMP,
            HASH,
        }

        #endregion
        #region Constructors

        private DepInfo()
            : this(-1, string.Empty, string.Empty, EDepMethod.TIMESTAMP)
        {
        }

        public DepInfo(int index, string filename, string folder)
            : this(index, filename, folder, EDepMethod.TIMESTAMP)
        {
        }

        public DepInfo(int index, string filename, string folder, EDepMethod method)
            : this(index, filename, folder, method, Hash160.Empty)
        {
        }
        public DepInfo(int index, string filename, string folder, EDepMethod method, Hash160 hash)
        {
            Index = index;
            Filename = filename;
            Folder = folder;
            Full = Path.Join(Folder, Filename);
            Hash = hash;
            Method = method;
        }

        #endregion
        #region Properties

        public int Index {get; set; }
        public string Filename {get; private set; }
        public string Folder {get; private set; }
        public string Full {get; set; }
        public Hash160 Hash {get; set; } = Hash160.Empty;
        public EStatus Status {get; set; }= EStatus.UNINITIALIZED;
        public EDepMethod Method {get; set; }= EDepMethod.TIMESTAMP;
        public EDepRule Rule {get; set; }= EDepRule.ON_CHANGE;

        #endregion
        #region Methods

        internal void init()
        {
            Hash = Hash160.Empty;
            Status = EStatus.UNINITIALIZED;

            update();

            // The update told us that the file is there so we can modify it
            // to the initial state 'UNCHANGED'.
            if (Status == EStatus.CHANGED)
                Status = EStatus.UNCHANGED;
        }

        private static Hash160 computeHash(string fullFilename, EDepMethod method)
        {
            switch (method)
            {
                case DepInfo.EDepMethod.HASH:
                    {
                        FileInfo fileInfo = new FileInfo(fullFilename);
                        return fileInfo.Exists ? HashUtility.compute(fileInfo) : Hash160.Empty;
                    }

                case DepInfo.EDepMethod.TIMESTAMP:
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
            Hash160 newHash = computeHash(Full, Method);
            if (Rule == EDepRule.MUST_EXIST)
            {
                if (newHash == Hash160.Empty || newHash != Hash)
                {
                    Status = EStatus.CHANGED;
                    Hash = newHash;
                }
                else
                {
                    Status = EStatus.UNCHANGED;
                }
            }
            else
            {
                if (Hash == newHash)
                {
                    Status = EStatus.UNCHANGED;
                }
                else
                {
                    Status = EStatus.CHANGED;
                    Hash = newHash;
                }
            }
        }

        public void setFilenameAndFolder(string filename, string folder)
        {
            Filename = filename;
            Folder = folder;
            Full = Path.Join(folder + filename);
        }

        public bool checkifEqual(DepInfo other)
        {
            return (Full == other.Full);
        }

        #endregion
        #region Operators

        public static bool operator ==(DepInfo a, DepInfo b)
        {
            if ((object)a == null && (object)b == null)
                return true;
            if ((object)a == null || (object)b == null)
                return false;

            return a.Full == b.Full && a.Hash == b.Hash;
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
            return depInfo.Full == Full && depInfo.Hash == Hash;
        }

        public override int GetHashCode()
        {
            return Full.GetHashCode();
        }

        #endregion
    }
}