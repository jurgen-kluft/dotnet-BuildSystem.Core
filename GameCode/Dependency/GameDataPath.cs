using System;
using DataBuildSystem;

namespace GameData
{
	public enum EGameDataPath : uint
	{
		Src=0x00000,
		Gdd=0x10000,
		Dst=0x20000,
		Pub=0x30000,
	}

    public enum EGameDataScope : uint
    {
        DataUnit=0x000,
        GameData=0x100
    }

    [Flags]
    public enum EGameData : uint
    {
        GduBigFileData = EGameDataPath.Pub | EGameDataScope.DataUnit | 0,
        GduBigFileToc = EGameDataPath.Pub | EGameDataScope.DataUnit | 1,
        GduBigFileFilenames = EGameDataPath.Pub | EGameDataScope.DataUnit | 2,
        GduBigFileHashes = EGameDataPath.Pub | EGameDataScope.DataUnit | 3,
        GameDataCompilerLog = EGameDataPath.Dst | EGameDataScope.GameData | 0,
        GameDataCppData = EGameDataPath.Pub | EGameDataScope.GameData | 1,
        GameDataCppCode = EGameDataPath.Pub | EGameDataScope.GameData | 2,
        GameDataDll = EGameDataPath.Gdd | EGameDataScope.GameData | 3,
        GameDataSignatureDb = EGameDataPath.Dst | EGameDataScope.GameData | 4,
    }

    public static class GameDataPath
	{
        public static string BigFileExtension { get; set; }
        public static string BigFileTocExtension { get; set; }
        public static string BigFileFdbExtension { get; set; }
        public static string BigFileHdbExtension { get; set; }

		public static string GetPath(EGameDataPath p)
		{
			return p switch
			{
				EGameDataPath.Src => BuildSystemConfig.SrcPath,
				EGameDataPath.Gdd => BuildSystemConfig.GddPath,
				EGameDataPath.Dst => BuildSystemConfig.DstPath,
				EGameDataPath.Pub => BuildSystemConfig.PubPath,
				_ => string.Empty
			};
		}

        public static EGameDataPath GetPathFor(EGameData unit)
		{
            var path = (uint)unit & 0xFF0000;
            return (EGameDataPath)path;
		}

        public static bool IsGameData(EGameData e)
        {
            return ((uint)e & 0xFF00) == (uint)EGameDataScope.GameData;
        }

		public static string GetExtFor(EGameData unit)
		{
			return unit switch
			{
				EGameData.GduBigFileData => BigFileExtension,
				EGameData.GduBigFileToc => BigFileTocExtension,
				EGameData.GduBigFileFilenames => BigFileFdbExtension,
				EGameData.GduBigFileHashes => BigFileHdbExtension,
                EGameData.GameDataDll => ".dll",
                EGameData.GameDataSignatureDb => ".sdb",
                EGameData.GameDataCompilerLog => ".gdcl",
                EGameData.GameDataCppData => BigFileExtension,
                EGameData.GameDataCppCode => ".h",
				_ => string.Empty
			};
		}
		public static string GetFilePathFor(string name, EGameData unit)
		{
			var path = GetPathFor(unit);
			var dirPath = GetPath(path);
			return Path.Join(dirPath, name + GetExtFor(unit));
		}

        EGameData.GduBigFileData => BigFileExtension,
        EGameData.GduBigFileToc => BigFileTocExtension,
        EGameData.GduBigFileFilenames => BigFileFdbExtension,
        EGameData.GduBigFileHashes => BigFileHdbExtension,
        EGameData.GameDataDll => ".dll",
        EGameData.GameDataSignatureDb => ".sdb",
        EGameData.GameDataCompilerLog => ".gdcl",
        EGameData.GameDataCppData => BigFileExtension,
        EGameData.GameDataCppCode => ".h",

    }
}
