using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    // e.g. new FileId(new CopyCompiler("Textures/Background.PNG"));
    public sealed class CopyCompiler : IDataCompiler, IFileIdProvider
    {
        private string mSrcFilename;
        private string mDstFilename;
        private Dependency mDependency;

        public CopyCompiler(string filename) : this(filename, filename)
        {
        }
        public CopyCompiler(string srcFilename, string dstFilename)
        {
            mSrcFilename = srcFilename;
            mDstFilename = dstFilename;
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

        public IFileIdProvider CompilerFileIdProvider => this;
        public long FileId { get; set; }

        public DataCompilerOutput CompilerExecute()
        {
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.Src, mSrcFilename);
                mDependency.Add(1, EGameDataPath.Dst, mDstFilename);
            }

            if (!mDependency.Update(null)) return new(0, new[] { mDstFilename });

            // Execute the actual purpose of this compiler
            try
            {
                File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, mDstFilename), true);
            }
            catch (Exception)
            {
                return new(-1, new[] { mDstFilename });
            }
            return new(1, new[] { mDstFilename });
        }
    }
}
