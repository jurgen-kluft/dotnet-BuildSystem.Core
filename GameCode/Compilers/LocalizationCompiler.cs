using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;
namespace GameData
{
    public class LocalizationCompiler : IDataCompiler, IFileIdsProvider
    {
        private readonly string mSrcFilename;
        private readonly List<string> mDstFilenames = new ();
        private readonly List<FileId> mFileIds = new ();

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
                        mFileIds.Add(FileId.NewInstance(filename.ToLower()));
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

        public void CompilerFinished()
        {
            
        }

        public FileId[] FileIds { get { return mFileIds.ToArray(); } }
    }
}
