using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    // e.g. new FileId(new ShaderCompiler("StaticMesh.vs"));
    public sealed class ShaderCompiler : IDataCompiler, IFileIdProvider
    {
        private string mSrcFilename;
        private string mDstFilename;
        private Dependency mDependency;

        public ShaderCompiler(string filename) : this(filename, filename)
        {
        }
        public ShaderCompiler(string srcFilename, string dstFilename)
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
            if (dc is not ShaderCompiler cc) return;

            mSrcFilename = cc.mSrcFilename;
            mDstFilename = cc.mDstFilename;
            mDependency = cc.mDependency;
        }

        public IFileIdProvider CompilerFileIdProvider => this;
        public long FileId { get; set; }

        public DataCompilerOutput CompilerExecute()
        {
            var result = DataCompilerResult.None;
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.Src, mSrcFilename);
                mDependency.Add(1, EGameDataPath.Dst, mDstFilename);
                result = DataCompilerResult.DstMissing;
            }
            else
            {
                var result3 = mDependency.Update(delegate(short id, State state)
                {
                    var result2 = DataCompilerResult.None;
                    if (state == State.Missing)
                    {
                        result2 = id switch
                        {
                            0 => (DataCompilerResult.SrcMissing),
                            1 => (DataCompilerResult.DstMissing),
                            _ => (DataCompilerResult.None),
                        };
                    }
                    else if (state == State.Modified)
                    {
                        result2 |= id switch
                        {
                            0 => (DataCompilerResult.SrcChanged),
                            1 => (DataCompilerResult.DstChanged),
                            _ => (DataCompilerResult.None)
                        };
                    }

                    return result2;
                });

                if (result3 == DataCompilerResult.UpToDate)
                {
                    result = DataCompilerResult.UpToDate;
                    return new DataCompilerOutput(result, new[] { mDstFilename }, this);
                }
            }

            try
            {
                // Execute the actual purpose of this compiler
                File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, mDstFilename), true);

                // Note: On Windows we have different tools than on Mac/Linux
                // Note: Shaders are in HLSL and for Mac (Metal) need to be compiled

                // Execution is done, update the dependency to reflect the new state
                result = mDependency.Update(null);
            }
            catch (Exception)
            {
                result = (DataCompilerResult)(result | DataCompilerResult.Error);
            }

            // The result returned here is the result that 'caused' this compiler to execute its action and not the 'new' state.
            return new DataCompilerOutput(result, new[] { mDstFilename }, this);
        }
    }
}
