using System;
using DataBuildSystem;

namespace GameData
{
	public enum EGameDataPath : ushort
	{
		Src=0x0000,
		Gdd=0x1000,
		Dst=0x2000,
		Pub=0x3000,
	}

    public enum EGameDataScope : ushort
    {
        DataUnit=0x0000,
        GameData=0x0100
    }

    [Flags]
    public enum EGameData : ushort
    {
        GduBigFileData = EGameDataPath.Pub | EGameDataScope.DataUnit | 0,
        GduBigFileToc = EGameDataPath.Pub | EGameDataScope.DataUnit | 1,
        GduBigFileFilenames = EGameDataPath.Pub | EGameDataScope.DataUnit | 2,
        GduBigFileHashes = EGameDataPath.Pub | EGameDataScope.DataUnit | 3,
        GduDataFileLog = EGameDataPath.Dst | EGameDataScope.DataUnit | 4,
        GameDataCppData = EGameDataPath.Pub | EGameDataScope.GameData | 5,
        GameDataCppCode = EGameDataPath.Pub | EGameDataScope.GameData | 6,
        GameDataSignatureDb = EGameDataPath.Dst | EGameDataScope.GameData | 7,
        GameDataDll = EGameDataPath.Gdd | EGameDataScope.GameData | 8,
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
            var path = (uint)unit & 0xF000;
            return (EGameDataPath)path;
		}

        public static bool IsGameData(EGameData e)
        {
            return ((uint)e & 0x0F00) == (uint)EGameDataScope.GameData;
        }

		public static string GetExtFor(EGameData unit)
		{
			return unit switch
			{
				EGameData.GduBigFileData => BigFileExtension,
				EGameData.GduBigFileToc => BigFileTocExtension,
				EGameData.GduBigFileFilenames => BigFileFdbExtension,
				EGameData.GduBigFileHashes => BigFileHdbExtension,
                EGameData.GduDataFileLog => ".dfl",
                EGameData.GameDataDll => ".dll",
                EGameData.GameDataSignatureDb => ".sdb",
                EGameData.GameDataCppData => BigFileExtension,
                EGameData.GameDataCppCode => ".h",
				_ => string.Empty
			};
		}
		private static string GetFilePathFor(string name, EGameData unit)
		{
			var path = GetPathFor(unit);
			var dirPath = GetPath(path);
			return Path.Join(dirPath, name + GetExtFor(unit));
		}

        public static string GduBigFileData(string name)
        {
            return GetFilePathFor(name, EGameData.GduBigFileData);
        }
        public static string GduBigFileToc(string name)
        {
            return GetFilePathFor(name, EGameData.GduBigFileToc);
        }
        public static string GduBigFileFilenames(string name)
        {
            return GetFilePathFor(name, EGameData.GduBigFileFilenames);
        }
        public static string GduBigFileHashes(string name)
        {
            return GetFilePathFor(name, EGameData.GduBigFileHashes);
        }
        public static string GduDataFileLog(string name)
        {
            return GetFilePathFor(name, EGameData.GduDataFileLog);
        }
        public static string GameDataDll(string name)
        {
            return GetFilePathFor(name, EGameData.GameDataDll);
        }
        public static string GameDataSignatureDb(string name)
        {
            return GetFilePathFor(name, EGameData.GameDataSignatureDb);
        }
        public static string GameDataCppData(string name)
        {
            return GetFilePathFor(name, EGameData.GameDataCppData);
        }
        public static string GameDataCppCode(string name)
        {
            return GetFilePathFor(name, EGameData.GameDataCppCode);
        }
    }
}
