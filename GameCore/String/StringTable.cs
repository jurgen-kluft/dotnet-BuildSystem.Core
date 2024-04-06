using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public class StringTable
    {
        #region Fields

        private readonly UTF8Encoding _encoding = new UTF8Encoding();
        private readonly Dictionary<string, int> _dictionary = new();
        private readonly List<uint> _hashes = new();
        private readonly List<int> _lengths = new();
        private readonly List<StreamReference> _references = new();
        private readonly List<string> _strings = new();

        #endregion

        #region Properties

        public StreamReference Reference { get; set; }

        public string this[int index] => _strings[index];

        public int Count => _strings.Count;

        #endregion

        #region Public Methods

        public int Add(string inString)
        {
            if (_dictionary.TryGetValue(inString, out var index)) return index;
            index = _strings.Count;

            _dictionary.Add(inString, index);
            _references.Add(StreamReference.NewReference);
            _strings.Add(inString);

            var utf8 = _encoding.GetBytes(inString);
            _lengths.Add(utf8.Length + 1);

            var hash = Hashing.Compute(utf8);
            _hashes.Add(hash);

            return index;
        }

        public byte[] GetBytes(string inString)
        {
            return _encoding.GetBytes(inString);
        }

        private int InternalIndexOf(string inString)
        {
            var index = _dictionary.GetValueOrDefault(inString, -1);
            return index;
        }

        private int IndexOf(string inString)
        {
            return InternalIndexOf(inString);
        }

        public uint HashOf(string inString)
        {
            var index = IndexOf(inString);
            return index == -1 ? uint.MaxValue : _hashes[index];
        }

        public uint LengthOf(string inString)
        {
            var index = IndexOf(inString);
            return index == -1 ? 0 : (uint)_lengths[index];
        }

        public int LengthOfByIndex(int index)
        {
            return _lengths[index];
        }

        private StreamReference InternalReferenceOf(string inString)
        {
            var index = InternalIndexOf(inString);
            return index == -1 ? StreamReference.Empty : _references[index];
        }

        public StreamReference ReferenceOf(string inString)
        {
            var str = inString;
            return InternalReferenceOf(str);
        }

        public void SortByHash()
        {
            Dictionary<uint, string> hashToString = new();
            Dictionary<uint, int> hashToLength = new();
            Dictionary<uint, StreamReference> hashToReference = new();
            for (var i = 0; i < _strings.Count; ++i)
            {
                var hash = _hashes[i];
                var str = _strings[i];
                hashToString.Add(hash, str);
                hashToLength.Add(hash, _lengths[i]);
                hashToReference.Add(hash, InternalReferenceOf(str));
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

        public void Write(IBinaryStreamWriter writer, Dictionary<StreamReference, StreamOffset> dataOffsetDataBase)
        {
            SortByHash();

            StreamOffset offset = new(writer.Position);

            // Need to determine some good size
            var utf8 = new byte[8192];

            // Write strings and assign them a StreamReference and StreamOffset
            foreach (var s in _strings)
            {
                var r = InternalReferenceOf(s);
                dataOffsetDataBase.Add(r, offset);

                var utf8Len = _encoding.GetBytes(s, 0, s.Length, utf8, 0);
                utf8[utf8Len] = 0; // Do include a terminating zero
                writer.Write(utf8, 0, utf8Len + 1);

                offset += utf8Len + 1;
            }
        }

        public void Write(IDataWriter writer)
        {
            SortByHash();

            // Write StringTable
            writer.BeginBlock(Reference, sizeof(int));
            {
                var hashesReference = StreamReference.NewReference;
                var referencesReference = StreamReference.NewReference;
                var stringsReference = StreamReference.NewReference;

                writer.Write(Count);
                writer.Write(hashesReference);
                writer.Write(referencesReference);

                // String hashes
                writer.BeginBlock(hashesReference, sizeof(int));
                {
                    foreach (var s in _hashes)
                        writer.Write(s);
                    writer.EndBlock();
                }

                // String References
                writer.BeginBlock(referencesReference, sizeof(int));
                {
                    foreach (var s in _strings)
                    {
                        var r = InternalReferenceOf(s);
                        writer.Write(r);
                    }
                }

                // String Data
                writer.BeginBlock(stringsReference, sizeof(int));
                {
                    foreach (var s in _strings)
                    {
                        var r = InternalReferenceOf(s);
                        if (!writer.BeginBlock(r, sizeof(int))) continue;
                        writer.Write(s);
                        writer.EndBlock();
                    }
                }
            }
            writer.EndBlock();
        }

        #endregion
    }
}
