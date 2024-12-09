using System.Text;

namespace GameCore
{
    public class StringTable
    {
        private readonly UTF8Encoding _encoding = new ();
        private readonly Dictionary<string, int> _dictionary;
        private readonly List<uint> _hashes;
        private readonly List<int> _byteLengths;
        private readonly List<StreamReference> _references;
        private readonly List<string> _strings;
        private byte[] _utf8;

        public StringTable(int estimatedNumberOfStrings = 4096, int longestUtf8StrLen = 8192)
        {
            _dictionary = new Dictionary<string, int>(estimatedNumberOfStrings);
            _hashes = new List<uint>(estimatedNumberOfStrings);
            _byteLengths = new List<int>(estimatedNumberOfStrings);
            _references = new List<StreamReference>(estimatedNumberOfStrings);
            _strings = new List<string>(estimatedNumberOfStrings);
            _utf8 = new byte[longestUtf8StrLen];

            Reference = StreamReference.NewReference;
            HashesReference = StreamReference.NewReference;
            OffsetsReference = StreamReference.NewReference;
            RuneLengthsReference  = StreamReference.NewReference;
            ByteLengthsReference  = StreamReference.NewReference;
            StringsReference = StreamReference.NewReference;
        }

        public StreamReference Reference { get;  }
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
            _references.Add(StreamReference.NewReference);
            _strings.Add(inString);

            var count = _encoding.GetByteCount(inString);
            if ((count + 1) > _utf8.Length)
            {
                _utf8 = new byte[_utf8.Length + (_utf8.Length / 4)];
            }

            var len = _encoding.GetBytes(inString, 0, inString.Length, _utf8, 0);
            _utf8[len] = 0; // Do include a terminating zero
            _byteLengths.Add(len + 1);
            var hash = Hashing.Compute(_utf8.AsSpan(0, len + 1));
            _hashes.Add(hash);

            return index;
        }


        public int ByteCountForIndex(int index)
        {
            return _byteLengths[index];
        }

        public StreamReference StreamReferenceForIndex(int index)
        {
            return _references[index];
        }

        private struct HashIndex
        {
            public uint Hash;
            public int Index;
        };

        public void Write(IDataWriter writer)
        {
            // Sort Hashes, HashIndex is used to keep track of the original index
            var sortedHashes = new List<HashIndex>(_hashes.Count);
            var count = 0;
            foreach (var hash in _hashes)
            {
                sortedHashes.Add(new HashIndex { Hash = hash, Index = count });
                count += 1;
            }
            sortedHashes.Sort((a, b) => a.Hash.CompareTo(b.Hash));

            // We need to precompute the size of the string table parts
            var hashesSize = count * sizeof(uint);
            var offsetsSize = count * sizeof(uint);
            var runeLengthsSize = count * sizeof(uint);
            var byteLengthsSize = count * sizeof(uint);

            var stringsSize = 0;
            foreach (var s in _byteLengths)
                stringsSize += s;

            const int mainSize = sizeof(int) + sizeof(int) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong);

            // Write StringTable

            writer.NewBlock(Reference, 8, mainSize);
            writer.NewBlock(HashesReference, 8, hashesSize);
            writer.NewBlock(OffsetsReference, 8, offsetsSize);
            writer.NewBlock(RuneLengthsReference, 8, runeLengthsSize);
            writer.NewBlock(ByteLengthsReference, 8, byteLengthsSize);
            writer.NewBlock(StringsReference, 8, stringsSize);

            {
                writer.OpenBlock(Reference);
                writer.Write(StringTools.Encode_32_5('S','T','R','T','B'));
                writer.Write(count);
                writer.WriteBlockReference(HashesReference);
                writer.WriteBlockReference(OffsetsReference);
                writer.WriteBlockReference(RuneLengthsReference);
                writer.WriteBlockReference(ByteLengthsReference);
                writer.WriteBlockReference(StringsReference);
                writer.CloseBlock();

                // String Hashes
                writer.OpenBlock(HashesReference);
                {
                    for (var i = 0; i < count; ++i)
                    {
                        writer.Write(sortedHashes[i].Hash);
                    }
                }
                writer.CloseBlock();

                // String Offsets
                writer.OpenBlock(OffsetsReference);
                {
                    var offset = 0;
                    for (var i = 0; i < count; ++i)
                    {
                        var j = sortedHashes[i].Index;
                        writer.Write(offset);
                        offset += _byteLengths[j];
                    }
                }
                writer.CloseBlock();

                // String Rune Lengths
                writer.OpenBlock(RuneLengthsReference);
                {
                    var offset = 0;
                    for (var i = 0; i < count; ++i)
                    {
                        var j = sortedHashes[i].Index;
                        writer.Write(offset);
                        offset += _strings[j].Length;
                    }
                }
                writer.CloseBlock();

                // String Byte Lengths
                writer.OpenBlock(ByteLengthsReference);
                {
                    var offset = 0;
                    for (var i = 0; i < count; ++i)
                    {
                        var j = sortedHashes[i].Index;
                        writer.Write(offset);
                        offset += _byteLengths[j];
                    }
                }
                writer.CloseBlock();

                // String Data
                writer.OpenBlock(StringsReference);
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var j = sortedHashes[i].Index;
                        var utf8Len = _encoding.GetBytes(_strings[j], 0, _strings[j].Length, _utf8, 0);
                        _utf8[utf8Len] = 0; // Do include a terminating zero
                        writer.Mark(_references[j]); // Mark a reference to the position in the data stream
                        writer.Write(_utf8, 0, utf8Len + 1);
                    }
                }
                writer.CloseBlock();
            }
        }
    }
}
