using System;
using System.Collections.Generic;

namespace GameCore
{
    public class FileIdTable
    {
        #region Fields

        private StreamReference mStreamReference;
        private readonly Dictionary<StreamReference, int> mReferenceToIndex = new ();
        private readonly Dictionary<Int64, int> mIDToIndex = new ();
        private readonly List<Int64> mItems = new();
        private readonly List<StreamReference> mReferences = new();

        #endregion
        #region Properties

        public StreamReference Reference
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

        public List<Int64> All
        {
            get
            {
                return mItems;
            }
        }

        public Int64 this[int index]
        {
            get
            {
                return mItems[index];
            }
        }

        #endregion
        #region Private Methods

        private int InternalIndexOf(Int64 inItem)
        {
            int index;
            if (!mIDToIndex.TryGetValue(inItem, out index))
                index = -1;
            return index;
        }

        private StreamReference InternalReferenceOf(Int64 inItem)
        {
            int index = InternalIndexOf(inItem);
            if (index == -1)
                return StreamReference.Empty;
            else
                return mReferences[index];
        }

        #endregion
        #region Public Methods

        public StreamReference Add(StreamReference inReference, Int64 inItem)
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

        public StreamReference ReferenceOf(Int64 inItem)
        {
            return InternalReferenceOf(inItem);
        }

        public void Write(IDataWriter writer)
        {
            writer.BeginBlock(Reference, sizeof(Int32));
            {
                StreamReference idReference = StreamReference.Instance;

                writer.Write(Count);
                writer.Write(idReference);

                writer.BeginBlock(idReference, 16);
                {
                    for (int i = 0; i < mItems.Count; ++i)
                    {
                        Int64 id = mItems[i];
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
