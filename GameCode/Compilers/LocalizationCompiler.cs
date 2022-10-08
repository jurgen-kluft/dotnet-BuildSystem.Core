using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;
namespace GameData
{
    public sealed class LocalizationCompiler : IDataCompiler, IFileIdProvider
    {
        private readonly string mSrcFilename;
        private readonly List<string> mDstFilenames = new ();

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

        public IFileIdProvider CompilerFileIdProvider
        {
            get
            {
                return this;
            }
        }

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
                return new DataCompilerOutput(-1, mDstFilenames.ToArray());
            }
            return new DataCompilerOutput(0, mDstFilenames.ToArray());        
        }

        public Int64 FileId { get; set; }
    }
}
