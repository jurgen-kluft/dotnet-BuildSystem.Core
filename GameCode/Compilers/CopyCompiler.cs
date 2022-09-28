using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    /*	

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

        public CopyCompiler(string filename) : this(filename, filename)
        {
        }
        public CopyCompiler(string srcfilename, string dstfilename)
        {
            mSrcFilename = srcfilename;
            mDstFilename = dstfilename;
        }

        public void CompilerSignature(IBinaryWriter stream)
        {
			stream.Write(mSrcFilename);
			stream.Write(mDstFilename);
        }

        public void CompilerSetup()
        {
        }

        public void CompilerWrite(IBinaryWriter stream)
        {
			stream.Write(mSrcFilename);
			stream.Write(mDstFilename);
            
            // Save dependency information
        }

        public void CompilerRead(IBinaryReader stream)
        {
			mSrcFilename = stream.ReadString();
			mDstFilename = stream.ReadString();

            // Load dependency information
        }

        public void CompilerExecute()
        {
            // Execute the actual purpose of this compiler
            File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, mDstFilename), true );
        }

        public void CompilerFinished()
        {
            // Update dependency information of src and dst file

        }

        public FileId[] FileIds { get { return new FileId[] { FileId.NewInstance(mDstFilename) }; } }
    }
}
