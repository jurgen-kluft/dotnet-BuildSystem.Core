using System;
using System.IO;
using System.Collections.Generic;
using Core;

namespace DataBuildSystem
{
    /// <summary>
    /// The dependency system implementation for a machine, local or remote
    /// </summary>
    internal class DependencySystemImp
    {
        #region Fields

        /// <summary>
        /// Our own string comparer since we compare filenames in LowerCase due
        /// to possible case differences as to how they are entered in the .cs 
        /// files and retrieved from the FileSystem.
        /// </summary>
        public class FilenameInsensitiveComparer : IEqualityComparer<Filename>
        {
            public bool Equals(Filename x, Filename y)
            {
                return x == y;
            }

            public int GetHashCode(Filename obj)
            {
                return obj.GetHashCode();
            }
        }

        private readonly Dirname mSrcPath;
        private readonly Dirname mDstPath;
        private readonly Dirname mDstDepPath;

        private readonly Dictionary<Filename, DepFile> mDepFileDictionary;
        private bool mAnythingModified;

        #endregion
        #region Constructor

        public DependencySystemImp(Dirname srcPath, Dirname dstPath, Dirname dstDepPath)
        {
            mSrcPath = srcPath;
            mDstPath = dstPath;
            mDstDepPath = dstDepPath;
            mDepFileDictionary = new Dictionary<Filename, DepFile>();
        }

        #endregion
        #region Properties

        public Dirname srcPath
        {
            get
            {
                return mSrcPath;
            }
        }

        public Dirname dstPath
        {
            get
            {
                return mDstPath;
            }
        }

        public Dirname depPath
        {
            get
            {
                return mDstDepPath;
            }
        }

        public bool anythingModified
        {
            get
            {
                return mAnythingModified;
            }
        }

        #endregion
        #region Methods

        public bool findDepFile(Filename filename, out DepFile depFile)
        {
            return mDepFileDictionary.TryGetValue(filename, out depFile);
        }

        private static bool FilterDelegate(DirectoryScanner.FilterEvent e)
        {
            return e.isFolder ? DependencySystemConfig.FolderFilter(e.name) : DependencySystemConfig.FileFilter(e.name); 
        }

        /// <summary>
        /// Load the dependency files
        /// </summary>
        /// <returns>True if load was successful</returns>
        public bool build()
        {
            mDepFileDictionary.Clear();

            // The dependency files are in the dstDepPath and here we will iterate
            // over all files in this directory and it's subdirectories.
            DirectoryScanner scanner = new DirectoryScanner(depPath);
            scanner.scanSubDirs = true;
            string extension = "*" + DependencySystemConfig.Extension;
            if (!scanner.collect(Dirname.Empty, extension, FilterDelegate))
                return false;

            List<Filename> depFilenames = scanner.filenames;
            foreach (Filename depFilename in depFilenames)
            {
                Filename mainFilename = depFilename.PoppedExtension();

                // TODO: Could be loaded async!

                DepFile[] depFiles;
                if (DepFile.sReadMulti(mainFilename, depPath, out depFiles))
                {
                    foreach (DepFile d in depFiles)
                    {
                        d.loaded = true;
                        mDepFileDictionary.Add(mainFilename, d);
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: The DepFile {0} failed to load!", mainFilename);
                }
            }

            // Successful
            return true;
        }

        /// <summary>
        /// Load all dependency files from one file
        /// </summary>
        /// <param name="masterDepfileFilename"></param>
        public bool loadCache(Filename masterDepfileFilename)
        {
            masterDepfileFilename.Extension = DependencySystemConfig.Extension;

            FileInfo fileInfo = new FileInfo(masterDepfileFilename.ToString());
            if (!fileInfo.Exists)
                return false;

            FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader reader = new StreamReader(fileStream);

            List<DepFile> readDepFiles = DepFile.sReadMulti(reader);
            foreach(DepFile d in readDepFiles)
            {
                d.loaded = true;
                Filename main = d.main.filename;
                mDepFileDictionary.Add(main, d);                
            }

            reader.Close();
            fileStream.Close();
            return true;
        }

        public static DepFile[] sLoadDepFile(Filename srcFilename)
        {
            Filename depfileFilename = srcFilename.PushedExtension(DependencySystemConfig.Extension);

            FileInfo fileInfo = new FileInfo(depfileFilename);
            if (!fileInfo.Exists)
                return new DepFile[0];

            FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader reader = new StreamReader(fileStream);

            List<DepFile> readDepFiles = DepFile.sReadMulti(reader);
            foreach (DepFile d in readDepFiles)
                d.loaded = true;

            reader.Close();
            fileStream.Close();

            if (readDepFiles.Count == 0)
                return null;

            return readDepFiles.ToArray();
        }

        /// <summary>
        /// Save all dependency files into one file
        /// </summary>
        /// <param name="masterDepfileFilename"></param>
		public void saveCache(Filename masterDepfileFilename)
		{
            // Directory exists at destination ?
            DirectoryInfo dstDirInfo = new DirectoryInfo(masterDepfileFilename.AbsolutePath.ToString());
            if (!dstDirInfo.Exists)
                dstDirInfo.Create();

            FileInfo fileInfo = new FileInfo(masterDepfileFilename.ToString());
            FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Create);
            StreamWriter writer = new StreamWriter(fileStream);

            foreach (KeyValuePair<Filename, DepFile> pair in mDepFileDictionary)
            {
                DepFile d = pair.Value;
                d.save(writer);
            }

            writer.Flush();
            writer.Close();
            fileStream.Close();
		}

