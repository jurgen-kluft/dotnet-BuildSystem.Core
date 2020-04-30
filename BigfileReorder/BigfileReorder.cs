using System;
using System.Collections.Generic;
using System.IO;
using Core;
using DataBuildSystem;

namespace BigfileFileReorder
{
    /// <summary>
    /// File ordering of a Bigfile + BigfileToc so as to optimize the reading speed 
    /// of the game/application. A FileOrder information file has to be generated, 
    /// either manually, or by logging the order of files accessed in the game/application.
    /// 
    /// Example scopes:
    /// - Boot state of the game
    /// - Attract/Demo
    /// - Menu
    /// - Car 1-12
    /// - Track 1-12
    /// 
    /// File order information file is organized like:
    /// class FileOrder
    /// {
    ///     class Scope
    ///     {
    ///         string mName;
    ///         int mFrameIndex;
    ///         int[] mFileIds;
    ///     }
    ///     Scope[] mScopes;
    /// }
    /// 
    /// 
    /// Solution #1:
    ///     When a request is handled to load a file with FileId X then by remembering the last FileOffset
    ///     and FileSize we can iterate over the number of possible FileOffset[] for FileId X and when we 
    ///     find a FileOffset that is after the last read FileOffset we use that one to load X.
    /// 
    /// Things to consider:
    /// - Only read ahead
    /// - Multiple offsets (duplicating files on disk) for one FileId.
    /// - In the application FileIds need to be in order as they appear on the storage media, so by sorting
    ///   FileIds we can sort the access if necessary.
    /// 
    /// </summary>
    public class BigfileFileOrder
    {
        #region FileGroup class

		private class FileGroup
		{
			#region Constructor
			public FileGroup(int order)
			{
				mOrder = order;
			}
			#endregion
			#region Properties
			public int GroupOrder
			{
				get
				{
					return mOrder;
				}
			}
			#endregion
			#region Methods

			public void addSubGroup(int order)
			{
				if ( -1 == order )
				{
					mDefaultGroup = new FileGroup(-1);
					mCurrentGroup = mDefaultGroup;
					return;
				}
				if ( null == mSubGroups )
				{
					mSubGroups = new List<FileGroup>();
				}

				foreach (FileGroup group in mSubGroups)
				{
					if (order == group.GroupOrder)
					{
						return;
					}
				}
				mCurrentGroup = new FileGroup(order);
				mSubGroups.Add(mCurrentGroup);
			}

			public FileGroup getSubGroup(int order)
			{
				if ( -1 == order )
				{
					return mDefaultGroup;
				}

				if (null == mSubGroups)
				{
					return null;
				}

				foreach (FileGroup group in mSubGroups)
				{
					if (order == group.GroupOrder)
					{
						return group;
					}
				}
				return null;
			}

			public void addFile(int fileId)
			{
				mFileIds.Add(fileId);
			}

			public FileGroup getCurrentGroup()
			{
				if (null != mCurrentGroup)
				{
					return mCurrentGroup.getCurrentGroup();
				}
				return this;
			}

			public void setCurrentGroup(int order)
			{
				if (-1 == order)
				{
					mCurrentGroup = mDefaultGroup;
					return;
				}
				if (null == mSubGroups)
				{
					return;
				}

				foreach (FileGroup group in mSubGroups)
				{
					if (order == group.GroupOrder)
					{
						mCurrentGroup = group;
						return;
					}
				}
			}

			public int getFilesNum()
			{
				int defaultNum = 0;
				int subNum = 0;
				if (null != mDefaultGroup)
				{
					defaultNum = mDefaultGroup.getFilesNum();
				}

				if (null != mSubGroups)
				{
					foreach (FileGroup group in mSubGroups)
					{
						subNum += group.getFilesNum();
					}
				}

				return mFileIds.Count + defaultNum + subNum;
			}

			public void collectAllFiles(ref List<Int32> collector)
			{
				for ( int i = 0; i < mFileIds.Count; i++ )
				{
					collector.Add(mFileIds[i]);
				}
				if ( null != mDefaultGroup )
				{
					mDefaultGroup.collectAllFiles(ref collector);
				}

				if ( null != mSubGroups )
				{
					foreach(FileGroup group in mSubGroups)
					{
						group.collectAllFiles(ref collector);
					}
				}
			}

            public void collect(BigfileFileOrder order, List<int> remap)
			{
				for (int i = 0; i < mFileIds.Count; i++)
				{
                    order.collect(mFileIds[i], remap);
				}

				if (null != mDefaultGroup)
				{
                    mDefaultGroup.collect(order, remap);
				}

				if (null != mSubGroups)
				{
					foreach (FileGroup group in mSubGroups)
					{
                        group.collect(order, remap);
					}
				}
			}

			#endregion
			#region Fields

			private readonly int mOrder = -1;
			private readonly List<int> mFileIds = new List<int>();
			private FileGroup mCurrentGroup;
			private FileGroup mDefaultGroup;
			private List<FileGroup> mSubGroups;

