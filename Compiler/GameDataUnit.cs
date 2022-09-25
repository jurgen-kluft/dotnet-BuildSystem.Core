using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using GameCore;

namespace DataBuildSystem
{
    using Int8 = SByte;
    using UInt8 = Byte;

    public class GameDataUnits
    {
        private Dictionary<Hash160, Int32> HashToIndex = new Dictionary<Hash160, Int32>();

        public GameDataUnits() { }

        public void Save(string dstpath)
        {
            Hash160[] hashes = new Hash160[HashToIndex.Count];
            foreach (KeyValuePair<Hash160, Int32> item in HashToIndex)
            {
                hashes[item.Value] = item.Key;
            }
            BinaryFileWriter binaryfile = new BinaryFileWriter();
            binaryfile.Open(Path.Join(dstpath, "GameDataUnitsHash.db"));

            binaryfile.Write((Int32)hashes.Length);
            foreach (Hash160 hash in hashes)
            {
                hash.WriteTo(binaryfile);
            }

            binaryfile.Close();
        }

        public void Load(string dstpath)
        {
            BinaryFileReader binaryfile = new BinaryFileReader();
            binaryfile.Open(Path.Join(dstpath, "GameDataUnitsHash.db"));

            Int32 numHashes = binaryfile.ReadInt32();
            Hash160[] hashes = new Hash160[numHashes];
            for (int i = 0; i < numHashes; i++)
            {
                hashes[i] = Hash160.ReadFrom(binaryfile);
            }
            binaryfile.Close();

            HashToIndex = new Dictionary<Hash160, int>(numHashes);
            for (int i = 0; i < numHashes; i++)
            {
                HashToIndex.Add(hashes[i], i);
            }
        }
    }

    [Flags]
    public enum EGameDataUnit : Int32
    {
        GameDataDll = 0x0,
        GameDataCompilerLog = 0x1,
        GameDataData = 0x2,
        GameDataRelocation = 0x4,
        BigFileData = 0x8,
        BigFileToc = 0x10,
        BigFileFilenames = 0x20,
        BigFileHashes = 0x40,
    }

    public class GameDataUnit
    {
        public string Name { get; private set; }
        public string SubPath { get; private set; }
        public Hash160 Hash { get; private set; }
        public Int32 Index { get; private set; }
        private Int32 Units { get; set; }

        public bool IsUpToDate(EGameDataUnit u)
        {
            return (Units & (1 << (int)u)) != 0;
        }

        private GameDataUnit()
        {

        }

        public GameDataUnit(string name, Hash160 hash, Int32 index)
        {
            Name = name;
            Hash = hash;
            Index = index;
            Units = 0;
        }

        public void Verify(string gddpath, string dstpath)
        {
            Dependency dep = new Dependency(Dependency.EPath.Gdd, SubPath, Name + ".dll");
            if (!dep.Load())
            {
                Units = 0; // Nothing is up-to-date

                dep.Add((int)EGameDataUnit.GameDataCompilerLog, Dependency.EPath.Dst, SubPath, Name + ".gdcl");
                dep.Add((int)EGameDataUnit.GameDataData, Dependency.EPath.Dst, SubPath, Name + ".gdf");
                dep.Add((int)EGameDataUnit.GameDataRelocation, Dependency.EPath.Dst, SubPath, Name + ".gdr");
                dep.Add((int)EGameDataUnit.BigFileData, Dependency.EPath.Dst, SubPath, Name + ".bfd");
                dep.Add((int)EGameDataUnit.BigFileToc, Dependency.EPath.Dst, SubPath, Name + ".bft");
                dep.Add((int)EGameDataUnit.BigFileFilenames, Dependency.EPath.Dst, SubPath, Name + ".bff");
                dep.Add((int)EGameDataUnit.BigFileHashes, Dependency.EPath.Dst, SubPath, Name + ".bfh");
                dep.Save();
            }
            else
            {
                List<int> ids = new(8);
                if (dep.Update(ids) > 0)
                {
                    Units = 0;
                    foreach (int id in ids)
                    {
                        Units |= (1 << id);
                    }
                }
            }

            //       - Check if source files have changed
        }

        public void Save(IBinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(SubPath);
            Hash.WriteTo(writer);
            writer.Write(Index);
            writer.Write(Units);
        }
        
        public static GameDataUnit Load(IBinaryReader reader)
        {
            GameDataUnit gdu = new();
            gdu.Name = reader.ReadString();
            gdu.SubPath = reader.ReadString();
            gdu.Hash = Hash160.ReadFrom(reader);
            gdu.Index = reader.ReadInt32();
            gdu.Units = reader.ReadInt32();
        }
    }

