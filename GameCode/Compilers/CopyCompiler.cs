using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
	public class CopyCompiler : IDataCompiler, IDataCompilerClient, IFileIdsProvider
	{
		private readonly Filename mFilename;
		private FileId fileId = new FileId();
		private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

		public CopyCompiler(string filename)
		{
			filename = Environment.expandVariables(filename);
			mFilename = new Filename(filename);
		}

		public string group { get { return "CopyCompiler"; } }
		public EDataCompilerStatus status { get { return mStatus; } }

		public void csetup()
		{
		}

		public void csave(GameData.IDataCompilerStream stream)
		{ 
		}

		public void cload(GameData.IDataCompilerStream stream)
		{ 
		}

		public void cteardown()
		{
		}

		public void onExecute()
		{
		}

		public void onFinished()
		{

		}

		public void registerAt(IFileRegistrar registrar)
		{
			fileId = registrar.Add(mFilename);
		}

		public FileId[] fileIds { get { return new FileId[] { fileId }; } }
	}
}