        public class AsyncUpdateScope
        {
            public volatile int mComplete;
            public volatile int mIncomplete;

            public void MarkIncomplete(Async objAsync)
            {
                mIncomplete++;
            }

            public void MarkComplete(Async objAsync)
            {
                mComplete++;
            }

            public bool Complete
            {
                get
                {
                    return mComplete == mIncomplete;
                }
            }
        }

        /// <summary>
        /// Update the status of all DepFiles
        /// </summary>
        public void update()
        {
#if  ASYNC
            {
                foreach (KeyValuePair<Filename, DepFile> pair in mDepFileDictionary)
                {
                    DepFile d = pair.Value;
                    if (d.loaded)
                    {
                        d.update(srcPath, dstPath);
                    }
                }
            }
#else
            {
                AsyncUpdateScope scope = new AsyncUpdateScope();
                foreach (KeyValuePair<Filename, DepFile> pair in mDepFileDictionary)
                {
                    DepFile d = pair.Value;
                    if (d.loaded)
                    {
                        Async a = new Async(delegate { d.updated = false; d.update(); }, scope.MarkComplete);
                        scope.MarkIncomplete(a);
                    }
                }

                while (scope.Complete == false)
                {
                }
            }
#endif
        }

        /// <summary>
        /// Add a dependency, mark it as 'used'.
        /// </summary>
        /// <param name="inDepFile">A dependency file to add</param>
        public bool add(DepFile inDepFile)
        {
            mAnythingModified = true;

            Filename main = inDepFile.main.filename;
            if (!mDepFileDictionary.ContainsKey(main))
                mDepFileDictionary.Add(main, inDepFile);
            else
                mDepFileDictionary[main] = inDepFile;

            return true;
        }

        /// <summary>
        /// Get a dependency file by main filename
        /// </summary>
        public DepFile get(Filename srcMainFilename)
        {
            DepFile d;
            if (mDepFileDictionary.TryGetValue(srcMainFilename, out d))
                return d;
            else
                return null;
        }

        /// <summary>
        /// Add a dependency, init it and save it
        /// </summary>
        /// <param name="inDepFile">A dependency file to add, init and save</param>
        public bool register(DepFile inDepFile)
        {
            bool result = false;
            if (add(inDepFile))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Set dependency, add it or update it
        /// </summary>
        /// <param name="mainFilename">The main file</param>
        /// <param name="outputFilename">The generated output file</param>
        public bool set(Filename mainFilename, Filename outputFilename)
        {
            DepFile depFile = DepFile.sCreate(mainFilename, srcPath, outputFilename, dstPath);
            add(depFile);
            return true;
        }

        /// <summary>
        /// Set dependency, add it or update it
        /// </summary>
        /// <param name="mainFilename">The main file</param>
        /// <param name="outputFilenames">The generated output files</param>
        public bool set(Filename mainFilename, List<Filename> outputFilenames)
        {
            DepFile depFile = DepFile.sCreate(mainFilename, srcPath, outputFilenames.ToArray(), dstPath);
            add(depFile);
            return true;
        }

        /// <summary>
        /// Set dependency, add it or update it
        /// </summary>
        /// <param name="mainFilename">The main file</param>
        /// <param name="inputFilenames">The additional input files</param>
        /// <param name="outputFilenames">The generated output files</param>
        public bool set(Filename mainFilename, List<Filename> inputFilenames, List<Filename> outputFilenames)
        {
            DepFile depFile = DepFile.sCreate(mainFilename, inputFilenames.ToArray(), srcPath, outputFilenames.ToArray(), dstPath);
            add(depFile);
            return true;
        }

        public bool remove(Filename mainFilename)
        {
            if (mDepFileDictionary.ContainsKey(mainFilename))
                mDepFileDictionary.Remove(mainFilename);

            return true;
        }

        /// <summary>
        /// Determines the modification status of a DepFile
        /// </summary>
        /// <param name="mainFilename">The main file</param>
        /// <returns>Returns True if any of the dependencies of a DepFile is not equal to UNCHANGED.</returns>
        public bool isModified(Filename mainFilename)
        {
            return isModified(mainFilename, string.Empty);
        }

        public bool isModified(Filename mainFilename, string userKey)
        {
            DepFile depFile;
            if (!mDepFileDictionary.TryGetValue(mainFilename, out depFile))
            {
                // No dependency file existed before for this file, so we can
                // only say with certainty that it has changed, since we have
                // no proof otherwise.
                return true;
            }

            return (userKey.Length != 0 && depFile.userKey != userKey) || depFile.isModified();
        }

        #endregion
    }
}
