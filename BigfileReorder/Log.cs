using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BigfileFileReorder
{
	class Log
	{
		private StreamWriter mFileWriter;
		static private Log sInstance;
		protected Log()
		{
			mFileWriter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "reorder.log");
		}

		static public Log sGetInstance()
		{
			return sInstance;
		}

		static public void sInit()
		{
			sInstance = new Log();
		}

		public void write(string text)
		{
			mFileWriter.Write(text);
		}

		public void writeLine(string text)
		{
			mFileWriter.WriteLine(text);
		}

		public void flush()
		{
			mFileWriter.Flush();
		}
	}
}
