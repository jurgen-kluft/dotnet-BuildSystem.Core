using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    // e.g. new MeshCompiler("Models/Car.tri");
    public class MeshCompiler : IDataCompiler, IDataCompilerClient, IFileIdsProvider
    {
        private string mSrcFilename;
        private string mDstFilename;

        public MeshCompiler(string filename) : this(filename, filename)
        {
        }

        public MeshCompiler(string srcfilename, string dstfilename)
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
            
            // Call our external process to compile this mesh (obj, ply) into a tri format
        }

        public void CompilerFinished()
        {
            // Update dependency information of src and dst file

        }

        public FileId[] FileIds { get { return new FileId[] { FileId.NewInstance(mDstFilename) }; } }
    }
}
