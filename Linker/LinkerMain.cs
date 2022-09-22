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
            // - Platform     NDS                                           (NDS/WII/PSP/PS2/PS3/XBOX/X360/PC)
            // - Target       NDS                                           (NDS/WII/PSP/PS2/PS3/XBOX/X360/PC)
            // - Config       Config.%PLATFORM%.cs
            // - Name         Game
            // - Territory    Europe                                        (Europe/USA/Asia/Japan)
            // - SrcPath      I:\Dev\Game\Data\Data
            // - Root         Root.cs
            // - DstPath      %SrcPath%\Bin.%PLATFORM%
            // - PubPath      %SrcPath%\Publish.%PLATFORM%
            // - ToolPath     %SrcPath%\Tools
            if (!BuildSystemCompilerConfig.Init(cmdLine["name"], cmdLine["config"], cmdLine.HasParameter("bigfile"), cmdLine["platform"], cmdLine["target"], cmdLine["territory"], cmdLine["srcpath"], cmdLine["folder"], cmdLine["dstpath"], cmdLine["deppath"], cmdLine["toolpath"], cmdLine["pubpath"]))
            {
                Console.WriteLine("Usage: -name [NAME]");
                Console.WriteLine("       -config [FILENAME]");
                Console.WriteLine("       -platform [PLATFORM]");
                Console.WriteLine("       -target [PLATFORM]");
                Console.WriteLine("       -territory [Europe/USA/Asia/Japan]");
                Console.WriteLine("       -srcpath [SRCPATH]");
                Console.WriteLine("       -dstpath [DSTPATH]");
                Console.WriteLine("       -deppath [DSTDEPPATH]");
                Console.WriteLine("       -toolpath [TOOLPATH]");
                Console.WriteLine("       -pubpath [PUBLISHPATH]");
                Console.WriteLine();
                Console.WriteLine("Optional:");
                Console.WriteLine("       -bigfile => If not given then only the BigfileToc and FilenameDb will be build.");
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
                List<Filename> referencedAssemblies = new List<Filename>();
                referencedAssemblies.Add(new Filename(Assembly.GetExecutingAssembly().Location));

                List<Filename> configSrcFiles = new List<Filename>();
                configSrcFiles.Add(new Filename(GameCore.Environment.expandVariables(BuildSystemCompilerConfig.ConfigFilename)));
                Filename configAsmFilename = new Filename(BuildSystemCompilerConfig.Name + "Linker.Config.dll");
                Filename configAssemblyFilename = configAsmFilename;
                configDataAssembly = AssemblyCompiler.Compile(configAssemblyFilename, configSrcFiles.ToArray(), new Filename[0], BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.DepPath, referencedAssemblies.ToArray());
            }

            // BuildSystem.DataCompiler configuration
            IBuildSystemCompilerConfig configForCompiler = AssemblyUtil.Create1<IBuildSystemCompilerConfig>(configDataAssembly);
            if (configForCompiler != null)
                BuildSystemCompilerConfig.Init(configForCompiler);

            // Bigfile configuration
            IBigfileConfig configForBigfileBuilder = AssemblyUtil.Create1<IBigfileConfig>(configDataAssembly);
            if (configForBigfileBuilder != null)
                BigfileConfig.Init(configForBigfileBuilder);
            Filename bigFilename = new Filename(BuildSystemCompilerConfig.Name);

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

            Filename mainNodeFile = new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileNodeExtension);
            Filename fileEntriesCacheFilename = new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileTocExtension + ".cache");

            Dictionary<Filename, FileEntry> loadedFileEntries;
            LoadFileEntryCache(BuildSystemCompilerConfig.DstPath + fileEntriesCacheFilename, out loadedFileEntries);
            UpdateFileEntryCache(BuildSystemCompilerConfig.DstPath, loadedFileEntries);

            Queue<Filename> allNodeFilenames = new Queue<Filename>();
            Dictionary<Filename, FileEntry> currentFileEntries = new Dictionary<Filename, FileEntry>(loadedFileEntries.Count);
            allNodeFilenames.Enqueue(mainNodeFile);
            while (allNodeFilenames.Count > 0)
            {
                Filename subNodeFile = allNodeFilenames.Dequeue();
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
            //  - The bigfile file
            // IN
            //  - All node files
            // OUT
            //  - BigfileToc
            //  - BigfileFdb
            //  - BigfileHdb
            Filename bigfileFilename = new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileTocExtension);
            DepFile depFile = new DepFile(bigfileFilename, BuildSystemCompilerConfig.PubPath);
            if (!depFile.load(BuildSystemCompilerConfig.DepPath))
            {
                depFile.addIn(fileEntriesCacheFilename, BuildSystemCompilerConfig.DstPath);
                if (BuildSystemCompilerConfig.BuildBigfile)
                    depFile.addOut(new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileExtension), BuildSystemCompilerConfig.PubPath);
                depFile.addOut(new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileFdbExtension), BuildSystemCompilerConfig.PubPath);
                depFile.addOut(new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileHdbExtension), BuildSystemCompilerConfig.PubPath);
            }
            else
            {
                if (BuildSystemCompilerConfig.BuildBigfile)
                    depFile.addOut(new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileExtension), BuildSystemCompilerConfig.PubPath);
            }

            if (!allLoadedFileEntriesAreUsed || !allFileEntriesAreUpToDate || depFile.isModified())
            {
                BigfileBuilder bfb = new BigfileBuilder(BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DepPath, BuildSystemCompilerConfig.PubPath, bigfileFilename);
                foreach (FileEntry f in currentFileEntries.Values)
                    bfb.add(f.filename, f.contenthash);

                Console.WriteLine("Linking... (Bigfile={0})", BuildSystemCompilerConfig.BuildBigfile ? "Yes" : "No");
                if (bfb.save(BuildSystemCompilerConfig.Endian, BuildSystemCompilerConfig.BuildBigfile))
                {
                    if (!allLoadedFileEntriesAreUsed || !allFileEntriesAreUpToDate)
                        SaveFileEntryCache(BuildSystemCompilerConfig.DstPath + fileEntriesCacheFilename, currentFileEntries);

                    depFile.save(BuildSystemCompilerConfig.DepPath);
                }
            }
            else
            {
                Console.WriteLine("Up-to-date");
            }

            return Success();
        }

        internal class FileEntry
        {
            private Filename mFilename;
            public Int64 mFileSize;
            public Int64 mFileLastWriteTime;
            public Hash160 mFileContentHash;
            public bool mUsed;
            public bool mUpToDate;

            public FileEntry(Filename filename)
                : this(filename, 0, 0, Hash160.Empty)
            {
            }
            public FileEntry(Filename filename, Int64 filesize, Int64 filelastwritetime, Hash160 filecontenthash)
            {
                mFilename = filename;
                mFileSize = filesize;
                mFileLastWriteTime = filelastwritetime;
                mFileContentHash = filecontenthash;
                mUsed = false;
                mUpToDate = false;
            }

            public Filename filename
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

            public void update(Dirname path)
            {
                FileInfo fileinfo = new FileInfo(path + mFilename);
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

        public static bool LoadFileEntryCache(Filename inCacheFilename, out Dictionary<Filename, FileEntry> outFileEntries)
        {
            outFileEntries = new Dictionary<Filename, FileEntry>();

            xTextStream ts = new xTextStream(inCacheFilename);
            if (ts.Exists())
            {
                ts.Open(xTextStream.EMode.READ);
                while (true)
                {
                    string line = ts.read.ReadLine();
                    if (line == null)
                        break;

                    string[] items = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length == 4)
                    {
                        Filename filename = new Filename(items[0]);
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

        public static void UpdateFileEntryCache(Dirname dstPath, Dictionary<Filename, FileEntry> inFileEntries)
        {
            foreach (FileEntry f in inFileEntries.Values)
                f.update(dstPath);
        }

        public static bool SaveFileEntryCache(Filename inCacheFilename, Dictionary<Filename, FileEntry> inFileEntries)
        {
            xTextStream ts = new xTextStream(inCacheFilename);
            if (ts.Open(xTextStream.EMode.WRITE))
            {
                foreach(FileEntry f in inFileEntries.Values)
                {
                    ts.write.WriteLine("{0}={1:X16}={2:X16}={3:X32}", f.filename, f.filesize, f.fileLastWriteTime, f.contenthash);
                }
                ts.Close();
            }
            return true;
        }

        public static bool LoadNodeFile(Filename inNodeFilename, ref Queue<Filename> outNodeFilenames, Dictionary<Filename, FileEntry> inLoadedFileEntries, ref Dictionary<Filename, FileEntry> outCurrentFileEntries)
        {
            xTextStream ts = new xTextStream(inNodeFilename);
            ts.Open(xTextStream.EMode.READ);

            while (true)
            {
                string line = ts.read.ReadLine();
                if (line == null)
                    break;

                if (line.StartsWith("Node"))
                {
                    string[] keyValuePair = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    outNodeFilenames.Enqueue(new Filename(keyValuePair[1]));
                }
                else if (line.StartsWith("File"))
                {
                    string[] keyValuePair = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    Filename filename = new Filename(keyValuePair[1]);
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

