using System;
using DataBuildSystem;

namespace GameData
{
	public enum EGameDataPath : byte
	{
		Src,
		Gdd,
		Dst,
		Pub,
	}

	[Flags]
	public enum EGameData : int
	{
		GameDataDll = 0,
        SignatureDatabase = 1,
		GameDataCompilerLog = 2,
		GameDataData = 3,
		BigFileData = 4,
		BigFileToc = 5,
		BigFileFilenames = 6,
		BigFileHashes = 7,
        GameCodeData = 8,
        GameCodeHeader = 9,
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
				EGameDataPath.Src => BuildSystemCompilerConfig.SrcPath,
				EGameDataPath.Gdd => BuildSystemCompilerConfig.GddPath,
				EGameDataPath.Dst => BuildSystemCompilerConfig.DstPath,
				EGameDataPath.Pub => BuildSystemCompilerConfig.PubPath,
				_ => string.Empty
			};
		}

        private static EGameDataPath[] _sGameDataUnitToEPath = [EGameDataPath.Gdd, EGameDataPath.Dst, EGameDataPath.Dst, EGameDataPath.Dst, EGameDataPath.Dst, EGameDataPath.Dst, EGameDataPath.Pub, EGameDataPath.Pub, EGameDataPath.Pub, EGameDataPath.Pub, EGameDataPath.Pub, EGameDataPath.Pub];
		public static EGameDataPath GetPathFor(EGameData unit)
		{
			return _sGameDataUnitToEPath[(int)unit];
		}
		public static string GetExtFor(EGameData unit)
		{
			return unit switch
			{
                EGameData.GameDataDll => ".dll",
                EGameData.SignatureDatabase => ".sdb",
				EGameData.GameDataCompilerLog => ".gdl",
				EGameData.GameDataData => BuildSystemCompilerConfig.DataFileExtension,
				EGameData.BigFileData => BigFileExtension,
				EGameData.BigFileToc => BigFileTocExtension,
				EGameData.BigFileFilenames => BigFileFdbExtension,
				EGameData.BigFileHashes => BigFileHdbExtension,
                EGameData.GameCodeData => ".gcd",
                EGameData.GameCodeHeader => ".h",
				_ => string.Empty
			};
		}
		public static string GetFilePathFor(string name, EGameData unit)
		{
			var path = GetPathFor(unit);
			var dirPath = GetPath(path);
			return Path.Join(dirPath, name + GetExtFor(unit));
		}
	}
}
