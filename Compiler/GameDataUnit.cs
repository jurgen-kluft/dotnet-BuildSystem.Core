using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using GameCore;

namespace DataBuildSystem
{
    using Int8 = SByte;
    using UInt8 = Byte;

    [Flags]
    public enum EGameDataUnit : Int32
    {
        GameDataDll,
        GameDataCompilerLog,
        GameDataData,
        GameDataRelocation,
        BigFileData,
        BigFileToc,
        BigFileFilenames,
        BigFileHashes,
    }

    public class GameDataUnit
    {
        public Hash160 Hash { get; set; }
        public Int32 Index { get; set; }

        public bool IsUpToDate(EGameDataUnit u)
        {
            return false;
        }
    }
}
