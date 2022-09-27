using System;
using GameData;
using GameCore;

namespace DataBuildSystem
{
	public struct State
	{
		private enum StateEnum : sbyte
		{
			Ok = 0,
			Modified = 1,
			Missing = 2,
		}
		private sbyte Value { get; set; }

		public static readonly State Ok = new() { Value = (sbyte)StateEnum.Ok };
		public static readonly State Missing = new() { Value = (sbyte)StateEnum.Missing };
		public static readonly State Modified = new() { Value = (sbyte)StateEnum.Modified };

		public bool IsOk { get { return Value == 0; } }
		public bool IsModified { get { return ((sbyte)Value & (sbyte)(StateEnum.Modified)) == (sbyte)StateEnum.Modified; } }
		public bool IsMissing { get { return ((sbyte)Value & (sbyte)(StateEnum.Missing)) == (sbyte)StateEnum.Missing; } }
	}

	public class GameDataCompilerLog
	{
		private BinaryFileReader mReader;

		private Dictionary<Hash160, Type> mCompilerTypeSet = new Dictionary<Hash160, Type>();

		// When 
		private HashSet<Hash160> mCompilerSignatureSet = new HashSet<Hash160>();

		public enum EState
		{
			UPTODATE,
		}

		public State Verify(List<GameData.IDataCompiler> compilers)
		{
			// Cross reference all compilers and the log on disk
			// Check if all source files are up to date
			// Return true when both are fine
			return State.Ok;
		}

		public void RegisterCompiler(Type type)
		{
			Hash160 typeSignature = HashUtility.Compute_ASCII(type.FullName);
			if (!mCompilerTypeSet.ContainsKey(typeSignature))
			{
				mCompilerTypeSet.Add(typeSignature, type);
			}
		}

		public bool Create(string dstpath, string filename, List<IDataCompiler> cl)
		{
			MemoryStream memoryStream = new();
			BinaryMemoryWriter memoryWriter = new();
			if (memoryWriter.Open(memoryStream))
			{
				BinaryFileWriter fileWriter = new();
				if (fileWriter.Open(Path.Join(dstpath, filename)))
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
					return true;
				}
			}
			return false;
		}

		public bool OpenStreamIn(string path, string filename)
		{
			mReader = new BinaryFileReader();
			if (mReader.Open(Path.Join(path, filename)))
			{
				return true;
			}
			return false;
		}

		public bool StreamIn(List<IDataCompiler> compilers, int count)
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

		public void CloseStreamIn()
		{
			if (mReader != null)
			{
				mReader.Close();
				mReader = null;
			}
		}
	}
}