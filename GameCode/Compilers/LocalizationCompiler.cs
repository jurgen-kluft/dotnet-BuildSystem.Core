using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;
namespace GameData
{
    public sealed class LocalizationCompiler : IDataCompiler, IFileIdProvider
    {
        private string mSrcFilename;
        private List<string> mDstFilenames = new();
        private Dependency mDependency;

        public LocalizationCompiler(string localizationFile)
        {
            mSrcFilename = Path.ChangeExtension(localizationFile, ".loc") + ".ids" + ".lst";
        }

        public void CompilerSignature(IBinaryWriter stream)
        {
            stream.Write(mSrcFilename);
            for (var i = 0; i < mDstFilenames.Count; i++)
                stream.Write(mDstFilenames[i]);
        }

        public void CompilerWrite(IBinaryWriter stream)
        {
            stream.Write(mSrcFilename);
            stream.Write(mDstFilenames.Count);
            for (var i = 0; i < mDstFilenames.Count; i++)
                stream.Write(mDstFilenames[i]);
            mDependency.WriteTo(stream);
        }

        public void CompilerRead(IBinaryReader stream)
        {
            mSrcFilename = stream.ReadString();
            var count = stream.ReadInt32();
            mDstFilenames.Clear();
            for (var i = 0; i < count; i++)
                mDstFilenames.Add(stream.ReadString());
            mDependency = Dependency.ReadFrom(stream);
        }

        public void CompilerConstruct(IDataCompiler dc)
        {
            if (dc is LocalizationCompiler lc)
            {
                mSrcFilename = lc.mSrcFilename;
                mDstFilenames = lc.mDstFilenames;
                mDependency = lc.mDependency;
            }
        }

        public IFileIdProvider CompilerFileIdProvider => this;
        public long FileId { get; set; }

        public DataCompilerOutput CompilerExecute()
        {
            var result = DataCompilerOutput.EResult.Ok;
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.Src, mSrcFilename);
                // Load 'languages list' file
                try
                {
                    TextStream ts = new(mSrcFilename);
                    ts.Open(TextStream.EMode.Read);
                    while (!ts.Reader.EndOfStream)
                    {
                        var filename = ts.Reader.ReadLine();
                        if (string.IsNullOrEmpty(filename))
                        {
                            mDstFilenames.Add(filename);
                        }
                    }
                    ts.Close();
                }
                catch (Exception)
                {
                    result = DataCompilerOutput.EResult.Error;
                }
                finally
                {
                    result = DataCompilerOutput.EResult.DstMissing;
                }
            }
            else
            {
                if (!mDependency.Update(delegate (short id, State state)
                {
                    if (state == State.Missing)
                    {
                        switch (id)
                        {
                            case 0: result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.SrcMissing); break;
                            default: result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.DstMissing); break;
                        }
                    }
                    else if (state == State.Modified)
                    {
                        switch (id)
                        {
                            case 0: result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.SrcChanged); break;
                            default: result = (DataCompilerOutput.EResult)(result | DataCompilerOutput.EResult.DstChanged); break;
                        }
                    }
                }))
                {
                    result = DataCompilerOutput.EResult.Ok;
                }
            }

            return new DataCompilerOutput(result, mDstFilenames.ToArray());
        }
    }
}
