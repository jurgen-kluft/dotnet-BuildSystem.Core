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
            mSrcFilename = srcFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            mDstFilename = dstFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
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
            if (dc is not CopyCompiler cc) return;
            
            mSrcFilename = cc.mSrcFilename;
            mDstFilename = cc.mDstFilename;
            mDependency = cc.mDependency;
        }

        public IFileIdProvider CompilerFileIdProvider => this;
        public long FileId { get; set; }

        public DataCompilerOutput CompilerExecute()
        {
            var result = DataCompilerOutput.EResult.None;
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.Src, mSrcFilename);
                mDependency.Add(1, EGameDataPath.Dst, mDstFilename);
                result = DataCompilerOutput.EResult.DstMissing;
            }
            else
            {
                if (!mDependency.Update(delegate(short id, State state)
                    {
                        if (state == State.Missing)
                        {
                            result = id switch
                            {
                                0 => (result | DataCompilerOutput.EResult.SrcMissing),
                                1 => (result | DataCompilerOutput.EResult.DstMissing),
                                _ => result
                            };
                        }
                        else if (state == State.Modified)
                        {
                            result = id switch
                            {
                                0 => (result | DataCompilerOutput.EResult.SrcChanged),
                                1 => (result | DataCompilerOutput.EResult.DstChanged),
                                _ => result
                            };
                        }
                    }))
                {
                    result = DataCompilerOutput.EResult.Ok;
                    return new DataCompilerOutput(result, new[] { mDstFilename });
                }
            }

            try
            {
                // Execute the actual purpose of this compiler
                File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, mDstFilename), true);

                // Execution is done, update the dependency to reflect the new state
                mDependency.Update(null);
            }
            catch (Exception)
            {
                result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.Error);
            }

            // The result returned here is the result that 'caused' this compiler to execute its action and not the 'new' state.
            return new DataCompilerOutput(result, new[] { mDstFilename });
        }
    }
}
