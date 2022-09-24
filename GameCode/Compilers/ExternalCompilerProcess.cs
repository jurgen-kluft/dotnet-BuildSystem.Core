using System;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    /// <summary>
    /// An external process
    /// </summary>
    public class ExternalCompilerProcess
    {
        #region Fields

        private string mBinarizerName = string.Empty;
        private bool mProcessHasBeenModified = false;
        private Process mProcess;
		
        #endregion
        #region Constructor

        public ExternalCompilerProcess(string binarizer, string workDir, int timeOutInMinutes)
        {
            mBinarizerName = binarizer;
            constructProcess(new Filename(binarizer), new Dirname(workDir), timeOutInMinutes);
        }

        #endregion
        #region Properties

        public string name
        {
            get
            {
                return mBinarizerName;
            }
        }

        public bool isModified
        {
            get
            {
                return mProcessHasBeenModified;
            }
        }

        public Process process
        {
            get
            {
                return mProcess;
            }
        }

        #endregion
        #region Construct Process

        private void constructProcess(Filename binarizer, Dirname workDir, int timeOutInMinutes)
        {
            mProcess = new Process(BuildSystemCompilerConfig.ToolPath+binarizer, workDir, timeOutInMinutes);

            FileCommander.createDirectoryOnDisk(BuildSystemCompilerConfig.DstPath);

            //const string additionalExtension = ".tdep";

            mProcessHasBeenModified = true;
        }

        #endregion
        #region Arguments

        public string arguments(string filename)
        {
            return String.Format("-srcpath \"{0}\" -dstpath \"{1}\" -srcfile \"{2}\"", BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.DstPath, filename, BuildSystemCompilerConfig.PlatformName);
        }

        public string arguments(string filename, string appendToCmdLine)
        {
            string a = arguments(filename);
            a += " " + appendToCmdLine;
            return a;
        }

        #endregion
    }
}
