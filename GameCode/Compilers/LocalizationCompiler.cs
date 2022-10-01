using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;
namespace GameData
{
    public class LocalizationCompiler : IDataCompiler, IFilesProvider
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

        public IFilesProvider CompilerFilesProvider
        {
            get
            {
                return this;
            }
        }
        
        public int CompilerExecute(List<string> out_dst_relative_filepaths)
        {
            // Load 'languages list' file
            try
            {
                xTextStream ts = new (mSrcFilename);
                ts.Open(xTextStream.EMode.READ);
                while (!ts.read.EndOfStream)
                {
                    string filename = ts.read.ReadLine();
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
                return -1;
            }
            finally
            {
                //mStatus = SUCCESS;
            }
            return 0;
        }

        public UInt64 FilesProviderId { get; set; }
        public string[] FilesProviderFilepaths { get { return mDstFilenames.ToArray(); } }
    }
}
