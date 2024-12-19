using System.Reflection;

using GameCore;
using GameData;

namespace DataBuildSystem
{
    public class SignatureDataBase : ISignatureDataBase
    {
        private class PrimaryEntry
        {
            public readonly List<uint> IndexList = new();
            public readonly List<Hash160> SignatureList = new();
        }

        private struct Entry
        {
            public uint Primary;
            public uint Secondary;
        }

        private readonly List<PrimaryEntry> _entries = new();
        private readonly Dictionary<Hash160, Entry> _signatureToEntry = new();

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

            var numSignatures = reader.ReadInt32();
            _signatureToEntry.EnsureCapacity(numSignatures);

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
                var primaryEntry = _entries[entryIndex];
                primaryEntry.SignatureList.Capacity = entryCapacity;
                primaryEntry.IndexList.Capacity = entryCapacity;
                var primaryIndex = (uint)entryIndex;
                for (var j = 0; j < entryCapacity; j++)
                {
                    var signature = Hash160.ReadFrom(reader);
                    var secondaryIndex = reader.ReadUInt32();
                    primaryEntry.SignatureList.Add(signature);
                    primaryEntry.IndexList.Add(secondaryIndex);
                    _signatureToEntry.Add(signature, new Entry { Primary = primaryIndex, Secondary = secondaryIndex });
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
                PrimaryEntry primaryEntry = _entries[i];
                writer.Write(i); // primaryIndex
                writer.Write(primaryEntry.IndexList.Count); // entryCapacity
                for (var j = 0; j < primaryEntry.SignatureList.Count; j++)
                {
                    primaryEntry.SignatureList[j].WriteTo(writer); // signature
                    writer.Write(primaryEntry.IndexList[j]); // secondaryIndex
                }
            }

            writer.Close();
            return true;
        }
    }
}
