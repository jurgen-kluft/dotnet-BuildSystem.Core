using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public class StringTable
    {
        #region Fields

        private readonly Dictionary<string, int> mDictionary = new ();
        private readonly List<uint> mHashes = new();
        private readonly List<int> mLengths = new();
        private readonly List<StreamReference> mReferences = new ();
        private readonly List<string> mStrings = new ();

        #endregion
        #region Properties

        public StreamReference Reference { get; set; }

        public string this[int index] => mStrings[index];

        private int Count => mStrings.Count;

        #endregion
        #region Public Methods

        public int Add(string inString)
        {
            if (!mDictionary.TryGetValue(inString, out int index))
            {
                index = mStrings.Count;

                mDictionary.Add(inString, index);
                mReferences.Add(StreamReference.NewReference);
                mStrings.Add(inString);

                byte[] utf8 = UTF8Encoding.UTF8.GetBytes(inString);
                mLengths.Add(utf8.Length + 1);

                uint hash = ComputeHashOf(inString);
                mHashes.Add(hash);
            }

            return mLengths[index];
        }

        private int InternalIndexOf(string inString)
        {
            if (!mDictionary.TryGetValue(inString, out int index))
                index = -1;
            return index;
        }

        private int IndexOf(string inString)
        {
            return InternalIndexOf(inString);
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
            return InternalReferenceOf(str);
        }

        public void SortByHash()
        {
            mHashes.Clear();
            Dictionary<uint, string> hashToString = new ();
            Dictionary<uint, StreamReference> hashToReference = new ();
            foreach (string s in mStrings)
            {
                uint hash = ComputeHashOf(s);
                mHashes.Add(hash);
                hashToString.Add(hash, s);
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
            writer.BeginBlock(Reference, sizeof(Int32));
            {
                StreamReference hashesReference = StreamReference.NewReference;
                StreamReference referencesReference = StreamReference.NewReference;
                StreamReference stringsReference = StreamReference.NewReference;

                writer.Write(Count);
                writer.Write(hashesReference);
                writer.Write(referencesReference);

                // String hashes
                writer.BeginBlock(hashesReference, sizeof(Int32));
                {
                    foreach (uint s in mHashes)
                        writer.Write(s);
                    writer.EndBlock();
                }

                // String References
                writer.BeginBlock(referencesReference, sizeof(Int32));
                {
                    foreach (string s in mStrings)
                    {
                        StreamReference r = InternalReferenceOf(s);
                        writer.Write(r);
                    }
                }

                // String Data
                writer.BeginBlock(stringsReference, sizeof(Int32));
                {
                    foreach (string s in mStrings)
                    {
                        StreamReference r = InternalReferenceOf(s);
                        if (writer.BeginBlock(r, sizeof(Int32)))
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
}
