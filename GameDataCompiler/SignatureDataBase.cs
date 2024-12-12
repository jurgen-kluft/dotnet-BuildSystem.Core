using System.Reflection;

using GameCore;
using GameData;

namespace DataBuildSystem
{
    public class SignatureDataBase : ISignatureDataBase
    {
        private class BigfileEntry
        {
            public List<uint> FileIndexList = new();
            public List<Hash160> SignatureList = new();
        }

        private struct Entry
        {
            public uint BigfileIndex;
            public uint FileIndex;
        }

        private readonly List<BigfileEntry> _entries = [];
        private readonly Dictionary<Hash160, Entry> _signatureToEntry = [];

        public (uint bigfileIndex, uint fileIndex) GetFileId(Hash160 signature)
        {
            if (_signatureToEntry.TryGetValue(signature, out var e))
                return (e.BigfileIndex, e.FileIndex);
            return (uint.MaxValue, uint.MaxValue);
        }

        public bool Register(Hash160 signature, uint bigfileIndex, uint fileIndex)
        {
            if (_signatureToEntry.TryGetValue(signature, out var e))
                return false;

            if (bigfileIndex > _entries.Capacity)
                _entries.Capacity = (int)bigfileIndex + 16;
            while (bigfileIndex >= _entries.Count)
                _entries.Add(new BigfileEntry());

            _signatureToEntry.Add(signature, new Entry { BigfileIndex = bigfileIndex, FileIndex = fileIndex });
            BigfileEntry bfe = _entries[(int)bigfileIndex];
            bfe.SignatureList.Add(signature);
            bfe.FileIndexList.Add(fileIndex);
            return true; // Success registering this signature
        }

        public void RemoveBigfile(uint index)
        {
            if (index < _entries.Count)
            {
                BigfileEntry bfe = _entries[(int)index];
                foreach (var signature in bfe.SignatureList)
                    _signatureToEntry.Remove(signature);
                bfe.SignatureList.Clear();
                bfe.FileIndexList.Clear();
            }
        }

        public bool Load(string filepath)
        {
            BinaryFileReader reader = new();
            if (!reader.Open(filepath))
                return false;

            _signatureToEntry.Clear();
            _entries.Clear();

            _signatureToEntry.EnsureCapacity(reader.ReadInt32());

            var numEntries = reader.ReadInt32();
            _entries.Capacity = numEntries;
            for (var i = 0; i < numEntries; i++)
            {
                _entries.Add(new BigfileEntry());
            }

            for (var i = 0; i < numEntries; i++)
            {
                var entryIndex = reader.ReadInt32();
                var entryCapacity = reader.ReadInt32();
                var bfe = _entries[entryIndex];
                bfe.SignatureList.Capacity = entryCapacity;
                bfe.FileIndexList.Capacity = entryCapacity;
                for (var j = 0; j < entryCapacity; j++)
                {
                    var signature = Hash160.ReadFrom(reader);
                    var bigfileIndex = reader.ReadUInt32();
                    var fileIndex = reader.ReadUInt32();
                    bfe.SignatureList.Add(signature);
                    bfe.FileIndexList.Add(fileIndex);

                    _signatureToEntry.Add(signature, new Entry { BigfileIndex = bigfileIndex, FileIndex = fileIndex });
                }
            }

            reader.Close();
            return true;
        }

        public bool Save(string filepath)
        {
            var writer = ArchitectureUtils.CreateBinaryWriter(filepath, LocalizerConfig.Platform);
            if (writer == null) return false;

            writer.Write(_signatureToEntry.Count);

            writer.Write(_entries.Count);
            for (var i = 0; i < _entries.Count; i++)
            {
                BigfileEntry bfe = _entries[i];
                writer.Write(i);
                writer.Write(bfe.FileIndexList.Count);
                for (var j = 0; j < bfe.SignatureList.Count; j++)
                {
                    bfe.SignatureList[j].WriteTo(writer);
                    writer.Write(i);
                    writer.Write(bfe.FileIndexList[j]);
                }
            }

            writer.Close();
            return true;
        }
    }
}
