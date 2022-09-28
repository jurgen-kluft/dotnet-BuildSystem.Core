using System;
using GameData;
using GameCore;

namespace DataBuildSystem
{
	public class GameDataCompilerLog
	{
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

		public Result Merge(List<GameData.IDataCompiler> previous_compilers, List<GameData.IDataCompiler> current_compilers)
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

		public Result Save(List<IDataCompiler> cl)
		{
			BinaryFileWriter fileWriter = new();
			if (fileWriter.Open(FilePath))
			{
				MemoryStream memoryStream = new();
				BinaryMemoryWriter memoryWriter = new();
				if (memoryWriter.Open(memoryStream))
				{
					foreach (IDataCompiler compiler in cl)
					{
						memoryWriter.Reset();
						Type compilerType = compiler.GetType();
						Hash160 compilerTypeSignature = HashUtility.Compute_ASCII(compilerType.FullName);
						compilerTypeSignature.WriteTo(memoryWriter);
						compiler.CompilerSignature(memoryWriter);
						Hash160 compilerSignature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);

						// byte[4]: Length of Block
						// byte[20]: Compiler Type Signature
						// byte[20]: Compiler Signature
						// byte[]: Compiler Property Data

						memoryWriter.Reset();
						compilerTypeSignature.WriteTo(memoryWriter);
						compilerSignature.WriteTo(memoryWriter);
						compiler.CompilerWrite(memoryWriter);
						fileWriter.Write(memoryStream.Length);
						fileWriter.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
					}
					memoryWriter.Close();
					fileWriter.Close();
					return Result.Ok;
				}
				fileWriter.Close();
			}
			return Result.Error;
		}

		public bool Load(List<IDataCompiler> compilers)
		{
			BinaryFileReader reader = new ();
			if (reader.Open(FilePath))
			{
				while (reader.Position < reader.Length)
				{
					UInt32 blockSize = reader.ReadUInt32();
					Hash160 compilerTypeSignature = Hash160.ReadFrom(reader);
					Hash160 compilerSignature = Hash160.ReadFrom(reader);

					// We could have a type signature in the log that doesn't exists anymore because
					// the name of the compiler has been changed. When this is the case we need to
					// inform the user of this class that the log is out-of-date!

					Type type;
					if (mCompilerTypeSet.TryGetValue(compilerTypeSignature, out type))
					{
						IDataCompiler compiler = Activator.CreateInstance(type) as IDataCompiler;
						if (!mCompilerSignatureSet.Contains(compilerSignature))
						{
							mCompilerSignatureSet.Add(compilerSignature);
							compilers.Add(compiler);
						}
						compiler.CompilerRead(reader);
					}
					else
					{
						if (!reader.SkipBytes((Int64)blockSize))
							break;
					}
				}

				reader.Close();
				return true;
			}
		}
	}

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

}

