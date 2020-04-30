using System;
using System.Collections.Generic;

namespace Core
{
    public class StringTable
    {
        #region Fields

        private StreamReference mStreamReference;
        private bool mAllLowerCase;
        private readonly Dictionary<string, int> mDictionary = new Dictionary<string, int>();
        private readonly List<string> mStrings = new List<string>();
        private readonly List<uint> mHashes = new List<uint>();
        private readonly List<StreamReference> mReferences = new List<StreamReference>();

        #endregion
        #region Properties

        public StreamReference reference
        {
            get
            {
                return mStreamReference;
            }
            set
            {
                mStreamReference = value;
            }
        }

        public bool forceLowerCase
        {
            get
            {
                return mAllLowerCase;
            }
            set
            {
                mAllLowerCase = value;
            }
        }

        public int Count
        {
            get
            {
                return mStrings.Count;
            }
        }

        public List<string> All
        {
            get
            {
                return mStrings;
            }
        }

        public string this[int index]
        {
            get
            {
                return mStrings[index];
            }
        }

        #endregion
        #region Public Methods

        public void Add(string inString)
        {
            string str = inString;
            if (mAllLowerCase)
                str = str.ToLower();

            uint hash = ComputeHashOf(str);

            int index;
            if (!mDictionary.TryGetValue(str, out index))
            {
                mDictionary.Add(str, mStrings.Count);
                mReferences.Add(StreamReference.Instance);
                mStrings.Add(str);
                mHashes.Add(hash);
            }
        }

        private int InternalIndexOf(string inString)
        {
            int index;
            if (!mDictionary.TryGetValue(inString, out index))
                index = -1;
            return index;
        }

        private int IndexOf(string inString)
        {
            string str = inString;
            if (mAllLowerCase)
                str = str.ToLower();
            return InternalIndexOf(str);
        }

        private uint ComputeHashOf(string inString)
        {
            return Hashing.Compute_ASCII(inString);
        }

        public uint HashOf(string inString)
        {
            int index = IndexOf(inString);
            if (index == -1) 
                return UInt32.MaxValue;
            else
                return mHashes[index];
        }

        private StreamReference InternalReferenceOf(string inString)
        {
            int index = InternalIndexOf(inString);
            if (index==-1) 
                return StreamReference.Empty;
            else
                return mReferences[index];
        }

        public StreamReference ReferenceOf(string inString)
        {
            string str = inString;
            if (mAllLowerCase)
                str = str.ToLower();
            return InternalReferenceOf(str);
        }

        public void SortByHash()
        {
            mHashes.Clear();
            Dictionary<uint, string> hashToString = new Dictionary<uint, string>();
            Dictionary<uint, StreamReference> hashToReference = new Dictionary<uint, StreamReference>();
            foreach (string s in mStrings)
            {
                string str = s;
                if (mAllLowerCase)
                    str = str.ToLower();

                uint hash = ComputeHashOf(str);
                mHashes.Add(hash);
                hashToString.Add(hash, str);
                hashToReference.Add(hash, InternalReferenceOf(s));
            }

            mHashes.Sort();

            mStrings.Clear();
            mReferences.Clear();
            foreach (uint hash in mHashes)
            {
                string s;
                hashToString.TryGetValue(hash, out s);
                mStrings.Add(s);
                StreamReference r;
                hashToReference.TryGetValue(hash, out r);
                mReferences.Add(r);
            }

            int index = 0;
            mDictionary.Clear();
            foreach (string s in mStrings)
            {
                mDictionary.Add(s, index);
                ++index;
            }
        }

        public void Write(IDataWriter writer)
        {
            SortByHash();

            // Write StringTable
            writer.BeginBlock(reference, EStreamAlignment.ALIGN_32);
            {
                StreamReference hashesReference = StreamReference.Instance;
                StreamReference referencesReference = StreamReference.Instance;
                StreamReference stringsReference = StreamReference.Instance;

                writer.Write(Count);
                writer.Write(hashesReference);
                writer.Write(referencesReference);

                // String hashes
                writer.BeginBlock(hashesReference, EStreamAlignment.ALIGN_32);
                {
                    foreach (uint s in mHashes)
                        writer.Write(s);
                    writer.EndBlock();
                }

                // String References
                writer.BeginBlock(referencesReference, EStreamAlignment.ALIGN_32);
                {
                    foreach (string s in mStrings)
                    {
                        StreamReference r = InternalReferenceOf(s);
                        writer.Write(r);
                    }
                }

                // String Data
                writer.BeginBlock(stringsReference, EStreamAlignment.ALIGN_32);
                {
                    foreach (string s in mStrings)
                    {
                        StreamReference r = InternalReferenceOf(s);
                        if (writer.BeginBlock(r, EStreamAlignment.ALIGN_8))
                        {
                            writer.Write(s);
                            writer.EndBlock();
                        }
                    }
                }
            }
            writer.EndBlock();
        }

        #endregion
    }

    public class FileIdTable
    {
        #region Fields

        private StreamReference mStreamReference;
        private readonly Dictionary<StreamReference, int> mReferenceToIndex = new Dictionary<StreamReference, int>();
        private readonly Dictionary<Hash128, int> mHashToIndex = new Dictionary<Hash128, int>();
        private readonly List<Hash128> mItems = new List<Hash128>();
        private readonly List<StreamReference> mReferences = new List<StreamReference>();

        #endregion
        #region Properties

        public StreamReference reference
        {
            get
            {
                return mStreamReference;
            }
            set
            {
                mStreamReference = value;
            }
        }

        public int Count
        {
            get
            {
                return mItems.Count;
            }
        }

        public List<Hash128> All
        {
            get
            {
                return mItems;
            }
        }

        public Hash128 this[int index]
        {
            get
            {
                return mItems[index];
            }
        }

        #endregion
        #region Private Methods

        private int InternalIndexOf(Hash128 inItem)
        {
            int index;
            if (!mHashToIndex.TryGetValue(inItem, out index))
                index = -1;
            return index;
        }

        private StreamReference InternalReferenceOf(Hash128 inItem)
        {
            int index = InternalIndexOf(inItem);
            if (index == -1)
                return StreamReference.Empty;
            else
                return mReferences[index];
        }

        #endregion
        #region Public Methods

        public StreamReference Add(StreamReference inReference, Hash128 inItem)
        {
            int index;
            if (!mReferenceToIndex.TryGetValue(inReference, out index))
            {
                mReferenceToIndex.Add(inReference, mItems.Count);
                mHashToIndex.Add(inItem, mReferences.Count);
                mReferences.Add(inReference);
                mItems.Add(inItem);
                return inReference;
            }
            else
            {
                return mReferences[index];
            }
        }

        public StreamReference ReferenceOf(Hash128 inItem)
        {
            return InternalReferenceOf(inItem);
        }

        public void Write(IDataWriter writer)
        {
            writer.BeginBlock(reference, EStreamAlignment.ALIGN_32);
            {
                StreamReference idReference = StreamReference.Instance;

                writer.Write(Count);
                writer.Write(idReference);

                writer.BeginBlock(idReference, EStreamAlignment.ALIGN_128);
                {
                    for (int i = 0; i < mItems.Count; ++i)
                    {
                        byte[] data = Hash128.ToBinary(mItems[i]);
                        writer.Mark(mReferences[i]);
                        writer.Write(data);
                    }
                    writer.EndBlock();
                }
                writer.EndBlock();
            }
        }


        #endregion
    }
}