			#endregion
		}

		#endregion
		#region FileCategory class

		private class FileCategory
        {
			public FileCategory(string name)
			{
				mCategoryName = name;
			}

			public string Name
			{
				get
				{
					return mCategoryName;
				}
			}

			public FileGroup Files
			{
				get
				{
					return mFiles;
				}
			}
			private readonly string mCategoryName = BigfileFileOrder.DEFAULT_SCOPE_NAME;
			private readonly FileGroup mFiles = new FileGroup(-1);
		}
		#endregion
        #region Fields

        private const string BEGIN_CATEGORY_PREFIX = "Begin Category:";
		private const string CONTINUE_CATEGORY_PREFIX = "Continue Category:";
		private const string DEFAULT_SCOPE_NAME = "_Default";
		private readonly List<FileCategory> mList = new List<FileCategory>();
		private readonly BigfileToc mBigfileToc = new BigfileToc();
		private int mCurrentCategory = -1;
		private List<Int32> mRecordedFiles = new List<Int32>();

        #endregion
        #region Reorder

        public void Reorder(string bigFileName, string tocFileName, string orderFileName, string platform)
        {
            // INPUT:
            // - BigFile
            // - BigFileToc
            //
            // PROCESS:
            // - Generate new BigFileToc
            // - Build new BigFile
            // 
            // OUTPUT:
            // - BigFile
            // - BigFileToc
            if (!scanOrderFile(orderFileName))
                return;

            // Load BigfileToc information
            Filename bigfileTocFilename = new Filename(tocFileName);
            mBigfileToc.load(bigfileTocFilename.PoppedExtension(), Config.Endian);

            appendUnrecordedFiles();

            // Create a big file builder to help us building a Bigfile and BigfileToc
            Filename bigfileFilename = new Filename(bigFileName);

            // Collect all Bigfile files in order
            List<int> remap = new List<int>();
            foreach (FileCategory category in mList)
                category.Files.collect(this, remap);

            List<BigfileFile> srcBigfileFiles = new List<BigfileFile>();
            for (int i = 0; i < mBigfileToc.Count; ++i)
                srcBigfileFiles.Add(mBigfileToc.infoOf(i));

            Filename newBigfileFilename = bigfileFilename;
            newBigfileFilename.ShortName = "Temp";
            BigfileBuilder.sReorder(bigfileFilename, Config.DstPath, srcBigfileFiles, newBigfileFilename, remap, Config.Endian);

            File.Copy(newBigfileFilename, bigfileFilename, true);
            File.Delete(newBigfileFilename);
        }

        #endregion
        #region Methods

