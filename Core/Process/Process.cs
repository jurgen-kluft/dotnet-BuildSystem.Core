using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;

namespace Core
{
    /// <summary>
    /// Using a process to call an external application
    /// </summary>
    public class Process
    {
        #region Fields

        private ProcessInfo mInfo;
        private ProcessExecutor mExecutor;

        #endregion
        #region Constructor

        public Process(Filename application, Dirname workDir, int timeOutInMinutes)
        {
            mInfo = new ProcessInfo(application);
            mInfo.WorkingDirectory = workDir;
            mInfo.TimeOut = timeOutInMinutes * 60 * 1000;
            mExecutor = new ProcessExecutor();
        }

#endregion
        #region Properties

        public bool Verbose
        {
            get { return mInfo.Verbose; }
            set { mInfo.Verbose = value; }
        }

        #endregion
        #region Execute

        public ProcessResult Execute(string arguments)
        {
            try
            {
			    mInfo.Arguments = arguments;
                ProcessResult pr = mExecutor.Execute(mInfo);
                return pr;
            }
            catch (Exception e)
            {
                return new ProcessResult("Exception occurred", String.Format("Exception occurred when executing process {0} with arguments {1} : {2}", mInfo.FileName, arguments, e.ToString()), -1, false);
            }
        }

        #endregion
    }
}
