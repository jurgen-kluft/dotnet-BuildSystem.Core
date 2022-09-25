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

    // e.g. new CopyCompiler("Textures/Background.PNG");
    public class CopyCompiler : IDataCompiler, IDataCompilerClient, IFileIdsProvider
    {
        private  string mSrcFilename;
        private  string mDstFilename;
        private FileId mFileId = new();
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        public CopyCompiler(string filename) : this(filename, filename)
        {
        }
        public CopyCompiler(string srcfilename, string dstfilename)
        {
            mSrcFilename = srcfilename;
            mDstFilename = dstfilename;
            mFileId = FileId.NewInstance(mDstFilename);
        }

        public EDataCompilerStatus CompilerStatus { get { return mStatus; } }

        public void CompilerSetup()
        {
        }

        public void CompilerWrite(IBinaryWriter stream)
        {
			stream.Write(mSrcFilename);
			stream.Write(mDstFilename);
			stream.Write((byte)mStatus);
        }

        public void CompilerRead(IBinaryReader stream)
        {
			mSrcFilename = stream.ReadString();
			mDstFilename = stream.ReadString();
			mStatus = (EDataCompilerStatus)stream.ReadUInt8();
        }

        public void CompilerExecute()
        {
        }

        public void CompilerFinished()
        {

        }

        public FileId[] FileIds { get { return new FileId[] { mFileId }; } }
    }
}
