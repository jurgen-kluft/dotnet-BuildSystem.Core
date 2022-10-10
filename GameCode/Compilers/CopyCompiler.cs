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

        public void CompilerConstruct(IDataCompiler dc)
        {
            if (dc is CopyCompiler cc)
            {
                mSrcFilename = cc.mSrcFilename;
                mDstFilename = cc.mDstFilename;
                mDependency = cc.mDependency;
            }
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

            DataCompilerOutput.EResult result = DataCompilerOutput.EResult.None;
            if (!mDependency.Update(delegate(short id, State state) {
                if (state == State.Missing)
                {
                    switch (id)
                    {
                        case 0: result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.SrcMissing); break;
                        case 1: result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.DstMissing); break;
                    }
                }
                else if (state == State.Modified)
                {
                    switch (id)
                    {
                        case 0: result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.SrcChanged); break;
                        case 1: result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.DstChanged); break;
                    }
                }
            }))
            {
                result = DataCompilerOutput.EState.Ok;
            }
            else
            {
                // Execute the actual purpose of this compiler
                try
                {
                    File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, mDstFilename), true);
                }
                catch (Exception)
                {
                    result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.Error);
                }
            }
            return new(result, new[] { mDstFilename });
        }
    }
}
