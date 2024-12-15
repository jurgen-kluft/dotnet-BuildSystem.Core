using System;
using DataBuildSystem;

namespace GameData
{
    public enum EGameDataPath : byte
    {
        GameDataSrcPath,
        GameDataGddPath,
        GameDataDstPath,
        GameDataPubPath,
    }

    public readonly struct GameDataPath
    {
        public static string BigFileExtension { get; set; }
        public static string BigFileTocExtension { get; set; }
        public static string BigFileFdbExtension { get; set; }
        public static string BigFileHdbExtension { get; set; }

        public const byte GameDataSrcPath = 0;
        public const byte GameDataGddPath = 1;
        public const byte GameDataDstPath = 2;
        public const byte GameDataPubPath = 3;

        public const byte GameDataScopeUnit = 0;
        public const byte GameDataScopeGlobal = 1;

        public const byte GameDataGduBigFileData = 0;
        public const byte GameDataGduBigFileToc = 1;
        public const byte GameDataGduBigFileFilenames = 2;
        public const byte GameDataGduBigFileHashes = 3;
        public const byte GameDataGduDataFileLog = 4;
        public const byte GameDataGameDataDll = 5;
        public const byte GameDataGameDataSignatureDb = 6;
        public const byte GameDataGameDataCppData = 7;
        public const byte GameDataGameDataCppCode = 8;
        public const byte GameDataSrcData = 9;
        public const byte GameDataDstData = 10;

        public EGameDataPath PathId { get; init; }
        public byte FileId { get; init; }
        public byte ScopeId { get; init; }

        public bool IsGameData => ScopeId == GameDataScopeGlobal;

        public string GetDirPath()
        {
            return (byte)PathId switch
            {
                GameDataSrcPath => BuildSystemConfig.SrcPath,
                GameDataGddPath => BuildSystemConfig.GddPath,
                GameDataDstPath => BuildSystemConfig.DstPath,
                GameDataPubPath => BuildSystemConfig.PubPath,
                _ => string.Empty
            };
        }

        public string GetFilePath(string name)
        {
            return Path.Join(GetDirPath(), name + GetFileExt());
        }

        public string GetRelativeFilePath(string name)
        {
            return name + GetFileExt();
        }

        private string GetFileExt()
        {
            return FileId switch
            {
                GameDataGduBigFileData => BigFileExtension,
                GameDataGduBigFileToc => BigFileTocExtension,
                GameDataGduBigFileFilenames => BigFileFdbExtension,
                GameDataGduBigFileHashes => BigFileHdbExtension,
                GameDataGduDataFileLog => ".dfl",
                GameDataGameDataDll => ".dll",
                GameDataGameDataSignatureDb => ".sdb",
                GameDataGameDataCppData => BigFileExtension,
                GameDataGameDataCppCode => ".h",
                _ => string.Empty
            };
        }

        public static readonly GameDataPath GameDataUnitBigFileData = new GameDataPath { PathId = EGameDataPath.GameDataPubPath, FileId = GameDataGduBigFileData, ScopeId = GameDataScopeUnit};
        public static readonly GameDataPath GameDataUnitBigFileToc = new GameDataPath { PathId = EGameDataPath.GameDataPubPath, FileId = GameDataGduBigFileToc, ScopeId = GameDataScopeUnit};
        public static readonly GameDataPath GameDataUnitBigFileFilenames = new GameDataPath { PathId = EGameDataPath.GameDataPubPath, FileId = GameDataGduBigFileFilenames, ScopeId = GameDataScopeUnit};
        public static readonly GameDataPath GameDataUnitBigFileHashes = new GameDataPath { PathId = EGameDataPath.GameDataPubPath, FileId = GameDataGduBigFileHashes, ScopeId = GameDataScopeUnit};
        public static readonly GameDataPath GameDataUnitDataFileLog = new GameDataPath { PathId = EGameDataPath.GameDataDstPath, FileId = GameDataGduDataFileLog, ScopeId = GameDataScopeUnit};
        public static readonly GameDataPath GameDataDll = new GameDataPath { PathId = EGameDataPath.GameDataGddPath, FileId = GameDataGameDataDll, ScopeId = GameDataScopeGlobal};
        public static readonly GameDataPath GameDataSignatureDb = new GameDataPath { PathId = EGameDataPath.GameDataDstPath, FileId = GameDataGameDataSignatureDb, ScopeId = GameDataScopeGlobal};
        public static readonly GameDataPath GameDataCppData = new GameDataPath { PathId = EGameDataPath.GameDataPubPath, FileId = GameDataGameDataCppData, ScopeId = GameDataScopeGlobal};
        public static readonly GameDataPath GameDataCppCode = new GameDataPath { PathId = EGameDataPath.GameDataPubPath, FileId = GameDataGameDataCppCode, ScopeId = GameDataScopeGlobal};
    }
}