    // TODO  Write up full design with all possible cases of data modification

    // A DataCompiler:
    //
    //     - Reads one or more source (input) files from srcpath
    //     - Processes those (can use external processes)
    //     - Writes resulting (output) files to destination (dstpath)
    //
    // Q: How to name and where to place the destination files?
    // A: Filename=HashOf(source filename), Extension can be used to distinguish many files (0000 to 9999?)
    //    So for one Data Compiler we only actually need to store the 'extensions'(number?) of the destination
    //    files in the stream, the path and filename is the same for each destination file.
    //
    // Q: Should we write the dependency information also in 'HashOf(source filename).dep'?
    // A: No, if we do, do we really need the 'Game Data Compiler Log' ?
    //    Yes, well the 'Game Data Compiler Log' is a very convenient way to do multi-core stream processing.
    //    We do not need to search for .dep files etc... and we do not need to keep opening, reading and
    //    closing those small files.

    // Need a database for DataUnit's that can map from Hash -> Index
    // DataUnits should be saved to and loaded from a BinaryFile 'GameDataUnits.slog'

    // A 'Game Data Unit' consists of (.GDU):
    //     - Name
    //     - Hash               (HashOf(filename of 'Game Data DLL'))
    //     - Index
    //     - State of 'Game Data DLL' (.DLL)
    //     - State of 'Game Data Compiler Log' (.GDL)
    //     - State of 'Game Data File' and 'Game Data Relocation File' (.GDF, .GDR)
    //     - State of 'Game Data Bigfile/TOC/Filename/Hashes' (.BFN, .BFH, .BFT, .BFD)

    // gddpath    = path with all the gamedata DLL's
    // srcpath    = path containing all the 'intermediate' assets (TGA, PGN, TRI, processed FBX files)
    // dstpath    = path containing all the 'cooked' assets and databases
    // pubpath    = path where all the 'Game Data' files and Bigfiles will be written (they are also written in the dstpath)

    // Collect all Game Data DLL's that need to be processed

    // There is a dependency on 'DataUnit.Index' for the generation of FileId's.
    // If this database is deleted then ALL Game Data and Bigfiles have to be regenerated.
    // The pollution of this database with stale items is ok, it does not impact memory usage.
    // It mainly results in empty bigfile sections, each of them being an offset of 4 bytes.


    //    If all up-to-date then done
    //    Else
    //       Case A:
    //           - 'BigFile Toc/Filename/Hash Files' is out-of-date/missing
    //           - Load 'Game Data Compiler Log'
    //           - Using 'Game Data Compiler Log' check if all 'source' files are up-to-date
    //             - If not up-to-date execute 'Game Data Compiler Log'
    //           - Build a database of Hash-FileId, sort Hashes and assign FileId
    //           - Load the 'Game Data DLL', inject with GameCore and GameCode
    //              - Find IDataRoot object
    //              - Instanciate the root object
    //              - Hand-out all the FileId's

    //       Case B:
    //           - 'Game Data DLL' is out-of-date
    //              - Load the 'Game Data DLL', inject with GameCore and GameCode
    //              - Find IDataRoot object
    //              - Instanciate the root object
    //              - Collect all IDataCompiler objects
    //              - Load 'Game Data Compiler Log'
    //                - See if there are any missing/added/changed IDataCompiler objects
    //                - So a IDataCompiler needs to build a unique Hash of itself!
    //                - Save 'Game Data Compiler Log'

    //       Case C:
    //           - 'Game Data Compiler Log' is out-of-date or missing
    //           - This is bad, we have lost our source to target dependency information
    //           - So we have to rebuild the log and all the data
    //           - And after that the 'Game Data File' and 'Game Data Relocation File' and
    //             'Game Data File' and 'Game Data Relocation File'.

    //       Case D:
    //           - 'Game Data File' and 'Game Data Relocation File' are out-of-date or missing
    //           - Using 'Game Data Compiler Log' check if all 'source' files are up-to-date
    //           - If any source file is out-of-date
    //             - Execute 'Game Data Compiler Log'
    //           - Build a database of Hash-FileId, sort Hashes and assign FileId
    //           - Load the 'Game Data DLL', inject with GameCore and GameCode
    //              - Find IDataRoot object
    //              - Instanciate the root object
    //              - Hand-out all the FileId's
    //              - Save 'Game Data File' and 'Game Data Relocation File'

    // Note: We could mitigate this by adding full dependency information as a file header of each target file.
}
