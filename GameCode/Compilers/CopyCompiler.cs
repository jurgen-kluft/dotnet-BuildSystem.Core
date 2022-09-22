using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
	/*
	A Compiler will be executed when the Compiler is streaming-in the CompilersLog.BinaryStream.
	
	Binary data is loaded in block for block and compilers are constructed and executed, once
	executed they are called to save themselves to a streaming-out object.
	
	TODO
	- MeshCompiler
	- MaterialCompiler
	- TextureCompiler

	 */

	public class CopyCompiler : IDataCompiler, IDataCompilerClient
	{
		private readonly string mFilename;
		private FileId mFileId = new FileId();
		private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

		public CopyCompiler(string filename)
		{
			mFilename = Environment.expandVariables(filename);
		}

		public EDataCompilerStatus CompilerStatus { get { return mStatus; } }

		public void CompilerSetup()
		{
		}

		public void CompilerSave(GameData.IDataCompilerLog stream)
		{ 
		}

		public void CompilerLoad(GameData.IDataCompilerLog stream)
		{ 
		}

		public void CompilerExecute()
		{
		}

		public void CompilerFinished()
		{

		}

		public FileId[] fileIds { get { return new FileId[] { mFileId }; } }
	}
}
