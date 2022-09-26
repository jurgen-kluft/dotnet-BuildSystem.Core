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
        private List<GameDataUnit> DataUnits = new List<GameDataUnit>();

        public GameDataUnits() { }

        public void Save(string dstpath)
        {
            BinaryFileWriter binaryfile = new BinaryFileWriter();
            binaryfile.Open(Path.Join(dstpath, "GameDataUnits.log"));

            binaryfile.Write((Int32)DataUnits.Count);
            foreach (GameDataUnit gdu in DataUnits)
            {
                gdu.Save(binaryfile);
            }

            binaryfile.Close();
        }

        public void Update(string srcpath, string dstpath)
        {
            // Foreach DataUnit that is out-of-date or missing
            foreach (GameDataUnit gdu in DataUnits)
            {
                bool gduGameDataDll = gdu.IsUpToDate(EGameDataUnit.GameDataDll);
                bool gduCompilerLog = gdu.IsUpToDate(EGameDataUnit.GameDataCompilerLog);
                bool gduGameDataData = gdu.IsUpToDate(EGameDataUnit.GameDataData, EGameDataUnit.GameDataRelocation);
                bool gduBigfile = gdu.IsUpToDate(EGameDataUnit.BigFileData, EGameDataUnit.BigFileToc, EGameDataUnit.BigFileFilenames, EGameDataUnit.BigFileHashes);

                //       Case A:
                //           - 'Game Data DLL' is out-of-date
                //              - Load the 'Game Data DLL'
                //              - Find IDataRoot object
                //              - Instanciate the root object
                //              - Collect all IDataCompiler objects
                //              - Load 'Game Data Compiler Log'
                //                - See if there are any missing/added/changed IDataCompiler objects
                //                - So a IDataCompiler needs to build a unique Hash of itself!
                //                - Save 'Game Data Compiler Log'
                //              - If 'Game Data Compiler Log' was identical -> Case B or Case D
                //              - Else -> Case C
                if (!gduGameDataDll)
                {


                }

                //       Case B:
                //           - 'BigFile Toc/Filename/Hash Files' is out-of-date/missing
                //           - Load 'Game Data Compiler Log'
                //           - Using 'Game Data Compiler Log' check if all 'source' and 'destination' files are up-to-date
                //             - If not up-to-date execute 'Game Data Compiler Log'
                //           - Build a database of Hash-FileId, sort Hashes and assign FileId
                //           - Load the 'Game Data DLL'
                //              - Find IDataRoot object
                //              - Instanciate the root object
                //              - Hand-out all the FileId's
                //              - Save 'Game Data File' and 'Game Data Relocation File'
                //              - Save 'BigFile Toc/Filename/Hash Files'
                if (!gduBigfile)
                {


                }

                //       Case C:
                //           - 'Game Data Compiler Log' is out-of-date or missing
                //           - This is bad, we have lost our source to target dependency information
                //           - So we have to rebuild this Compiler Log and Cook all the data
                //           - Build a database of Hash-FileId, sort Hashes and assign FileId
                //           - Load the 'Game Data DLL'
                //              - Find IDataRoot object
                //              - Instanciate the root object
                //              - Hand-out all the FileId's
                //              - Save 'Game Data File' and 'Game Data Relocation File'
                //              - Save 'BigFile Toc/Filename/Hash Files'
                if (!gduCompilerLog)
                {


                }

                //       Case D:
                //           - 'Game Data File' and 'Game Data Relocation File' are out-of-date or missing
                //           - Using 'Game Data Compiler Log' check if all 'source' files are up-to-date
                //           - If any source file is out-of-date
                //             - Execute 'Game Data Compiler Log'
                //           - Build a database of Hash-FileId, sort Hashes and assign FileId
                //           - Load the 'Game Data DLL',
                //              - Find IDataRoot object
                //              - Instanciate the root object
                //              - Hand-out all the FileId's
                //              - Save 'Game Data File' and 'Game Data Relocation File'
                //              - Save 'BigFile Toc/Filename/Hash Files'
                if (!gduGameDataData)
                {


                }


                // - cook
                // - save all output (compiler log, gamedata, bigfile)
            }
        }

        public void Load(string dstpath, string gddpath)
        {
            // Scan the gddpath folder for all game data .dll's
            Dictionary<Hash160, string> hashToPath = new();
            foreach (var path in GameCore.DirUtils.EnumerateFiles(gddpath, "*.dll", SearchOption.TopDirectoryOnly))
            {
                string filepath = path.RelativePath(gddpath.Length).Span.ToString();
                Hash160 hash = HashUtility.Compute_UTF8(filepath);
                hashToPath.Add(hash, filepath);
            }

            List<int> indices = new List<int>();
            for (int i = 0; i < (2 * hashToPath.Count); i++)
                indices.Add(0);

            BinaryFileReader binaryfile = new BinaryFileReader();
            if (binaryfile.Open(Path.Join(dstpath, "GameDataUnits.log")))
            {
                Int32 numUnits = binaryfile.ReadInt32();
                DataUnits = new List<GameDataUnit>(numUnits);

                for (int i = 0; i < numUnits; i++)
                {
                    GameDataUnit gdu = GameDataUnit.Load(binaryfile);

                    // Is this one still in the list of .dll's?
                    if (hashToPath.ContainsKey(gdu.Hash))
                    {
                        indices.Remove(gdu.Index);
                        hashToPath.Remove(gdu.Hash);
                        DataUnits.Add(gdu);
                    }
                }
                binaryfile.Close();
            }

            // Any new DataUnit's -> create them!
            foreach (var item in hashToPath)
            {
                GameDataUnit gdu = new(item.Value, indices[0]);
                indices.RemoveAt(0);

                DataUnits.Add(gdu);
            }
        }
    }

    [Flags]
    public enum EGameDataUnit : Int32
    {
        GameDataDll = 0,
        GameDataCompilerLog = 1,
        GameDataData = 2,
        GameDataRelocation = 3,
        BigFileData = 4,
        BigFileToc = 5,
        BigFileFilenames = 6,
        BigFileHashes = 7,
        SourceFiles = 8,
        All = 0x1FF,
    }

    /// e.g.
    /// FilePath: GameData.Fonts.dll
    /// Index: 1
    /// Units: 0

    public class GameDataUnit
    {
        public string FilePath { get; private set; }
        public Hash160 Hash { get; set; }
        public Int32 Index { get; set; }
        private Int32 Units { get; set; }

        public bool IsUpToDate(EGameDataUnit u)
        {
            return (Units & (1 << (int)u)) == 0;
        }
        public bool IsUpToDate(params EGameDataUnit[] pu)
        {
            Int32 u = 0;
            foreach (var item in pu)
            {
                u |= 1 << (Int32)item;
            }

            return (Units & (1 << (int)u)) == 0;
        }

        private GameDataUnit() { }

        public GameDataUnit(string filepath, Int32 index)
        {
            FilePath = filepath;
            Hash = HashUtility.Compute_UTF8(FilePath);
            Index = index;
            Units = (int)EGameDataUnit.All;
        }

        public void Verify(string gddpath, string dstpath)
        {
            Dependency dep = new Dependency(Dependency.EPath.Gdd, FilePath);
            if (dep.Load())
            {
                Int32 outofdate = 0;

                List<int> outofdate_ids = new(8);
                if (!IsUpToDate(EGameDataUnit.SourceFiles))
                    outofdate_ids.Add((int)EGameDataUnit.SourceFiles);

                if (dep.Update(outofdate_ids) > 0)
                {
                    foreach (int id in outofdate_ids)
                    {
                        outofdate |= (1 << id);
                    }
                }
                Units = (Units & outofdate) | outofdate;
            }
            else
            {
                Units = (int)EGameDataUnit.All; // Nothing is up-to-date

                dep.Add(
                    (int)EGameDataUnit.GameDataCompilerLog,
                    Dependency.EPath.Dst,
                    Path.ChangeExtension(FilePath, ".gdcl")
                );
                dep.Add(
                    (int)EGameDataUnit.GameDataData,
                    Dependency.EPath.Dst,
                    Path.ChangeExtension(FilePath, ".gdf")
                );
                dep.Add(
                    (int)EGameDataUnit.GameDataRelocation,
                    Dependency.EPath.Dst,
                    Path.ChangeExtension(FilePath, ".gdr")
                );
                dep.Add(
                    (int)EGameDataUnit.BigFileData,
                    Dependency.EPath.Dst,
                    Path.ChangeExtension(FilePath, ".bfd")
                );
                dep.Add(
                    (int)EGameDataUnit.BigFileToc,
                    Dependency.EPath.Dst,
                    Path.ChangeExtension(FilePath, ".bft")
                );
                dep.Add(
                    (int)EGameDataUnit.BigFileFilenames,
                    Dependency.EPath.Dst,
                    Path.ChangeExtension(FilePath, ".bff")
                );
                dep.Add(
                    (int)EGameDataUnit.BigFileHashes,
                    Dependency.EPath.Dst,
                    Path.ChangeExtension(FilePath, ".bfh")
                );
                dep.Save();
            }
        }

        public void VerifySource()
        {
            // Check if source files have changed, a change in any source file will have to be handled
            // by executing the DataCompiler to build up-to-date destination files.

            // So we need to use ActorFlow to stream the CompilerLog and each DataCompiler needs to check if
            // its source file(s) are up-to-date.
            Units = Units | (int)EGameDataUnit.SourceFiles;
        }

        public void Save(IBinaryWriter writer)
        {
            writer.Write(FilePath);
            Hash.WriteTo(writer);
            writer.Write(Index);
            writer.Write(Units);
        }

        public static GameDataUnit Load(IBinaryReader reader)
        {
            GameDataUnit gdu = new();
            gdu.FilePath = reader.ReadString();
            gdu.Hash = Hash160.ReadFrom(reader);
            gdu.Index = reader.ReadInt32();
            gdu.Units = reader.ReadInt32();
            return gdu;
        }
    }

    // TODO  Write up full design with all possible cases of data modification

    // A DataCompiler:
    //
    //     - Reads one or more source (input) files from srcpath
    //     - Processes those (can use external processes, e.g. 'dxc.exe')
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


    // Note: We could mitigate risks by adding full dependency information as a file header of each target file, or still
    //       have each DataCompiler write a .dep file to the destination.
}
