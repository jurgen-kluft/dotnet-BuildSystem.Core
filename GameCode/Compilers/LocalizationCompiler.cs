using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;
namespace GameData
{
    public class LocalizationCompiler : IDataCompiler, IDataCompilerClient, IFileIdsProvider
    {
        private readonly string mSrcFilename;
        private readonly List<string> mDstFilenames = new ();
        private readonly List<FileId> mFileIds = new ();
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        public LocalizationCompiler(string localizationFile)
        {
            mSrcFilename = Path.ChangeExtension(localizationFile, ".loc") + ".ids" + ".lst";
        }

        public EDataCompilerStatus CompilerStatus { get { return mStatus; } }

        public void CompilerSetup()
        {
        }

        public void CompilerWrite(IBinaryWriter stream)
        {
        }

        public Hash160 CompilerRead(IBinaryReader stream)
        {
            return Hash160.Empty;
        }

        public void CompilerExecute()
        {
            //if (mDependencySystem.isModified(mSrcFilename))
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
                    mStatus = EDataCompilerStatus.ERROR;
                }
                finally
                {
                    mStatus = EDataCompilerStatus.SUCCESS;
                }
            }
            //else
            //{
            //    mStatus = EDataCompilerStatus.UPTODATE;
            //}
        }

        public void CompilerFinished()
        {
            
        }

        public FileId[] FileIds { get { return mFileIds.ToArray(); } }
    }
}
