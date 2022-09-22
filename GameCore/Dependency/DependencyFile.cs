using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GameCore
{
    public class DepFile
    {
        #region Fields

        private string mUserKey = string.Empty;
        private string mExtension = string.Empty;
        private readonly DepInfo mMain;             /// The key/main file
        private readonly List<DepInfo> mIn;         /// Additional input files
        private readonly List<DepInfo> mOut;        /// The generated output files
        private readonly Dictionary<string, int> mInMap;
        private readonly Dictionary<string, int> mOutMap;

        public enum EDependencyState
        {
            NONE = 0,
            LOADED = 1,
            CREATED = 2,
        }

        private EDependencyState mState = EDependencyState.CREATED;
        private bool mUpdated = false;

        #endregion
        #region Constructors

        private DepFile() : this(Filename.Empty, Dirname.Empty)
        {
        }

        public DepFile(Filename filename, Dirname folder)
        {
            mMain = new DepInfo(filename, folder);
            mExtension = ".dep";

            mIn = new List<DepInfo>();
            mOut = new List<DepInfo>();

            mInMap = new Dictionary<string, int>();
            mOutMap = new Dictionary<string, int>();
        }

        #endregion
        #region Properties

        public DepInfo main
        {
            get
            {
                return mMain;
            }
        }

        public string userKey
        {
            set
            {
                mUserKey = value;
            }
            get
            {
                return mUserKey;
            }
        }

        public string extension
        {
            set
            {
                mExtension = value;
            }
            get
            {
                return mExtension;
            }
        }

        public bool loaded
        {
            get
            {
                return mState == EDependencyState.LOADED;
            }
            set
            {
                if (value)
                    mState = EDependencyState.LOADED;
                else
                    mState = EDependencyState.CREATED;
            }
        }

        public bool updated
        {
            get
            {
                return mUpdated;
            }
            set
            {
                mUpdated = value;
            }
        }

        public bool isModified()
        {
            if (mState == EDependencyState.CREATED)
                return true;

            if (mState == EDependencyState.LOADED)
                update();

            if (mMain.status != DepInfo.EStatus.UNCHANGED)
                return true;

            foreach (DepInfo d in mIn)
                if (d.status != DepInfo.EStatus.UNCHANGED)
                    return true;
            foreach (DepInfo d in mOut)
                if (d.status != DepInfo.EStatus.UNCHANGED)
                    return true;
            return false;
        }

        #endregion
        #region Private Methods

        private void addIn(DepInfo depInfoIN)
        {
            if (!mInMap.ContainsKey(depInfoIN.full))
            {
                mInMap.Add(depInfoIN.full, mIn.Count);
                mIn.Add(depInfoIN);
            }
        }

        private void addOut(DepInfo depInfoOUT)
        {
            if (!mOutMap.ContainsKey(depInfoOUT.full))
            {
                mOutMap.Add(depInfoOUT.full, mOut.Count);
                mOut.Add(depInfoOUT);
            }
        }

        internal bool save(StreamWriter writer)
        {
            try
            {
                writer.WriteLine("FILE={");
                {
                    writer.WriteLine("\tFILENAME={0}", mMain.filename);
                    writer.WriteLine("\tFOLDER={0}", mMain.folder);
                    writer.WriteLine("\tUSERKEY={0}", userKey);
                    writer.WriteLine("\tMETHOD={0}", mMain.method);
                    writer.WriteLine("\tRULE={0}", mMain.deprule);
                    writer.WriteLine("\tHASH={0}", mMain.hash);

                    foreach (DepInfo d in mIn)
                    {
                        writer.WriteLine("\tIN={");
                        writer.WriteLine("\t\tFILENAME={0}", d.filename);
                        writer.WriteLine("\t\tFOLDER={0}", d.folder);
                        writer.WriteLine("\t\tMETHOD={0}", d.method);
                        writer.WriteLine("\t\tRULE={0}", d.deprule);
                        writer.WriteLine("\t\tHASH={0}", d.hash);
                        writer.WriteLine("\t}");
                    }
                    foreach (DepInfo d in mOut)
                    {
                        writer.WriteLine("\tOUT={");
                        writer.WriteLine("\t\tFILENAME={0}", d.filename);
                        writer.WriteLine("\t\tFOLDER={0}", d.folder);
                        writer.WriteLine("\t\tMETHOD={0}", d.method);
                        writer.WriteLine("\t\tRULE={0}", d.deprule);
                        writer.WriteLine("\t\tHASH={0}", d.hash);
                        writer.WriteLine("\t}");
                    }
                }
                writer.WriteLine("}");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        #endregion
        #region Public Methods

        public void addIn(Filename filename, Dirname folder)
        {
            DepInfo di = new DepInfo(filename, folder);
            addIn(di);
        }
        public void addIn(Filename filename, Dirname folder, DepInfo.EDepRule rule)
        {
            DepInfo di = new DepInfo(filename, folder);
            di.deprule = rule;
            addIn(di);
        }
        public void addIn(Filename filename, Dirname folder, DepInfo.EMethod method)
        {
            DepInfo di = new DepInfo(filename, folder, method);
            addIn(di);
        }

        public void clearIn()
        {
            mIn.Clear();
            mInMap.Clear();
        }

        public void addOut(Filename filename, Dirname folder)
        {
            DepInfo di = new DepInfo(filename, folder);
            addOut(di);
        }
        public void addOut(Filename filename, Dirname folder, DepInfo.EDepRule rule)
        {
            DepInfo di = new DepInfo(filename, folder);
            di.deprule = rule;
            addOut(di);
        }
        public void addOut(Filename filename, Dirname folder, DepInfo.EMethod method)
        {
            DepInfo di = new DepInfo(filename, folder, method);
            addOut(di);
        }

        public void clearOut()
        {
            mOut.Clear();
            mOutMap.Clear();
        }

        public bool hasIn(Filename fullFilename)
        {
            int i;
            if (mInMap.TryGetValue(fullFilename, out i))
                return true;
            return false;
        }

        public bool hasOut(Filename fullFilename)
        {
            int i;
            if (mOutMap.TryGetValue(fullFilename, out i))
                return true;
            return false;
        }

        private bool init()
        {
            try
            {
                mMain.init();
                foreach (DepInfo d in mIn)
                    d.init();
                foreach (DepInfo d in mOut)
                    d.init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            mUpdated = true;
            return true;
        }

        internal bool update()
        {
            try
            {
                mMain.update();

                foreach (DepInfo d in mIn)
                    d.update();
                foreach (DepInfo d in mOut)
                    d.update();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            mUpdated = true;
            return true;
        }

        public bool delete(Dirname dstPath)
        {
            try
            {
                string depFilename = dstPath + "\\" + mMain.filename + extension;
                File.Delete(depFilename);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool save(Dirname dstPath)
        {
            try
            {
                if (!mUpdated)
                {
                    switch (mState)
                    {
                        case EDependencyState.CREATED:
                            init();
                            break;
                        case EDependencyState.LOADED:
                            update();
                            break;
                    }
                }

                Filename depFilename = dstPath + mMain.filename.PushedExtension(extension);

                // Directory exists at destination ?
                if (!Directory.Exists(depFilename.AbsolutePath))
                    Directory.CreateDirectory(depFilename.AbsolutePath);

                xTextStream ts = new xTextStream(depFilename);
                ts.Open(xTextStream.EMode.WRITE);
                save(ts.write);
                ts.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool load(Dirname dstPath)
        {
            clearIn();
            clearOut();

            try
            {
                FileInfo fileInfo = new FileInfo(dstPath + main.filename + extension);
                if (fileInfo.Exists == false)
                    return false;

                FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                StreamReader reader = new StreamReader(fileStream);

                bool ok = false;
                try
                {
                    ok = sReadSingle(reader, this);
                }
                catch(Exception e)
                {
                    clearIn();
                    clearOut();
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    reader.Close();
                    fileStream.Close();
                    ok = true;
                    loaded = true;
                }
                return ok;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool checkIfEqual(DepFile other)
        {
            if (!main.checkifEqual(other.main))
                return false;

            if (mIn.Count != other.mIn.Count)
                return false;

            if (mOut.Count != other.mOut.Count)
                return false;

            foreach (DepInfo d in mIn)
            {
                if (!other.mInMap.ContainsKey(d.filename))
                    return false;
            }

            foreach (DepInfo d in mOut)
            {
                if (!other.mOutMap.ContainsKey(d.filename))
                    return false;
            }

            return true;
        }

        #endregion
        #region Operators

        public static bool operator ==(DepFile a, DepFile b)
        {
            if ((object)a == null && (object)b == null)
                return true;
            if ((object)a == null || (object)b == null)
                return false;

            if (a.main == null && b.main == null)
                return true;
            
            if (a.main == null || b.main == null)
                return false;

            bool equal = false;
            if (a.main == b.main)
            {
                if (a.mIn.Count == b.mIn.Count)
                {
                    equal = true;
                    foreach (DepInfo ad in a.mIn)
                    {
                        foreach (DepInfo bd in b.mIn)
                        {
                            if (ad != bd)
                            {
                                equal = false;
                                break;
                            }
                        }
                        if (!equal)
                            break;
                    }
                }
            }
            return equal;
        }

        public static bool operator !=(DepFile a, DepFile b)
        {
            bool equal = (a == b);
            return !equal;
        }

        #endregion
        #region Public Static Methods

        public static DepFile sCreate(Filename filename, Dirname folder)
        {
            DepFile depFile = new DepFile(filename, folder);
            return depFile;
        }

        public static DepFile sCreate(Filename srcFilename, Dirname srcFolder, Filename dstFilename, Dirname dstFolder)
        {
            return sCreate(srcFilename, srcFolder, dstFilename, dstFolder, string.Empty);
        }

        public static DepFile sCreate(Filename srcFilename, Dirname srcFolder, Filename dstFilename, Dirname dstFolder, string userKey)
        {
            DepFile depFile = new DepFile(srcFilename, srcFolder);
            depFile.userKey = userKey;
            depFile.addOut(dstFilename, dstFolder);
            return depFile;
        }

        public static DepFile sCreate(Filename MainFilename, Dirname srcFolder, Filename[] OutFilenames, Dirname dstFolder)
        {
            return sCreate(MainFilename, new Filename[0], srcFolder, OutFilenames, dstFolder, string.Empty);
        }
        
        public static DepFile sCreate(Filename MainFilename, Dirname srcFolder, Filename[] OutFilenames, Dirname dstFolder, string userKey)
        {
            return sCreate(MainFilename, new Filename[0], srcFolder, OutFilenames, dstFolder, userKey);
        }

        public static DepFile sCreate(Filename MainFilename, Filename[] AdditionalInFilenames, Dirname srcFolder, Filename[] OutFilenames, Dirname dstFolder)
        {
            return sCreate(MainFilename, AdditionalInFilenames, srcFolder, OutFilenames, dstFolder, string.Empty);
        }

        public static DepFile sCreate(Filename MainFilename, Filename[] AdditionalInFilenames, Dirname srcFolder, Filename[] OutFilenames, Dirname dstFolder, string userKey)
        {
            DepFile depFile = new DepFile(MainFilename, srcFolder);
            try
            {
                depFile.userKey = userKey;

                if (AdditionalInFilenames != null)
                {
                    foreach (Filename i in AdditionalInFilenames)
                        depFile.addIn(i, srcFolder);
                }
                if (OutFilenames != null)
                {
                    foreach (Filename i in OutFilenames)
                        depFile.addOut(i, dstFolder);
                }

                return depFile;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return depFile;
            }
        }

        #endregion
        #region Private Static Methods

        private static bool sReadSingle(StreamReader reader, DepFile depfile)
        {
            bool readOneDepFile = false;

            Filename currentDepFile_filename = Filename.Empty;
            Dirname currentDepFile_folder = Dirname.Empty;

            bool currentDepInfoOpen = false;
            bool currentDepInfoIsIn = false;
            Filename currentDepInfo_filename = Filename.Empty;
            Dirname currentDepInfo_folder = Dirname.Empty;
            DepInfo.EMethod currentDepInfo_method = DepInfo.EMethod.TIMESTAMP;
            DepInfo.EDepRule currentDepInfo_rule = DepInfo.EDepRule.ON_CHANGE;
            Hash128 currentDepInfo_hash = Hash128.Empty;

            while (!reader.EndOfStream && !readOneDepFile)
            {
                string line = reader.ReadLine();
                line = line.Trim(' ', '\t');

                if (line.StartsWith("FILE="))
                {

                }
                else if (line.StartsWith("IN="))
                {
                    currentDepInfoOpen = true;
                    currentDepInfoIsIn = true;
                }
                else if (line.StartsWith("OUT="))
                {
                    currentDepInfoOpen = true;
                    currentDepInfoIsIn = false;
                }
                else if (line.StartsWith("FILENAME="))
                {
                    string[] lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        string name = lineParts[1].TrimStart(' ', '\t');
                        if (currentDepInfoOpen)
                        {
                            currentDepInfo_filename = new Filename(name);
                        }
                        else if (depfile != null)
                        {
                            currentDepFile_filename = new Filename(name);
                        }
                    }
                }
                else if (line.StartsWith("USERKEY="))
                {
                    string[] lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        string userKey = lineParts[1].TrimStart(' ', '\t');
                        if (depfile != null)
                            depfile.userKey = userKey;
                    }
                }
                else if (line.StartsWith("METHOD="))
                {
                    string[] lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        string modeStr = lineParts[1].TrimStart(' ', '\t');
                        DepInfo.EMethod method = (DepInfo.EMethod)Enum.Parse(typeof(DepInfo.EMethod), modeStr, true);
                        if (currentDepInfoOpen)
                        {
                            currentDepInfo_method = method;
                        }
                        else if (depfile != null)
                        {
                            depfile.main.method = method; 
                        }
                    }
                }
                else if (line.StartsWith("FOLDER="))
                {
                    string[] lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        string name = lineParts[1].TrimStart(' ', '\t');
                        if (currentDepInfoOpen)
                        {
                            currentDepInfo_folder = new Dirname(name);
                        }
                        else if (depfile != null)
                        {
                            currentDepFile_folder = new Dirname(name);
                        }
                    }
                }
                else if (line.StartsWith("HASH="))
                {
                    string[] lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        string hashStr = lineParts[1].Trim(' ', '\t');
                        if (currentDepInfoOpen)
                            currentDepInfo_hash = Hash128.FromString(hashStr);
                        else if (depfile != null)
                            depfile.main.hash = Hash128.FromString(hashStr);
                    }
                }
                else if (line.StartsWith("RULE="))
                {
                    string[] lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        string ruleStr = lineParts[1].Trim(' ', '\t');
                        DepInfo.EDepRule rule = (DepInfo.EDepRule)Enum.Parse(typeof(DepInfo.EDepRule), ruleStr, true);
                        if (currentDepInfoOpen)
                            currentDepInfo_rule = rule;
                        else if (depfile != null)
                            depfile.main.deprule = rule;
                    }
                }
                else if (line == "}")
                {
                    if (currentDepInfoOpen)
                    {
                        currentDepInfoOpen = false;
                        DepInfo depInfo = new DepInfo(currentDepInfo_filename, currentDepInfo_folder, currentDepInfo_method, currentDepInfo_hash);
                        depInfo.deprule = currentDepInfo_rule;
                        if (currentDepInfoIsIn)
                            depfile.addIn(depInfo);
                        else
                            depfile.addOut(depInfo);

                        currentDepInfo_filename = Filename.Empty;
                        currentDepInfo_folder = Dirname.Empty;
                        currentDepInfo_method = DepInfo.EMethod.TIMESTAMP;
                        currentDepInfo_rule = DepInfo.EDepRule.ON_CHANGE;
                        currentDepInfo_hash = Hash128.Empty;
                    }
                    else
                    {
                        depfile.main.setFilenameAndFolder(currentDepFile_filename, currentDepFile_folder);
                        readOneDepFile = true;
                    }
                }
            }

            return readOneDepFile;
        }

        internal static bool sReadMulti(string filename, Dirname path, out DepFile[] depFiles)
        {
            return sReadMulti(filename, string.Empty, path, out depFiles);
        }

        internal static List<DepFile> sReadMulti(StreamReader reader)
        {
            List<DepFile> readDepFiles = new List<DepFile>();
            while (!reader.EndOfStream)
            {
                DepFile d = new DepFile();
                if (!sReadSingle(reader, d))
                    break;
                readDepFiles.Add(d);
            }
            return readDepFiles;
        }

        internal static bool sReadMulti(string filename, string extension, Dirname path, out DepFile[] depFiles)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path + "\\" + filename + extension);
                if (fileInfo.Exists == false)
                {
                    depFiles = new DepFile[0];
                    return true;
                }

                FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                StreamReader reader = new StreamReader(fileStream);

                List<DepFile> readDepFiles = sReadMulti(reader);
                depFiles = readDepFiles.ToArray();

                reader.Close();
                fileStream.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                depFiles = null;
                return false;
            }
        }
        #endregion
        #region Equals, GetHashCode

        public override bool Equals(object obj)
        {
            DepFile depFile = (DepFile)obj;
            if (depFile.main != main)
                return false;

            if (mIn.Count != depFile.mIn.Count)
                return false;

            foreach (DepInfo d in mIn)
            {
                bool equal = false;
                foreach (DepInfo dd in depFile.mIn)
                {
                    if (d == dd)
                    {
                        equal = true;
                        break;
                    }
                }
                if (!equal)
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return main.GetHashCode();
        }

        #endregion
    }

}