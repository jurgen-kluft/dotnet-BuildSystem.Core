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
        private List<string> mDstFilenames = new ();

        public LocalizationCompiler(string localizationFile)
        {
            mSrcFilename = Path.ChangeExtension(localizationFile, ".loc") + ".ids" + ".lst";
        }

        public void CompilerSignature(IBinaryWriter stream)
        {
        }

        public void CompilerWrite(IBinaryWriter stream)
        {
        }

        public void CompilerRead(IBinaryReader stream)
        {
        }

        public void CompilerConstruct(IDataCompiler dc)
        {
            if (dc is LocalizationCompiler lc)
            {
                mSrcFilename = lc.mSrcFilename;
                mDstFilenames = lc.mDstFilenames;
                //mDependency = lc.mDependency;
            }
        }

        public IFileIdProvider CompilerFileIdProvider => this;
        public long FileId { get; set; }

        public DataCompilerOutput CompilerExecute()
        {
            // Load 'languages list' file
            try
            {
                TextStream ts = new (mSrcFilename);
                ts.Open(TextStream.EMode.Read);
                while (!ts.Reader.EndOfStream)
                {
                    string filename = ts.Reader.ReadLine();
                    if (String.IsNullOrEmpty(filename))
                    {
                        mDstFilenames.Add(filename);
                    }
                }
                ts.Close();
            }
            catch (Exception)
            {
                //mStatus = ERROR;
                return new DataCompilerOutput(DataCompilerOutput.EResult.Error, mDstFilenames.ToArray());
            }
            return new DataCompilerOutput(DataCompilerOutput.EResult.Ok, mDstFilenames.ToArray());
        }
    }
}
