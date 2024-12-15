using System.Reflection;

using GameCore;
using GameData;

namespace DataBuildSystem
{
    public class SignatureDataBase : ISignatureDataBase
    {
        private class PrimaryEntry
        {
            public readonly List<uint> IndexList = [];
            public readonly List<Hash160> SignatureList = [];
        }

        private struct Entry
        {
            public uint Primary;
            public uint Secondary;
        }

        private readonly List<PrimaryEntry> _entries = [];
        private readonly Dictionary<Hash160, Entry> _signatureToEntry = [];

        public (uint primary, uint secondary) GetEntry(Hash160 signature)
        {
            if (_signatureToEntry.TryGetValue(signature, out var e))
                return (e.Primary, e.Secondary);
            return (uint.MaxValue, uint.MaxValue);
        }

        public bool Register(Hash160 signature, uint primary, uint secondary)
        {
            if (_signatureToEntry.TryGetValue(signature, out var e))
                return false;

            if (primary > _entries.Capacity)
                _entries.Capacity = (int)primary + 16;
            while (primary >= _entries.Count)
                _entries.Add(new PrimaryEntry());

            _signatureToEntry.Add(signature, new Entry { Primary = primary, Secondary = secondary });
            PrimaryEntry bfe = _entries[(int)primary];
            bfe.SignatureList.Add(signature);
            bfe.IndexList.Add(secondary);
            return true; // Success registering this signature
        }

        public void RemovePrimary(uint index)
        {
            if (index < _entries.Count)
            {
                PrimaryEntry bfe = _entries[(int)index];
                foreach (var signature in bfe.SignatureList)
                    _signatureToEntry.Remove(signature);
                bfe.SignatureList.Clear();
                bfe.IndexList.Clear();
            }
        }

        public bool Load(string filepath)
        {
            if (!File.Exists(filepath))
                return false;

            var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            var reader = ArchitectureUtils.CreateBinaryFileReader(fileStream, LocalizerConfig.Platform);

            _signatureToEntry.Clear();
            _entries.Clear();

            _signatureToEntry.EnsureCapacity(reader.ReadInt32());

            var numEntries = reader.ReadInt32();
            _entries.Capacity = numEntries;
            for (var i = 0; i < numEntries; i++)
            {
                _entries.Add(new PrimaryEntry());
            }

            for (var i = 0; i < numEntries; i++)
            {
                var entryIndex = reader.ReadInt32();
                var entryCapacity = reader.ReadInt32();
                var bfe = _entries[entryIndex];
                bfe.SignatureList.Capacity = entryCapacity;
                bfe.IndexList.Capacity = entryCapacity;
                for (var j = 0; j < entryCapacity; j++)
                {
                    var signature = Hash160.ReadFrom(reader);
                    var bigfileIndex = reader.ReadUInt32();
                    var fileIndex = reader.ReadUInt32();
                    bfe.SignatureList.Add(signature);
                    bfe.IndexList.Add(fileIndex);

                    _signatureToEntry.Add(signature, new Entry { Primary = bigfileIndex, Secondary = fileIndex });
                }
            }

            reader.Close();
            return true;
        }

        public bool Save(string filepath)
        {
            var writer = ArchitectureUtils.CreateBinaryFileWriter(filepath, LocalizerConfig.Platform);
            if (writer == null) return false;

            writer.Write(_signatureToEntry.Count);

            writer.Write(_entries.Count);
            for (var i = 0; i < _entries.Count; i++)
            {
                PrimaryEntry bfe = _entries[i];
                writer.Write(i);
                writer.Write(bfe.IndexList.Count);
                for (var j = 0; j < bfe.SignatureList.Count; j++)
                {
                    bfe.SignatureList[j].WriteTo(writer);
                    writer.Write(i);
                    writer.Write(bfe.IndexList[j]);
                }
            }

            writer.Close();
            return true;
        }
    }
}
