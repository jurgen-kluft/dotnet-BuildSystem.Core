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

        public GameDataUnits()
        {

        }

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
        public Hash160 Hash { get; private set; }
        public Int32 Index { get; private set; }
        public EGameDataUnit Units { get; private set; }

        public bool IsUpToDate(EGameDataUnit u)
        {
            return Units.HasFlag(u);
        }

        public GameDataUnit(string name, Hash160 hash, Int32 index)
        {
            Name = name;
            Hash = hash;
            Index = index;
            Units = 0;
        }



    }



    // TODO  Write up full design with all possible cases of data modification

    // A 'Game Data Unit' consists of (.GDU):
    //     - Unique Hash
    //     - Index
    //     - 'Game Data DLL' (.DLL)
    //     - 'Game Data Compiler Log' (.GDL)
    //     - 'Game Data File' and 'Game Data Relocation File' (.GDF, .GDR)
    //     - 'Game Data Bigfile/TOC/Filename/Hashes' (.BFN, .BFH, .BFT, .BFD)

    // gddpath    = path with all the gamedata DLL's
    // srcpath    = path containing all the 'intermediate' assets (TGA, PGN, TRI, processed FBX files)
    // dstpath    = path containing all the 'cooked' assets and databases
    // pubpath    = path where all the 'Game Data' files and Bigfiles will be written (they are also written in the dstpath)

    // Collect all Game Data DLL's that need to be processed

    // Need a database that can map from Hash -> Index
    // There is a dependency on this database by the generation of FileId's.
    // If this file is deleted then ALL Game Data and Bigfiles have to be regenerated.
    // The pollution of this database with stale items is ok, it does not impact memory usage.
    // It mainly results in empty bigfile sections, each of them being an offset of 4 bytes.

    // DataUnit can be saved and loaded from a BinaryFile
    // So we can create a DataUnits.slog file.

    //
    // Foreach 'Game Data DLL' construct/use-existing DataUnit
    //    Associated with a 'Game Data DLL'
    //    Generate the hash for the DataUnit (from the name of the DLL)
    //    
    //
    // Sort DataUnits by Hash
    // Foreach DataUnit
    //    Register the hash and get the index    
    //
    // Foreach DataUnit
    //    Check the date-time and size signature of:
    //       - 'Game Data DLL'
    //       - 'Game Data Compiler Log'
    //       - 'Game Data File' and 'Game Data Relocation File'
    //       - 'BigFile Toc/Filename/Hash Files'
    //       - Check if source files have changed

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