        public bool scanOrderFile(string fileName)
		{
			try
			{
				// Create an instance of StreamReader to read from a file.
				// The using statement also closes the StreamReader.
				using (StreamReader sr = new StreamReader(fileName))
				{
					String line;
					bool skipGroup = false;
					int lineIndex = 0;
					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						lineIndex++;
						// New category
						if ( - 1 != line.IndexOf(BEGIN_CATEGORY_PREFIX) )
						{
							if ( !addFileGroup(line) )
							{
								skipGroup = true;
							}
							else
							{
								skipGroup = false;
							}
						}
						// Continue category
						else if ( -1 != line.IndexOf(CONTINUE_CATEGORY_PREFIX))
						{
							if (!continueFileGroup(line))
							{
								return false;
							}
							else
							{
								skipGroup = false;
							}
						}
						else if (!skipGroup)
						{
							if ( !addFile(line) )
							{
								return false;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				// Let the user know what went wrong.
				Console.WriteLine("Order file can't be open!");
				Console.WriteLine(e.Message);
				return false;
			}
			return true;
		}

		protected void collectAllFiles()
		{
			mRecordedFiles.Clear();
			foreach(FileCategory category in mList)
			{
				category.Files.collectAllFiles(ref mRecordedFiles);
			}
		}

		protected bool isRecorded(Int32 fileId)
		{
			foreach(Int32 id in mRecordedFiles)
			{
				if (id == fileId)
				{
					return true;
				}
			}
			return false;
		}

		protected void appendUnrecordedFiles()
		{
			collectAllFiles();

			mList[0].Files.setCurrentGroup(-1);
			mList[0].Files.getSubGroup(-1).setCurrentGroup(-1);

			for (Int32 fileId = 0; fileId < mBigfileToc.Count; fileId++ )
			{
				if ( !isRecorded(fileId) )
				{
					mList[0].Files.addFile(fileId);
					//Console.WriteLine("File <" + fileId + "> is not recorded!");
					BigfileFile file = mBigfileToc.infoOf(fileId);
					Log.sGetInstance().writeLine(file.filename);
				}
			}
			Log.sGetInstance().flush();
		}

		protected bool addFile(string line)
		{
			try
			{
				int lessThan = line.IndexOf("<");
				int moreThan = line.IndexOf(">");
				int mainOrder = -1;
				int subOrder = -1;
				string scopeName = DEFAULT_SCOPE_NAME;

				if (-1 != lessThan && -1 != moreThan && (lessThan + 1) < moreThan)
				{
					scopeName = line.Substring(0, lessThan);
					string order = line.Substring(lessThan + 1, moreThan - lessThan - 1);
					mainOrder = int.Parse(order);
				}
				else
				{
					return false;
				}

				lessThan = line.IndexOf("<", moreThan);
				moreThan = line.IndexOf(">", moreThan + 1);
				if (-1 != lessThan && -1 != moreThan && (lessThan + 1) < moreThan)
				{
					string order = line.Substring(lessThan + 1, moreThan - lessThan - 1);
					subOrder = int.Parse(order);
				}
				else
				{
					return false;
				}

				int colon = line.LastIndexOf(":");
				int fileId = -1;
				if (-1 != colon && colon > moreThan)
				{
					fileId = int.Parse(line.Substring(colon + 1));
				}
				else
				{
					return false;
				}

				getCurrentCategory().Files.getCurrentGroup().addFile(fileId);
			}
			catch ( Exception e )
			{
				Console.WriteLine(e.StackTrace);
				return false;
			}
			return true;
		}

		protected bool addFileGroup(string text)
		{
			string categoryName = "";
			int mainOrder = -1;
			int subOrder = -1;

			if (!getCategoryInfo(text, ref categoryName, ref mainOrder, ref subOrder))
			{
				return false;
			}

			bool isFound = false;
			for (int i = 0; i < mList.Count; i++)
			{
				if (mList[i].Name == categoryName)
				{
					isFound = true;
					mCurrentCategory = i;
					break;
				}
			}

			if (!isFound)
			{
				FileCategory category = new FileCategory(categoryName);
				mList.Add(category);

				mCurrentCategory = mList.Count - 1;
			}

			FileGroup group1 = getCurrentCategory().Files.getSubGroup(mainOrder);
			if ( null == group1 )
			{
				getCurrentCategory().Files.addSubGroup(mainOrder);
				group1 = getCurrentCategory().Files.getSubGroup(mainOrder);
			}
			else
			{
				getCurrentCategory().Files.setCurrentGroup(mainOrder);
			}
			FileGroup group2 = group1.getSubGroup(subOrder);
			if ( null != group2 )
			{
				return false;
			}
			group1.addSubGroup(subOrder);
			return true;
		}

		protected bool continueFileGroup(string text)
		{
			string categoryName = "";
			int mainOrder = -1;
			int subOrder = -1;

			if (!getCategoryInfo(text, ref categoryName, ref mainOrder, ref subOrder))
			{
				return false;
			}

			for (int i = 0; i < mList.Count; i++)
			{
				if (mList[i].Name == categoryName)
				{
					int old = mCurrentCategory;
					mCurrentCategory = i;
					FileGroup group1 = getCurrentCategory().Files.getSubGroup(mainOrder);
					if (null == group1)
					{
						mCurrentCategory = old;
						return false;
					}
					FileGroup group2 = group1.getSubGroup(subOrder);
					if (null == group2)
					{
						mCurrentCategory = old;
						return false;
					}

					getCurrentCategory().Files.setCurrentGroup(mainOrder);
					group1.setCurrentGroup(subOrder);
					return true;
				}
			}
			return false;
		}

		private FileCategory getCurrentCategory()
		{
			if ( mCurrentCategory >= 0 && mCurrentCategory < mList.Count )
			{
				return mList[mCurrentCategory];
			}
			return null;
		}

		protected static bool getCategoryInfo(string text, ref string categoryName, ref int mainOrder, ref int subOrder)
		{
			try
			{
				int lessThan = text.IndexOf("<");
				int moreThan = text.IndexOf(">");

				if (-1 != lessThan && -1 != moreThan && (lessThan + 1) < moreThan)
				{
					categoryName = text.Substring(lessThan + 1, moreThan - lessThan - 1);
				}
				else
				{
					return false;
				}

				lessThan = text.IndexOf("<", moreThan);
				moreThan = text.IndexOf(">", moreThan + 1);
				if (-1 != lessThan && -1 != moreThan && (lessThan + 1) < moreThan)
				{
					string order = text.Substring(lessThan + 1, moreThan - lessThan - 1);
					mainOrder = int.Parse(order);
				}
				else
				{
					return false;
				}

				lessThan = text.IndexOf("<", moreThan);
				moreThan = text.IndexOf(">", moreThan + 1);
				if (-1 != lessThan && -1 != moreThan && (lessThan + 1) < moreThan)
				{
					string order = text.Substring(lessThan + 1, moreThan - lessThan - 1);
					subOrder = int.Parse(order);
				}
				else
				{
					return false;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				return false;
			}
			return true;
		}

        public bool collect(Int32 fileId, List<int> remap)
		{
            remap.Add(fileId);
			return true;
        }

        #endregion
    }
}
