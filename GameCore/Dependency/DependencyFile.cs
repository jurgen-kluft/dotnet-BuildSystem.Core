using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GameCore
{
    public class DepFile
    {
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

        private DepFile() : this(string.Empty, string.Empty)
        {
        }

        public DepFile(string filename, string folder)
        {
            mMain = new DepInfo(0, filename, folder);
            mExtension = ".dep";

            mIn = new List<DepInfo>();
            mOut = new List<DepInfo>();

            mInMap = new Dictionary<string, int>();
            mOutMap = new Dictionary<string, int>();
        }

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

            if (mMain.Status != DepInfo.EStatus.UNCHANGED)
                return true;

            foreach (var d in mIn)
                if (d.Status != DepInfo.EStatus.UNCHANGED)
                    return true;
            foreach (var d in mOut)
                if (d.Status != DepInfo.EStatus.UNCHANGED)
                    return true;
            return false;
        }

        private void addIn(DepInfo depInfoIN)
        {
            if (!mInMap.ContainsKey(depInfoIN.Full))
            {
                mInMap.Add(depInfoIN.Full, mIn.Count);
                mIn.Add(depInfoIN);
            }
        }

        private void addOut(DepInfo depInfoOUT)
        {
            if (!mOutMap.ContainsKey(depInfoOUT.Full))
            {
                mOutMap.Add(depInfoOUT.Full, mOut.Count);
                mOut.Add(depInfoOUT);
            }
        }

        internal bool save(System.IO.StreamWriter writer)
        {
            try
            {
                writer.WriteLine("FILE={");
                {
                    writer.WriteLine("\tFILENAME={0}", mMain.Filename);
                    writer.WriteLine("\tFOLDER={0}", mMain.Folder);
                    writer.WriteLine("\tUSERKEY={0}", userKey);
                    writer.WriteLine("\tMETHOD={0}", mMain.Method);
                    writer.WriteLine("\tRULE={0}", mMain.Rule);
                    writer.WriteLine("\tHASH={0}", mMain.Hash);

                    foreach (var d in mIn)
                    {
                        writer.WriteLine("\tIN={");
                        writer.WriteLine("\t\tFILENAME={0}", d.Filename);
                        writer.WriteLine("\t\tFOLDER={0}", d.Folder);
                        writer.WriteLine("\t\tMETHOD={0}", d.Method);
                        writer.WriteLine("\t\tRULE={0}", d.Rule);
                        writer.WriteLine("\t\tHASH={0}", d.Hash);
                        writer.WriteLine("\t}");
                    }
                    foreach (var d in mOut)
                    {
                        writer.WriteLine("\tOUT={");
                        writer.WriteLine("\t\tFILENAME={0}", d.Filename);
                        writer.WriteLine("\t\tFOLDER={0}", d.Folder);
                        writer.WriteLine("\t\tMETHOD={0}", d.Method);
                        writer.WriteLine("\t\tRULE={0}", d.Rule);
                        writer.WriteLine("\t\tHASH={0}", d.Hash);
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

        public void addIn(string filename, string folder)
        {
            var di = new DepInfo(0, filename, folder);
            addIn(di);
        }
        public void addIn(string filename, string folder, DepInfo.EDepRule rule)
        {
            var di = new DepInfo(0, filename, folder);
            di.Rule = rule;
            addIn(di);
        }
        public void addIn(string filename, string folder, DepInfo.EDepMethod method)
        {
            var di = new DepInfo(0, filename, folder, method);
            addIn(di);
        }

        public void clearIn()
        {
            mIn.Clear();
            mInMap.Clear();
        }

        public void addOut(string filename, string folder)
        {
            var di = new DepInfo(0, filename, folder);
            addOut(di);
        }
        public void addOut(string filename, string folder, DepInfo.EDepRule rule)
        {
            var di = new DepInfo(0, filename, folder);
            di.Rule = rule;
            addOut(di);
        }
        public void addOut(string filename, string folder, DepInfo.EDepMethod method)
        {
            var di = new DepInfo(0, filename, folder, method);
            addOut(di);
        }

        public void clearOut()
        {
            mOut.Clear();
            mOutMap.Clear();
        }

        public bool hasIn(string fullFilename)
        {
            int i;
            if (mInMap.TryGetValue(fullFilename, out i))
                return true;
            return false;
        }

        public bool hasOut(string fullFilename)
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
                foreach (var d in mIn)
                    d.init();
                foreach (var d in mOut)
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

                foreach (var d in mIn)
                    d.update();
                foreach (var d in mOut)
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

        public bool delete(string dstPath)
        {
            try
            {
                var depFilename = dstPath + "\\" + mMain.Filename + extension;
                File.Delete(depFilename);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool save(string dstPath)
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

                var depFilename = dstPath + mMain.Filename + extension;

                // Directory exists at destination ?
                if (!Directory.Exists(depFilename))
                    Directory.CreateDirectory(depFilename);

                var ts = new TextStream(depFilename);
                ts.Open(TextStream.EMode.Write);
                save(ts.Writer);
                ts.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool load(string dstPath)
        {
            clearIn();
            clearOut();

            try
            {
                var fileInfo = new FileInfo(dstPath + main.Filename + extension);
                if (fileInfo.Exists == false)
                    return false;

                var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var reader = new System.IO.StreamReader(fileStream);

                var ok = false;
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

            foreach (var d in mIn)
            {
                if (!other.mInMap.ContainsKey(d.Filename))
                    return false;
            }

            foreach (var d in mOut)
            {
                if (!other.mOutMap.ContainsKey(d.Filename))
                    return false;
            }

            return true;
        }

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

            var equal = false;
            if (a.main == b.main)
            {
                if (a.mIn.Count == b.mIn.Count)
                {
                    equal = true;
                    foreach (var ad in a.mIn)
                    {
                        foreach (var bd in b.mIn)
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
            var equal = (a == b);
            return !equal;
        }

        public static DepFile sCreate(string filename, string folder)
        {
            var depFile = new DepFile(filename, folder);
            return depFile;
        }

        public static DepFile sCreate(string srcFilename, string srcFolder, string dstFilename, string dstFolder)
        {
            return sCreate(srcFilename, srcFolder, dstFilename, dstFolder, string.Empty);
        }

        public static DepFile sCreate(string srcFilename, string srcFolder, string dstFilename, string dstFolder, string userKey)
        {
            var depFile = new DepFile(srcFilename, srcFolder);
            depFile.userKey = userKey;
            depFile.addOut(dstFilename, dstFolder);
            return depFile;
        }

        public static DepFile sCreate(string MainFilename, string srcFolder, string[] OutFilenames, string dstFolder)
        {
            return sCreate(MainFilename, new string[0], srcFolder, OutFilenames, dstFolder, string.Empty);
        }

        public static DepFile sCreate(string MainFilename, string srcFolder, string[] OutFilenames, string dstFolder, string userKey)
        {
            return sCreate(MainFilename, new string[0], srcFolder, OutFilenames, dstFolder, userKey);
        }

        public static DepFile sCreate(string MainFilename, string[] AdditionalInFilenames, string srcFolder, string[] OutFilenames, string dstFolder)
        {
            return sCreate(MainFilename, AdditionalInFilenames, srcFolder, OutFilenames, dstFolder, string.Empty);
        }

        public static DepFile sCreate(string MainFilename, string[] AdditionalInFilenames, string srcFolder, string[] OutFilenames, string dstFolder, string userKey)
        {
            var depFile = new DepFile(MainFilename, srcFolder);
            try
            {
                depFile.userKey = userKey;

                if (AdditionalInFilenames != null)
                {
                    foreach (var i in AdditionalInFilenames)
                        depFile.addIn(i, srcFolder);
                }
                if (OutFilenames != null)
                {
                    foreach (var i in OutFilenames)
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

        private static bool sReadSingle(System.IO.StreamReader reader, DepFile depfile)
        {
            var readOneDepFile = false;

            var currentDepFile_filename = string.Empty;
            var currentDepFile_folder = string.Empty;

            var currentDepInfoOpen = false;
            var currentDepInfoIsIn = false;
            var currentDepInfo_filename = string.Empty;
            var currentDepInfo_folder = string.Empty;
            var currentDepInfo_method = DepInfo.EDepMethod.TIMESTAMP;
            var currentDepInfo_rule = DepInfo.EDepRule.ON_CHANGE;
            var currentDepInfo_hash = Hash160.Empty;

            while (!reader.EndOfStream && !readOneDepFile)
            {
                var line = reader.ReadLine();
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
                    var lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        var name = lineParts[1].TrimStart(' ', '\t');
                        if (currentDepInfoOpen)
                        {
                            currentDepInfo_filename = new string(name);
                        }
                        else if (depfile != null)
                        {
                            currentDepFile_filename = new string(name);
                        }
                    }
                }
                else if (line.StartsWith("USERKEY="))
                {
                    var lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        var userKey = lineParts[1].TrimStart(' ', '\t');
                        if (depfile != null)
                            depfile.userKey = userKey;
                    }
                }
                else if (line.StartsWith("METHOD="))
                {
                    var lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        var modeStr = lineParts[1].TrimStart(' ', '\t');
                        var method = (DepInfo.EDepMethod)Enum.Parse(typeof(DepInfo.EDepMethod), modeStr, true);
                        if (currentDepInfoOpen)
                        {
                            currentDepInfo_method = method;
                        }
                        else if (depfile != null)
                        {
                            depfile.main.Method = method;
                        }
                    }
                }
                else if (line.StartsWith("FOLDER="))
                {
                    var lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        var name = lineParts[1].TrimStart(' ', '\t');
                        if (currentDepInfoOpen)
                        {
                            currentDepInfo_folder = new string(name);
                        }
                        else if (depfile != null)
                        {
                            currentDepFile_folder = new string(name);
                        }
                    }
                }
                else if (line.StartsWith("HASH="))
                {
                    var lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        var hashStr = lineParts[1].Trim(' ', '\t');
                        if (currentDepInfoOpen)
                            currentDepInfo_hash = Hash160.FromString(hashStr);
                        else if (depfile != null)
                            depfile.main.Hash = Hash160.FromString(hashStr);
                    }
                }
                else if (line.StartsWith("RULE="))
                {
                    var lineParts = line.Split('=');
                    if (lineParts.Length == 2)
                    {
                        var ruleStr = lineParts[1].Trim(' ', '\t');
                        var rule = (DepInfo.EDepRule)Enum.Parse(typeof(DepInfo.EDepRule), ruleStr, true);
                        if (currentDepInfoOpen)
                            currentDepInfo_rule = rule;
                        else if (depfile != null)
                            depfile.main.Rule = rule;
                    }
                }
                else if (line == "}")
                {
                    if (currentDepInfoOpen)
                    {
                        currentDepInfoOpen = false;
                        var depInfo = new DepInfo(0, currentDepInfo_filename, currentDepInfo_folder, currentDepInfo_method, currentDepInfo_hash);
                        depInfo.Rule = currentDepInfo_rule;
                        if (currentDepInfoIsIn)
                            depfile.addIn(depInfo);
                        else
                            depfile.addOut(depInfo);

                        currentDepInfo_filename = string.Empty;
                        currentDepInfo_folder = string.Empty;
                        currentDepInfo_method = DepInfo.EDepMethod.TIMESTAMP;
                        currentDepInfo_rule = DepInfo.EDepRule.ON_CHANGE;
                        currentDepInfo_hash = Hash160.Empty;
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

        internal static bool sReadMulti(string filename, string path, out DepFile[] depFiles)
        {
            return sReadMulti(filename, string.Empty, path, out depFiles);
        }

        internal static List<DepFile> sReadMulti(System.IO.StreamReader reader)
        {
            var readDepFiles = new List<DepFile>();
            while (!reader.EndOfStream)
            {
                var d = new DepFile();
                if (!sReadSingle(reader, d))
                    break;
                readDepFiles.Add(d);
            }
            return readDepFiles;
        }

        internal static bool sReadMulti(string filename, string extension, string path, out DepFile[] depFiles)
        {
            try
            {
                var fileInfo = new FileInfo(path + "\\" + filename + extension);
                if (fileInfo.Exists == false)
                {
                    depFiles = new DepFile[0];
                    return true;
                }

                var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var reader = new System.IO.StreamReader(fileStream);

                var readDepFiles = sReadMulti(reader);
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

        public override bool Equals(object obj)
        {
            var depFile = (DepFile)obj;
            if (depFile.main != main)
                return false;

            if (mIn.Count != depFile.mIn.Count)
                return false;

            foreach (var d in mIn)
            {
                var equal = false;
                foreach (var dd in depFile.mIn)
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
    }

}
