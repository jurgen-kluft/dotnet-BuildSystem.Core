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

		private static EGameDataPath[] sGameDataUnitToEPath = { EGameDataPath.Gdd, EGameDataPath.Dst, EGameDataPath.Dst, EGameDataPath.Dst, EGameDataPath.Pub, EGameDataPath.Pub, EGameDataPath.Pub, EGameDataPath.Pub };
		public static EGameDataPath GetPathFor(EGameData unit)
		{
			return sGameDataUnitToEPath[(int)unit];
		}
		private static string[] sGameDataUnitToExtension = { ".dll", ".gdcl", ".gdd", ".gdr", ".bfd", ".bft", ".bff", ".bfh" };
		public static string GetExtFor(EGameData unit)
		{
			return sGameDataUnitToExtension[(int)unit];
		}
		public static string GetFilePathFor(string name, EGameData unit)
		{
			EGameDataPath path = GetPathFor(unit);
			string dirpath = GetPath(path);
			return Path.Join(dirpath, name + GetExtFor(unit));
		}
	}
}
