#region Copyright
/// 
/// BuildSystem.Data.Linker
/// Copyright (C) 2009 J.J.Kluft
/// 
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU General Public License for more details.
/// 
/// You should have received a copy of the GNU General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
///
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using GameCore;

namespace DataBuildSystem
{
    // Vocabulary:
    // '.NET BuildSystem's DataCompiler' or in short 'DataCompiler'
    // '.NET BuildSystem's DataLinker' or in short 'DataLinker'

    class Program
    {
        #region Error & Success

        static int Error()
        {
            return 1;
        }

        static int Success()
        {
            return 0;
        }

        #endregion

        //
        // Load the $(name).bfn (Bigfile node) file that contains information of the content of
        // that Bigfile and also which sub nodes. Load every .bfn file recursively until we have
        // obtained all assets.
        // 
        // All the .bfn files are the IN of our dependency file and the Bigfile, BigfileToc, BigfileHdb, 
        // BigfileFdb are our OUT files.
        // 
        // TODO:
        // - Build cache files for every Node so that we do not have to collect all individual files for every unchanged node.
        // 
        static int Main(string[] args)
        {
            CommandLine cmdLine = new CommandLine(args);

			// On the command-line we have:
			// - platform     PC                                            (PS4/PS5/XBOXONE/XSX/PC)
			// - target       PC                                            (PS4/PS5/XBOXONE/XSX/PC)
			// - name         Game
			// - territory    Europe                                        (Europe/USA/Asia/Japan)
			// - basepath     i:\Data                                       (Can be set and used as %BASEPATH%)
			// - gddpath      %BASEPATH%\Gdd\Compiled
			// - srcpath      %BASEPATH%\Assets
			// - subpath                                                    (AI, Boot, Levels, Menu\FrontEnd)
			// - dstpath      %BASEPATH%\Bin.%PLATFORM%.%TARGET%
			// - pubpath      %BASEPATH%\Publish.%PLATFORM%.%TARGET%
			// - toolpath     %BASEPATH%\Tools
			if (!BuildSystemCompilerConfig.Init(cmdLine["name"], cmdLine["platform"], cmdLine["target"], cmdLine["territory"], cmdLine["basepath"], cmdLine["srcpath"], cmdLine["gddpath"], cmdLine["subpath"], cmdLine["dstpath"], cmdLine["pubpath"], cmdLine["toolpath"]))
            {
				Console.WriteLine("Usage: -name [NAME]");
				Console.WriteLine("       -platform [PLATFORM]");
				Console.WriteLine("       -target [PLATFORM]");
				Console.WriteLine("       -territory [Europe/USA/Asia/Japan]");
				Console.WriteLine("       -basepath [BASEPATH]");
				Console.WriteLine("       -gddpath [GDDPATH]");
				Console.WriteLine("       -srcpath [SRCPATH]");
				Console.WriteLine("       -subpath [SUBPATH]");
				Console.WriteLine("       -dstpath [DSTPATH]");
				Console.WriteLine("       -pubpath [PUBLISHPATH]");
				Console.WriteLine("       -toolpath [TOOLPATH]");
                Console.WriteLine();
                Console.WriteLine("Press a key");
                Console.ReadKey();
                return 1;
            }

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("------ DataBuildSystem.NET - Linker: v{0} (Platform: {1}, Target: {2}) ------", version, BuildSystemCompilerConfig.PlatformName, BuildSystemCompilerConfig.TargetName);

            Assembly configDataAssembly = null;
            if (!BuildSystemCompilerConfig.ConfigFilename.IsEmpty)
            {
                List<string> referencedAssemblies = new List<string>();
                referencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

                List<string> configSrcFiles = new List<string>();
                configSrcFiles.Add((GameCore.Environment.expandVariables(BuildSystemCompilerConfig.ConfigFilename)));
                string configAsmFilename = Path.Join(BuildSystemCompilerConfig.Name, "Linker.Config.dll");
                string configAssemblyFilename = configAsmFilename;
                configDataAssembly = AssemblyCompiler.Compile(configAssemblyFilename, configSrcFiles.ToArray(), new string[0], BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.DepPath, referencedAssemblies.ToArray());
            }

            // BuildSystem.DataCompiler configuration
            IBuildSystemCompilerConfig configForCompiler = AssemblyUtil.Create1<IBuildSystemCompilerConfig>(configDataAssembly);
            if (configForCompiler != null)
                BuildSystemCompilerConfig.Init(configForCompiler);

            // Bigfile configuration
            IBigfileConfig configForBigfileBuilder = AssemblyUtil.Create1<IBigfileConfig>(configDataAssembly);
            if (configForBigfileBuilder != null)
                BigfileConfig.Init(configForBigfileBuilder);
            string bigFilename = BuildSystemCompilerConfig.Name;

            // Dependency system configuration
            /// IDependencySystemConfig configForDependencySystem = AssemblyUtil.Create1<IDependencySystemConfig>(configDataAssembly);
            /// if (configForDependencySystem != null)
            ///     DependencySystemConfig.Init(configForDependencySystem);


            /// Recursively load all node files from NODE items and collect FILE items
            /// 
            /// Build a dependency file for the BigfileToc creation with:
            ///  - IN
            ///    NODE items
            ///  - OUT
            ///    Bigfile, BigfileToc, BigfileFdb, BigfileHdb
            ///    
            /// Build a dependency file for the Bigfile creation with
            ///  - IN 
            ///    assets
            ///  - OUT
            ///    bigfile
            /// 
            /// Rebuilding the Bigfile means rebuilding the BigfileToc due to the file offsets.
            /// 
            /// Combine all FILE items into the main BigfileToc
            ///   - Optimization: Resolve all FileIds to an index instead of a 128 bit hash, this means 
            ///     modifying game data files for modyfing the 128 bit hash to an index.
            /// Combine all assets into the Bigfile
            ///   - Optimization: Cache all assets for every node, to speed up building the full bigfile
            ///   
            /// Inner workings:
            ///  - FileIds = 128 bit string hash of the filename
            ///  - 
            /// 

            string mainNodeFile = Path.Join(BuildSystemCompilerConfig.Name , BigfileConfig.BigFileNodeExtension);
            string fileEntriesCacheFilename = Path.Join(BuildSystemCompilerConfig.Name, BigfileConfig.BigFileTocExtension + ".cache");

            Dictionary<string, FileEntry> loadedFileEntries;
            LoadFileEntryCache(BuildSystemCompilerConfig.DstPath + fileEntriesCacheFilename, out loadedFileEntries);
            UpdateFileEntryCache(BuildSystemCompilerConfig.DstPath, loadedFileEntries);

            Queue<string> allNodeFilenames = new ();
            Dictionary<string, FileEntry> currentFileEntries = new(loadedFileEntries.Count);
            allNodeFilenames.Enqueue(mainNodeFile);
            while (allNodeFilenames.Count > 0)
            {
                string subNodeFile = allNodeFilenames.Dequeue();
                LoadNodeFile(BuildSystemCompilerConfig.DstPath + subNodeFile, ref allNodeFilenames, loadedFileEntries, ref currentFileEntries);
            }

            // See if all are up-to-date, if so then we do not have to build.
            bool allFileEntriesAreUpToDate = true;
            foreach (FileEntry f in currentFileEntries.Values)
            {
                if (!f.uptodate)
                {
                    allFileEntriesAreUpToDate = false;
                    f.update(BuildSystemCompilerConfig.DstPath);
                }
            }

            // See if all the loaded file entries are used, it not then we are sure that we have to save the cache file
            bool allLoadedFileEntriesAreUsed = true;
            foreach (FileEntry f in loadedFileEntries.Values)
            {
                if (!f.used)
                {
                    allLoadedFileEntriesAreUsed = false;
                    break;
                }
            }

            // Create a dependency file with:
            // MAIN
            //  - The large bigfile file
            // IN
            //  - All bigfiles (.bfd, .bft, .bff, .bfh)
            // OUT
            //  - BigfileToc
            //  - BigfileFdb
            //  - BigfileHdb
            string bigfileFilename = Path.Join(BuildSystemCompilerConfig.Name, BigfileConfig.BigFileTocExtension);
            DepFile depFile = new DepFile(bigfileFilename, BuildSystemCompilerConfig.PubPath);
            if (!depFile.load(BuildSystemCompilerConfig.DepPath))
            {
                depFile.addIn(fileEntriesCacheFilename, BuildSystemCompilerConfig.DstPath);
                if (BuildSystemCompilerConfig.BuildBigfile)
                    depFile.addOut(Path.Join(BuildSystemCompilerConfig.Name , BigfileConfig.BigFileExtension), BuildSystemCompilerConfig.PubPath);
                depFile.addOut(Path.Join(BuildSystemCompilerConfig.Name , BigfileConfig.BigFileFdbExtension), BuildSystemCompilerConfig.PubPath);
                depFile.addOut(Path.Join(BuildSystemCompilerConfig.Name , BigfileConfig.BigFileHdbExtension), BuildSystemCompilerConfig.PubPath);
            }
            else
            {
                if (BuildSystemCompilerConfig.BuildBigfile)
                    depFile.addOut(Path.Join(BuildSystemCompilerConfig.Name , BigfileConfig.BigFileExtension), BuildSystemCompilerConfig.PubPath);
            }

            if (!allLoadedFileEntriesAreUsed || !allFileEntriesAreUpToDate || depFile.isModified())
            {
                //BigfileBuilder bfb = new BigfileBuilder(BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DepPath, BuildSystemCompilerConfig.PubPath, bigfileFilename);

                // TODO Linker's purpose is to merge Bigfiles into one large Bigfile

                //foreach (FileEntry f in currentFileEntries.Values)
                //    bfb.Add(f.filename, f.filesize);

                // Console.WriteLine("Linking... (Bigfile={0})", BuildSystemCompilerConfig.BuildBigfile ? "Yes" : "No");
                // if (bfb.Save(BuildSystemCompilerConfig.Endian, BuildSystemCompilerConfig.BuildBigfile))
                // {
                //     if (!allLoadedFileEntriesAreUsed || !allFileEntriesAreUpToDate)
                //         SaveFileEntryCache(BuildSystemCompilerConfig.DstPath + fileEntriesCacheFilename, currentFileEntries);
                //     depFile.save(BuildSystemCompilerConfig.DepPath);
                // }
            }
            else
            {
                Console.WriteLine("Up-to-date");
            }

            return Success();
        }

