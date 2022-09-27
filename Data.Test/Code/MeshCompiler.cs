using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    // e.g. new MeshCompiler("Models/Car.tri");
    public class MeshCompiler : IDataCompiler, IDataCompilerClient, IFileIdsProvider
    {
        private  string mSrcFilename;
        private  string mDstFilename;
        private FileId mFileId = new();
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        public MeshCompiler(string filename) : this(filename, filename)
        {
        }
        public MeshCompiler(string srcfilename, string dstfilename)
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
            
            // Save dependency information
        }

        public Hash160 CompilerRead(IBinaryReader stream)
        {
			mSrcFilename = stream.ReadString();
			mDstFilename = stream.ReadString();
			mStatus = (EDataCompilerStatus)stream.ReadUInt8();
            mFileId = FileId.NewInstance(mDstFilename);

            // Load dependency information

            // Return hash signature of this compiler
            return Hash160.Empty;
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

        public FileId[] FileIds { get { return new FileId[] { mFileId }; } }
    }
}
