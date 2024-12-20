using System.Text;

namespace GameCore
{
    public class StringTable
    {
        private readonly UTF8Encoding _encoding = new ();
        private readonly Dictionary<string, int> _dictionary;
        private readonly List<Hash160> _hashes;
        private readonly List<int> _byteLengths;
        private readonly List<string> _strings;
        private byte[] _utf8;

        public StringTable(int estimatedNumberOfStrings = 4096, int longestUtf8StrLen = 8192)
        {
            _dictionary = new Dictionary<string, int>(estimatedNumberOfStrings);
            _hashes = new List<Hash160>(estimatedNumberOfStrings);
            _byteLengths = new List<int>(estimatedNumberOfStrings);
            _strings = new List<string>(estimatedNumberOfStrings);
            _utf8 = new byte[longestUtf8StrLen];

            Reference = StreamReference.NewReference;
            HashesReference = StreamReference.NewReference;
            OffsetsReference = StreamReference.NewReference;
            RuneLengthsReference  = StreamReference.NewReference;
            ByteLengthsReference  = StreamReference.NewReference;
            StringsReference = StreamReference.NewReference;
        }

        private StreamReference Reference { get;  }
        private StreamReference  HashesReference { get;  }
        private StreamReference  OffsetsReference { get;  }
        private StreamReference  RuneLengthsReference { get;  }
        private StreamReference  ByteLengthsReference { get;  }
        private StreamReference  StringsReference { get;  }

        private int Count => _strings.Count;

        public int Add(string inString)
        {
            if (_dictionary.TryGetValue(inString, out var index)) return index;
            index = _strings.Count;

            _dictionary.Add(inString, index);
            _strings.Add(inString);

            var count = _encoding.GetByteCount(inString);
            if ((count + 1) > _utf8.Length)
            {
                _utf8 = new byte[_utf8.Length + (_utf8.Length / 4)];
            }

            var len = _encoding.GetBytes(inString, 0, inString.Length, _utf8, 0);
            _utf8[len] = 0; // Do include a terminating zero
            _byteLengths.Add(len + 1);
            var hash = HashUtility.Compute(_utf8, 0, len);
            _hashes.Add(hash);
            return index;
        }


        public int ByteCountForIndex(int index)
        {
            return _byteLengths[index];
        }

        private struct HashIndex
        {
            public Hash160 Hash;
            public int Index;
        };

        public void Write(IDataBlockWriter blockWriter)
        {
            // Sort Hashes, HashIndex is used to keep track of the original index
            var sortedHashes = new List<HashIndex>(_hashes.Count);
            var count = 0;
            foreach (var hash in _hashes)
            {
                sortedHashes.Add(new HashIndex { Hash = hash, Index = count });
                count += 1;
            }
            sortedHashes.Sort((a, b) => Hash160.Compare(a.Hash, b.Hash));

            // We need to precompute the size of the string table parts
            var hashesSize = count * sizeof(uint);
            var offsetsSize = count * sizeof(uint);
            var runeLengthsSize = count * sizeof(uint);
            var byteLengthsSize = count * sizeof(uint);

            // u32         mMagic;  // 'STRT'
            // u32         mNumStrings;
            // u32 const*  mHashes;
            // u32 const*  mOffsets;
            // u32 const*  mCharLengths;
            // u32 const*  mByteLengths;
            // const char* mStrings;
            const int mainSize = sizeof(int) + sizeof(int) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong);

            // Write StringTable

            blockWriter.NewBlock(Reference, 8);
            blockWriter.NewBlock(HashesReference, 8);
            blockWriter.NewBlock(OffsetsReference, 8);
            blockWriter.NewBlock(RuneLengthsReference, 8);
            blockWriter.NewBlock(ByteLengthsReference, 8);
            blockWriter.NewBlock(StringsReference, 8);

            {
                blockWriter.OpenBlock(Reference);
                BinaryWriter.Write(blockWriter,StringTools.Encode_32_5('S','T','R','T','B'));
                BinaryWriter.Write(blockWriter,count);
                blockWriter.WriteDataBlockReference(HashesReference);
                blockWriter.WriteDataBlockReference(OffsetsReference);
                blockWriter.WriteDataBlockReference(RuneLengthsReference);
                blockWriter.WriteDataBlockReference(ByteLengthsReference);
                blockWriter.WriteDataBlockReference(StringsReference);
                blockWriter.CloseBlock();

                // String Hashes (160-bit -> 32-bit)
                blockWriter.OpenBlock(HashesReference);
                {
                    for (var i = 0; i < count; ++i)
                    {
                        BinaryWriter.Write(blockWriter,sortedHashes[i].Hash.AsHash32());
                    }
                }
                blockWriter.CloseBlock();

                // String Offsets
                blockWriter.OpenBlock(OffsetsReference);
                {
                    var offset = 0;
                    for (var i = 0; i < count; ++i)
                    {
                        var j = sortedHashes[i].Index;
                        BinaryWriter.Write(blockWriter,offset);
                        offset += _byteLengths[j];
                    }
                }
                blockWriter.CloseBlock();

                // String Rune Lengths
                blockWriter.OpenBlock(RuneLengthsReference);
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var j = sortedHashes[i].Index;
                        BinaryWriter.Write(blockWriter, _strings[j].Length);
                    }
                }
                blockWriter.CloseBlock();

                // String Byte Lengths
                blockWriter.OpenBlock(ByteLengthsReference);
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var j = sortedHashes[i].Index;
                        BinaryWriter.Write(blockWriter,_byteLengths[j]);
                    }
                }
                blockWriter.CloseBlock();

                // String Data
                blockWriter.OpenBlock(StringsReference);
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var j = sortedHashes[i].Index;
                        var utf8Len = _encoding.GetBytes(_strings[j], 0, _strings[j].Length, _utf8, 0);
                        _utf8[utf8Len] = 0; // Do include a terminating zero
                        BinaryWriter.Write(blockWriter, _utf8, 0, utf8Len + 1);
                    }
                }
                blockWriter.CloseBlock();
            }
        }
    }
}
