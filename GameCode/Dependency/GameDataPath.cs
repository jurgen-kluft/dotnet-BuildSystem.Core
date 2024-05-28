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
	public enum EGameData : Int32
	{
		GameDataDll = 0,
		GameDataCompilerLog = 1,
		GameDataData = 2,
		GameDataRelocation = 3,
		BigFileData = 4,
		BigFileToc = 5,
		BigFileFilenames = 6,
		BigFileHashes = 7,
		Count = 8,
	}

	public static class GameDataPath
	{
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

		private static EGameDataPath[] _sGameDataUnitToEPath = { EGameDataPath.Gdd, EGameDataPath.Dst, EGameDataPath.Dst, EGameDataPath.Dst, EGameDataPath.Pub, EGameDataPath.Pub, EGameDataPath.Pub, EGameDataPath.Pub };
		public static EGameDataPath GetPathFor(EGameData unit)
		{
			return _sGameDataUnitToEPath[(int)unit];
		}
		public static string GetExtFor(EGameData unit)
		{
			return unit switch
			{
				EGameData.GameDataDll => ".dll",
				EGameData.GameDataCompilerLog => ".gdl",
				EGameData.GameDataData => BuildSystemCompilerConfig.DataFileExtension,
				EGameData.GameDataRelocation => BuildSystemCompilerConfig.DataRelocFileExtension,
				EGameData.BigFileData => BigfileConfig.BigFileExtension,
				EGameData.BigFileToc => BigfileConfig.BigFileTocExtension,
				EGameData.BigFileFilenames => BigfileConfig.BigFileFdbExtension,
				EGameData.BigFileHashes => BigfileConfig.BigFileHdbExtension,
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