        internal class FileEntry
        {
            private string mFilename;
            public Int64 mFileSize;
            public Int64 mFileLastWriteTime;
            public Hash160 mFileContentHash;
            public bool mUsed;
            public bool mUpToDate;

            public FileEntry(string filename)
                : this(filename, 0, 0, Hash160.Empty)
            {
            }
            public FileEntry(string filename, Int64 filesize, Int64 filelastwritetime, Hash160 filecontenthash)
            {
                mFilename = filename;
                mFileSize = filesize;
                mFileLastWriteTime = filelastwritetime;
                mFileContentHash = filecontenthash;
                mUsed = false;
                mUpToDate = false;
            }

            public string filename
            {
                get { return mFilename; }
                set { mFilename = value; }
            }
            public Int64 filesize
            {
                get { return mFileSize; }
                set { mFileSize = value; }
            }
            public Int64 fileLastWriteTime
            {
                get { return mFileLastWriteTime; }
                set { mFileLastWriteTime = value; }
            }
            public Hash160 contenthash
            {
                get { return mFileContentHash; }
                set { mFileContentHash = value; }
            }
            public bool used
            {
                get { return mUsed; }
                set { mUsed = value; }
            }
            public bool uptodate
            {
                get { return mUpToDate; }
            }

            public void update(string path)
            {
                FileInfo fileinfo = new FileInfo(Path.Join(path, mFilename));
                if (!fileinfo.Exists)
                {
                    mFileSize = 0;
                    mFileLastWriteTime = 0;
                    mFileContentHash = Hash160.Empty;
                    mUpToDate = false;
                }
                else
                {
                    mUpToDate = (fileinfo.LastWriteTime.Ticks == mFileLastWriteTime);
                    if (!mUpToDate)
                    {
                        mFileSize = fileinfo.Length;
                        mFileLastWriteTime = fileinfo.LastWriteTime.Ticks;
                        mFileContentHash = HashUtility.compute(fileinfo);
                    }
                }
            }
        }

