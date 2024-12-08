using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;
namespace GameData
{
    public sealed class Localization
    {
        public string m_Language;
        public List<string> m_Strings = new();
    }

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
        public uint FileIndex { get; set; }

        public DataCompilerOutput CompilerExecute()
        {
            DataCompilerResult result;
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.Src, mSrcFilename);
                // Load 'languages list' file
                try
                {
                    var reader = TextStream.OpenForRead(mSrcFilename);
                    while (!reader.EndOfStream)
                    {
                        var filename = reader.ReadLine();
                        if (string.IsNullOrEmpty(filename))
                        {
                            mDstFilenames.Add(filename);
                        }
                    }
                    reader.Close();
                }
                catch (Exception)
                {
                    result = DataCompilerResult.Error;
                }
                finally
                {
                    result = DataCompilerResult.DstMissing;
                }
            }
            else
            {
                result = mDependency.Update(delegate (short id, State state)
                {
                    var result2 = DataCompilerResult.UpToDate;
                    if (state == State.Missing)
                    {
                        switch (id)
                        {
                            case 0: result2 = DataCompilerResult.SrcMissing; break;
                            default: result2 = DataCompilerResult.DstMissing; break;
                        }
                    }
                    else if (state == State.Modified)
                    {
                        switch (id)
                        {
                            case 0: result2 = DataCompilerResult.SrcChanged; break;
                            default: result2 = DataCompilerResult.DstChanged; break;
                        }
                    }

                    return result2;
                });
            }

            return new DataCompilerOutput(result, mDstFilenames.ToArray(), this);
        }
    }
}
