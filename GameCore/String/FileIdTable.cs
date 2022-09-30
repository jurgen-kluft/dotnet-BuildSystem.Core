using System;
using System.Collections.Generic;

namespace GameCore
{
    public class FileIdTable
    {
        #region Fields

        private StreamReference mStreamReference;
        private readonly Dictionary<StreamReference, int> mReferenceToIndex = new ();
        private readonly Dictionary<UInt64, int> mIDToIndex = new ();
        private readonly List<UInt64> mItems = new();
        private readonly List<StreamReference> mReferences = new();

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

        public List<UInt64> All
        {
            get
            {
                return mItems;
            }
        }

        public UInt64 this[int index]
        {
            get
            {
                return mItems[index];
            }
        }

        #endregion
        #region Private Methods

        private int InternalIndexOf(UInt64 inItem)
        {
            int index;
            if (!mIDToIndex.TryGetValue(inItem, out index))
                index = -1;
            return index;
        }

        private StreamReference InternalReferenceOf(UInt64 inItem)
        {
            int index = InternalIndexOf(inItem);
            if (index == -1)
                return StreamReference.Empty;
            else
                return mReferences[index];
        }

        #endregion
        #region Public Methods

        public StreamReference Add(StreamReference inReference, UInt64 inItem)
        {
            int index;
            if (!mReferenceToIndex.TryGetValue(inReference, out index))
            {
                mReferenceToIndex.Add(inReference, mItems.Count);
                mIDToIndex.Add(inItem, mReferences.Count);
                mReferences.Add(inReference);
                mItems.Add(inItem);
                return inReference;
            }
            else
            {
                return mReferences[index];
            }
        }

        public StreamReference ReferenceOf(UInt64 inItem)
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
                        UInt64 id = mItems[i];
                        writer.Mark(mReferences[i]);
                        writer.Write(id);
                    }
                    writer.EndBlock();
                }
                writer.EndBlock();
            }
        }


        #endregion
    }
}
