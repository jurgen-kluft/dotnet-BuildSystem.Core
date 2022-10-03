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

        public int CompilerExecute(List<DataCompilerOutput> output)
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
                output.Add(new DataCompilerOutput(FileId, mDstFilenames.ToArray()));
                ts.Close();
            }
            catch (Exception)
            {
                //mStatus = ERROR;
                return -1;
            }
            return 0;
        }

        public Int64 FileId { get; set; }
    }
}
