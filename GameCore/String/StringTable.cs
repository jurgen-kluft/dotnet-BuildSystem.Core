using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public class StringTable
    {
        #region Fields

        private readonly UTF8Encoding mEncoding = new UTF8Encoding();
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

                byte[] utf8 = mEncoding.GetBytes(inString);
                mLengths.Add(utf8.Length + 1);

                uint hash = Hashing.Compute(utf8);
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
            Dictionary<uint, string> hashToString = new ();
            Dictionary<uint, int> hashToLength = new ();
            Dictionary<uint, StreamReference> hashToReference = new ();
            for (int i=0; i<mStrings.Count; ++i)
            {
                uint hash = mHashes[i];
                string str = mStrings[i];
                hashToString.Add(hash, str);
                hashToLength.Add(hash, mLengths[i]);
                hashToReference.Add(hash, InternalReferenceOf(str));
            }

            mHashes.Sort();

            mStrings.Clear();
            mReferences.Clear();
            mLengths.Clear();
            foreach (uint hash in mHashes)
            {
                hashToString.TryGetValue(hash, out string s);
                mStrings.Add(s);
                hashToReference.TryGetValue(hash, out StreamReference r);
                mReferences.Add(r);
                hashToLength.TryGetValue(hash, out int l);
                mLengths.Add(l);
            }

            mDictionary.Clear();
            for (int i=0; i<mStrings.Count; ++i)
            {
                mDictionary.Add(mStrings[i], i);
            }
        }

        public void Write(IBinaryStream writer, Dictionary<StreamReference, StreamOffset> dataOffsetDataBase)
        {
            SortByHash();

            StreamOffset offset = new(writer.Position);

            // Need to determine some good size
            byte[] utf8 = new byte[8192];

            // Write strings and assign them a StreamReference and StreamOffset
            foreach (string s in mStrings)
            {
                StreamReference r = InternalReferenceOf(s);
                dataOffsetDataBase.Add(r, offset);

                int utf8Len = mEncoding.GetBytes(s, 0, s.Length, utf8, 0);
                utf8[utf8Len] = 0; // Do include a terminating zero
                writer.Write(utf8, 0, utf8Len + 1);

                offset += utf8Len + 1;
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
