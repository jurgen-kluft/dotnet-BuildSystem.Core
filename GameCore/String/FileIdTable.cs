using System;
using System.Collections.Generic;

namespace GameCore
{
    public class FileIdTable
    {
        private StreamReference mStreamReference;
        private readonly Dictionary<StreamReference, int> mReferenceToIndex = new ();
        private readonly Dictionary<long, int> mIDToIndex = new ();
        private readonly List<long> mItems = new();
        private readonly List<StreamReference> mReferences = new();

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

        public List<long> All
        {
            get
            {
                return mItems;
            }
        }

        public long this[int index]
        {
            get
            {
                return mItems[index];
            }
        }

        private int InternalIndexOf(long inItem)
        {
            int index;
            if (!mIDToIndex.TryGetValue(inItem, out index))
                index = -1;
            return index;
        }

        private StreamReference InternalReferenceOf(long inItem)
        {
            var index = InternalIndexOf(inItem);
            if (index == -1)
                return StreamReference.Empty;
            else
                return mReferences[index];
        }

        public StreamReference Add(StreamReference inReference, long inItem)
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

        public StreamReference ReferenceOf(long inItem)
        {
            return InternalReferenceOf(inItem);
        }

        public void Write(IDataWriter writer)
        {
            writer.NewBlock(Reference, 8, 2 * sizeof(ulong));
            writer.OpenBlock(Reference);
            {
                var idReference = StreamReference.NewReference;

                writer.Write((long)Count);
                writer.WriteBlockReference(idReference);

                writer.NewBlock(idReference, 8, mItems.Count * sizeof(long));
                writer.OpenBlock(idReference);
                {
                    for (var i = 0; i < mItems.Count; ++i)
                    {
                        var id = mItems[i];
                        writer.Mark(mReferences[i]);
                        writer.Write(id);
                    }
                    writer.CloseBlock();
                }
                writer.CloseBlock();
            }
        }
    }
}
