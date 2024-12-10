using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class Model
    {
        // Index Buffer
        // Vertex Buffer
        // Materials
        // Textures
    }

    // e.g. new FileId(new ModelCompiler("Models/Teapot.glTF"));
    public sealed class ModelCompiler : IDataCompiler, IFileIdInstance
    {
        private string mSrcFilename;
        private string mDstFilename;
        private Dependency mDependency;

        public ModelCompiler(string filename) : this(filename, filename)
        {
        }
        public ModelCompiler(string srcFilename, string dstFilename)
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
            if (dc is not ModelCompiler cc) return;

            mSrcFilename = cc.mSrcFilename;
            mDstFilename = cc.mDstFilename;
            mDependency = cc.mDependency;
        }

        public IFileIdInstance CompilerFileIdProvider => this;
        public uint FileIndex { get; set; }
        public string[] FileNames => new string[] { mDstFilename };

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
                    return new DataCompilerOutput(result,  this);
                }
            }

            try
            {
                // Execute the actual purpose of this compiler
                File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, mDstFilename), true);

                // Execution is done, update the dependency to reflect the new state
                result = mDependency.Update(null);
            }
            catch (Exception)
            {
                result = (DataCompilerResult)(result | DataCompilerResult.Error);
            }

            // The result returned here is the result that 'caused' this compiler to execute its action and not the 'new' state.
            return new DataCompilerOutput(result,  this);
        }
    }
}
