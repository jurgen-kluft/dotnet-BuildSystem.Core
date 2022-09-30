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
    public class CopyCompiler : IDataCompiler, IFilesProvider
    {
        private  string mSrcFilename;
        private  string mDstFilename;
        private Dependency mDependency;

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

        public void CompilerWrite(IBinaryWriter stream)
        {
			stream.Write(mSrcFilename);
			stream.Write(mDstFilename);
            mDependency.WriteTo(stream);
        }

        public void CompilerRead(IBinaryReader stream)
        {
			mSrcFilename = stream.ReadString();
			mDstFilename = stream.ReadString();
            mDependency = Dependency.ReadFrom(stream);
        }

        public int CompilerExecute(List<string> dst_relative_filepaths)
        {
            // Execute the actual purpose of this compiler
            try
            {
                File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, mDstFilename), true);
            }
            catch (Exception)
			{
                return -1;
			}

            mDependency = new Dependency(EGameDataPath.Src, mSrcFilename);
            mDependency.Add(1, EGameDataPath.Dst, mDstFilename);
            mDependency.Update(Dependency.OnUpdateNop);
        
            dst_relative_filepaths.Add(mDstFilename);
            return 0;
        }

        public UInt64 FilesProviderId { get; set; }
        public string[] FilesProviderFilepaths { get { return new string[] { mDstFilename }; } }
    }
}
