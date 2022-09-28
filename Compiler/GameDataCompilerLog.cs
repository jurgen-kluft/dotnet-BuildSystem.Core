using System;
using GameData;
using GameCore;

namespace DataBuildSystem
{
	public struct Result
	{
		private enum ResultEnum : int
		{
			Ok = 0,
			OutOfDate = 1,
			Error = 2,
		}
		private int ResultValue { get; set; }
		public int AsInt { get { return (int)ResultValue; } }

		public static readonly Result Ok = new() { ResultValue = (int)ResultEnum.Ok };
		public static readonly Result OutOfData = new() { ResultValue = (int)ResultEnum.OutOfDate };
		public static readonly Result Error = new() { ResultValue = (int)ResultEnum.Error };

		public static Result FromRaw(int b) { return new() { ResultValue = (int)(b & 0x3) }; }

		public bool IsOk { get { return ResultValue == 0; } }
		public bool IsOutOfData { get { return ((int)ResultValue & (int)(ResultEnum.OutOfDate)) != 0; } }
		public bool IsError { get { return ((int)ResultValue & (int)(ResultEnum.Error)) != 0; } }

		public static bool operator ==(Result b1, Result b2)
		{
			return b1.AsInt == b2.AsInt;
		}

		public static bool operator !=(Result b1, Result b2)
		{
			return b1.AsInt != b2.AsInt;
		}

		public override int GetHashCode()
		{
			return ResultValue;
		}

		public override bool Equals(object obj)
		{
			Result other = (Result)obj;
			return this.AsInt == other.AsInt;
		}
	}

	public class GameDataCompilerLog
	{
		private BinaryFileReader mReader;
		private Dictionary<Hash160, Type> mCompilerTypeSet = new Dictionary<Hash160, Type>();
		private HashSet<Hash160> mCompilerSignatureSet = new HashSet<Hash160>();
		private string FilePath { get; set; }

		public GameDataCompilerLog(string filepath)
		{
			FilePath = filepath;
		}

		public Result Verify(List<GameData.IDataCompiler> compilers)
		{
			//              - Load 'Game Data Compiler Log'
			//                - See if there are any missing/added/changed IDataCompiler objects
			//                - Check if all source files are up to date
			//                - So a IDataCompiler needs to build a unique Hash of itself!
			//                - Save 'Game Data Compiler Log'

			// Return Ok when both are fine, returns OutOfData when the compiler log is out-of-date
			return Result.Ok;
		}

		public Result Verify()
		{
			//              - Load 'Game Data Compiler Log'
			//                - Check if all source files are up to date
			//                - Check if compiler versions are fine
			//                - Check if compiler bundle has changed
			//                - Save 'Game Data Compiler Log'

			// Return Ok when both are fine, returns OutOfData when the compiler log is out-of-date
			return Result.Ok;
		}

		public Result Merge(List<GameData.IDataCompiler> compilers)
		{
			return Result.Ok;
		}

		public Result Build(List<GameData.IDataCompiler> compilers)
		{
			return Result.Ok;
		}

		public Result Execute()
		{
			return Result.Ok;
		}

		public void RegisterCompiler(Type type)
		{
			Hash160 typeSignature = HashUtility.Compute_ASCII(type.FullName);
			if (!mCompilerTypeSet.ContainsKey(typeSignature))
			{
				mCompilerTypeSet.Add(typeSignature, type);
			}
		}

		public Result Create(List<IDataCompiler> cl)
		{
			MemoryStream memoryStream = new();
			BinaryMemoryWriter memoryWriter = new();
			if (memoryWriter.Open(memoryStream))
			{
				BinaryFileWriter fileWriter = new();
				if (fileWriter.Open(FilePath))
				{
					foreach (IDataCompiler compiler in cl)
					{
						Type type = compiler.GetType();
						Hash160 typeSignature = HashUtility.Compute_ASCII(type.FullName);
						memoryWriter.Reset();
						typeSignature.WriteTo(memoryWriter);
						fileWriter.Write(memoryStream.Length);
						fileWriter.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
					}
					return Result.Ok;
				}
			}
			return Result.Error;
		}

		private bool OpenStreamIn()
		{
			mReader = new BinaryFileReader();
			if (mReader.Open(FilePath))
			{
				return true;
			}
			return false;
		}

		private bool StreamIn(List<IDataCompiler> compilers, int count)
		{
			for (int i = 0; i < count; i++)
			{
				Hash160 typeSignature = Hash160.ReadFrom(mReader);
				UInt32 blockSize = mReader.ReadUInt32();

				// We could have a type signature in the log that doesn't exists anymore because
				// the name of the compiler has been changed. When this is the case we need to
				// inform the user of this class that the log is out-of-date!

				Type type;
				if (mCompilerTypeSet.TryGetValue(typeSignature, out type))
				{
					IDataCompiler compiler = Activator.CreateInstance(type) as IDataCompiler;
					Hash160 compilerSignature = compiler.CompilerRead(mReader);
					if (!mCompilerSignatureSet.Contains(compilerSignature))
					{
						mCompilerSignatureSet.Add(compilerSignature);
						compilers.Add(compiler);
					}
				}
				else
				{
					if (!mReader.SkipBytes((Int64)blockSize))
						return false;
				}
			}

			return true;
		}

		private void CloseStreamIn()
		{
			if (mReader != null)
			{
				mReader.Close();
				mReader = null;
			}
		}
	}
}