        public static bool LoadFileEntryCache(string inCacheFilename, out Dictionary<string, FileEntry> outFileEntries)
        {
            outFileEntries = new Dictionary<string, FileEntry>();

            TextStream ts = new TextStream(inCacheFilename);
            if (ts.Exists())
            {
                ts.Open(TextStream.EMode.READ);
                while (true)
                {
                    string line = ts.read.ReadLine();
                    if (line == null)
                        break;

                    string[] items = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length == 4)
                    {
                        string filename = items[0];
                        Int64 filesize = StringTools.HexToInt64(items[1]);
                        Int64 filelastwritetime = StringTools.HexToInt64(items[2]);
                        Hash160 filemd5 = Hash160.FromString(items[3]);
                        outFileEntries.Add(filename, new FileEntry(filename, filesize, filelastwritetime, filemd5));
                    }
                }
                ts.Close();
            }
            return true;
        }

        public static void UpdateFileEntryCache(string dstPath, Dictionary<string, FileEntry> inFileEntries)
        {
            foreach (FileEntry f in inFileEntries.Values)
                f.update(dstPath);
        }

        public static bool SaveFileEntryCache(string inCacheFilename, Dictionary<string, FileEntry> inFileEntries)
        {
            TextStream ts = new TextStream(inCacheFilename);
            if (ts.Open(TextStream.EMode.WRITE))
            {
                foreach(FileEntry f in inFileEntries.Values)
                {
                    ts.write.WriteLine("{0}={1:X16}={2:X16}={3:X32}", f.filename, f.filesize, f.fileLastWriteTime, f.contenthash);
                }
                ts.Close();
            }
            return true;
        }

        public static bool LoadNodeFile(string inNodeFilename, ref Queue<string> outNodeFilenames, Dictionary<string, FileEntry> inLoadedFileEntries, ref Dictionary<string, FileEntry> outCurrentFileEntries)
        {
            TextStream ts = new TextStream(inNodeFilename);
            ts.Open(TextStream.EMode.READ);

            while (true)
            {
                string line = ts.read.ReadLine();
                if (line == null)
                    break;

                if (line.StartsWith("Node"))
                {
                    string[] keyValuePair = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    outNodeFilenames.Enqueue(keyValuePair[1]);
                }
                else if (line.StartsWith("File"))
                {
                    string[] keyValuePair = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    string filename = keyValuePair[1];
                    FileEntry fileentry;
                    if (inLoadedFileEntries.TryGetValue(filename, out fileentry))
                    {
                        fileentry.used = true;
                    }
                    else
                    {
                        fileentry = new FileEntry(filename);
                    }
                    outCurrentFileEntries.Add(filename, fileentry);
                }
            }

            ts.Close();
            return true;
        }
    }
}

