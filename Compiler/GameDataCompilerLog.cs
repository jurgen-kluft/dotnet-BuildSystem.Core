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

		private class SignatureComparer : IComparer<KeyValuePair<Hash160, IDataCompiler>>
		{
			public int Compare(KeyValuePair<Hash160, IDataCompiler> lhs, KeyValuePair<Hash160, IDataCompiler> rhs)
			{
				return Hash160.Compare(lhs.Key, rhs.Key);
			}
		}

		public Result Merge(List<IDataCompiler> previous_compilers, List<IDataCompiler> current_compilers, out List<IDataCompiler> merged_compilers)
		{
			merged_compilers = new List<IDataCompiler>(current_compilers.Count);

			// Cross-reference the 'previous_compilers' (loaded) with the 'current_compilers' (from GameData.___.dll) and combine into
			// 'merged_compilers'.
			// Report if there was anything 'merged'.
			
			// Build the signature database of 'previous_compilers'
			var previousCompilerSignatureList = BuildCompilerSignatureList(previous_compilers);
			var currentCompilerSignatureList = BuildCompilerSignatureList(current_compilers);

			int merged_previous_count = 0;
			int merged_current_count = 0;
			Result result = Result.Ok;
			foreach (var signature in currentCompilerSignatureList)
			{
				int index = previousCompilerSignatureList.BinarySearch(signature, new SignatureComparer());
				if (index >= 0)
				{
					merged_previous_count++;
					merged_compilers.Add(previousCompilerSignatureList[index].Value);
				}
				else
				{
					merged_current_count++;
					merged_compilers.Add(signature.Value);
				}
			}
			if (merged_previous_count == current_compilers.Count)
			{
				return Result.Ok;
			}
			return Result.OutOfData;
		}

		private List<KeyValuePair<Hash160, IDataCompiler>> BuildCompilerSignatureList(List<IDataCompiler> compilers)
		{
            MemoryStream memoryStream = new();
            BinaryMemoryWriter memoryWriter = new();

            List<KeyValuePair<Hash160, IDataCompiler>> signatureList = new(compilers.Count);
			foreach(IDataCompiler cl in compilers)
            {
                memoryWriter.Reset();
                Type compilerType = cl.GetType();
                Hash160 compilerTypeSignature = HashUtility.Compute_ASCII(compilerType.FullName);
                compilerTypeSignature.WriteTo(memoryWriter);
                cl.CompilerSignature(memoryWriter);
                Hash160 compilerSignature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                signatureList.Add(new KeyValuePair<Hash160, IDataCompiler>(compilerSignature, cl));
            }
            int Comparer(KeyValuePair<Hash160, IDataCompiler> lhs, KeyValuePair<Hash160, IDataCompiler> rhs)
            {
                return Hash160.Compare(lhs.Key, rhs.Key);
            }
            signatureList.Sort(Comparer);
			return signatureList;
		}

		private Dictionary<Hash160, IDataCompiler> BuildCompilerSignatureDict(List<IDataCompiler> compilers)
		{
            MemoryStream memoryStream = new();
            BinaryMemoryWriter memoryWriter = new();

            Dictionary<Hash160, IDataCompiler> signatureDict = new(compilers.Count);
			foreach(IDataCompiler cl in compilers)
            {
                memoryWriter.Reset();
                Type compilerType = cl.GetType();
                Hash160 compilerTypeSignature = HashUtility.Compute_ASCII(compilerType.FullName);
                compilerTypeSignature.WriteTo(memoryWriter);
                cl.CompilerSignature(memoryWriter);
                Hash160 compilerSignature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                signatureDict.Add(compilerSignature, cl);
            }
            return signatureDict;
		}


		public Result Execute(List<IDataCompiler> compilers, out List<string> dst_relative_filepaths)
		{
			dst_relative_filepaths = new();
			int result = 0;
			foreach (IDataCompiler c in compilers)
			{
				int r = c.CompilerExecute(dst_relative_filepaths);
				if (r < 0) return Result.Error;
				else result = r;
			}
			if (result == 0)
			{
				return Result.Ok;
			}
			return Result.OutOfData;
		}

		private void RegisterCompilers(List<IDataCompiler> compilers)
		{
			foreach(var cl in compilers)
			{
				Type type = cl.GetType();
				Hash160 typeSignature = HashUtility.Compute_ASCII(type.FullName);
				if (!mCompilerTypeSet.ContainsKey(typeSignature))
				{
					mCompilerTypeSet.Add(typeSignature, type);
				}
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
			return false;
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

