using System.Text;

namespace GameCore
{
    public class StringTable
    {
        private readonly UTF8Encoding _encoding = new ();
        private readonly Dictionary<string, int> _dictionary;
        private readonly List<uint> _hashes;
        private readonly List<int> _lengths;
        private readonly List<StreamReference> _references;
        private readonly List<string> _strings;
        private readonly byte[] _utf8;

        public StringTable(int estimatedNumberOfStrings = 4096, int longestUtf8StrLen = 8192)
        {
            _dictionary = new Dictionary<string, int>(estimatedNumberOfStrings);
            _hashes = new List<uint>(estimatedNumberOfStrings);
            _lengths = new List<int>(estimatedNumberOfStrings);
            _references = new List<StreamReference>(estimatedNumberOfStrings);
            _strings = new List<string>(estimatedNumberOfStrings);
            _utf8 = new byte[longestUtf8StrLen];

            Reference = StreamReference.NewReference;
            HashesReference = StreamReference.NewReference;
            OffsetsReference = StreamReference.NewReference;
            StringsReference = StreamReference.NewReference;
        }

        public StreamReference Reference { get;  }
        private StreamReference  HashesReference { get;  }
        private StreamReference  OffsetsReference { get;  }
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
            if (count < (_utf8.Length - 1))
            {
                var len = _encoding.GetBytes(inString, 0, inString.Length, _utf8, 0);
                _lengths.Add(len + 1);

                var hash = Hashing.Compute(_utf8.AsSpan(0, len + 1));
                _hashes.Add(hash);
            }
            else
            {
                var bytes = new byte[count + 1];
                var len = _encoding.GetBytes(inString, 0, inString.Length, bytes, 0);
                _lengths.Add(len + 1);

                var hash = Hashing.Compute(bytes);
                _hashes.Add(hash);
            }

            return index;
        }


        public int LengthOfByIndex(int index)
        {
            return _lengths[index];
        }

        public StreamReference ReferenceOfByIndex(int index)
        {
            return _references[index];
        }

        private void SortByHash()
        {
            Dictionary<uint, string> hashToString = new();
            Dictionary<uint, int> hashToLength = new();
            Dictionary<uint, StreamReference> hashToReference = new();
            for (var i = 0; i < _strings.Count; ++i)
            {
                var hash = _hashes[i];
                var str = _strings[i];
                var r = _references[i];
                hashToString.Add(hash, str);
                hashToLength.Add(hash, _lengths[i]);
                hashToReference.Add(hash, r);
            }

            _hashes.Sort();

            _strings.Clear();
            _references.Clear();
            _lengths.Clear();
            foreach (var hash in _hashes)
            {
                hashToString.TryGetValue(hash, out var s);
                _strings.Add(s);
                hashToReference.TryGetValue(hash, out var r);
                _references.Add(r);
                hashToLength.TryGetValue(hash, out var l);
                _lengths.Add(l);
            }

            _dictionary.Clear();
            for (var i = 0; i < _strings.Count; ++i)
            {
                _dictionary.Add(_strings[i], i);
            }
        }

        public void Write(IDataWriter writer)
        {
            SortByHash();

            // We need to precompute the size of the string table parts
            var hashesSize = _hashes.Count * sizeof(uint);
            var offsetsSize = _references.Count * sizeof(uint);

            var stringsSize = 0;
            foreach (var s in _lengths)
                stringsSize += s;

            var mainSize = sizeof(int) + sizeof(int) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong);

            // Write StringTable

            writer.NewBlock(Reference, 8, mainSize);
            writer.NewBlock(HashesReference, 8, hashesSize);
            writer.NewBlock(OffsetsReference, 8, offsetsSize);
            writer.NewBlock(StringsReference, 8, stringsSize);

            writer.OpenBlock(Reference);
            {
                writer.Write(StringTools.Encode_32_5('S','T','R','T','B'));
                writer.Write(Count);
                writer.WriteBlockReference(HashesReference);
                writer.WriteBlockReference(OffsetsReference);
                writer.WriteBlockReference(StringsReference);
                writer.CloseBlock();

                // String Hashes
                writer.OpenBlock(HashesReference);
                {
                    for (var i = 0; i < _strings.Count; ++i)
                    {
                        var h = _hashes[i];
                        writer.Write(h);
                    }
                    writer.CloseBlock();
                }

                // String Offsets
                writer.OpenBlock(OffsetsReference);
                {
                    var offset = 0;
                    for (var i = 0; i < _strings.Count; ++i)
                    {
                        writer.Write(offset);
                        offset += _lengths[i];
                    }
                    writer.CloseBlock();
                }

                // String Data
                writer.OpenBlock(StringsReference);
                {
                    for (var i = 0; i < _strings.Count; ++i)
                    {
                        var s = _strings[i];
                        var r = _references[i];
                        var utf8Len = _encoding.GetBytes(s, 0, s.Length, _utf8, 0);
                        _utf8[utf8Len] = 0; // Do include a terminating zero
                        writer.Mark(r); // Mark a reference to the position in the data stream
                        writer.Write(_utf8, 0, utf8Len + 1);
                    }
                    writer.CloseBlock();
                }
            }
        }
    }
}
