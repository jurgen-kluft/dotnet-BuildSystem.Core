using System;

namespace GameCore
{
    /// <summary>
    /// Represents a directory
    /// <remarks>
    /// Purpose of this class is to make a directory easy to use and safe.
    /// Note: Memory usage has been prevered over performance.
    /// </remarks>
    /// <interface>
    /// 
    /// e.g. 
    ///             C:\Documents\Music\Beatles
    ///             \\cnshaw235\Documents\Music\Beatles
    ///             ~/Data/Assets
    /// 
    /// Properties
    ///     Device                                                          (e.g. C:\)
    ///     DeviceName                                                      (e.g. C)
    ///     Relative                                                        (Documents\Music\Beatles)
    ///     Full                                                            (C:\Documents\Music\Beatles)
    ///     Levels                                                          (3)
    ///     
    /// Boolean
    ///     HasDevice
    ///     IsNetworkDevice
    ///     HasPath
    ///     
    /// Methods
    ///     Clear()
    ///     
    ///     ChangeDevice(device)
    ///     ChangePath(directory)
    ///     ChangeFull(device+directory)
    /// 
    ///     MakeAbsolute()                                                  (working directory)
    ///     MakeAbsolute(device + directory)
    ///     MakeRelative()                                                  (working directory)
    ///     MakeRelative(device + directory)
    /// 
    ///     LevelUp()                                                       (C:\Documents\Music)
    ///     LevelDown(folder)                                               (C:\Documents\Music\Beatles\folder)
    /// 
    /// </interface>
    /// </summary>
    [Serializable]
    public struct Dirname
    {
        #region Internal Statics

        private static readonly bool sIgnoreCase = true;
        private static readonly char sSlash = System.IO.Path.DirectorySeparatorChar;
        private static readonly char sOtherSlash = System.IO.Path.DirectorySeparatorChar == '\\' ? '/' : '\\';
        private static readonly string sSlashStr = "" + System.IO.Path.DirectorySeparatorChar;
        private static readonly string sDoubleSlash = "" + System.IO.Path.DirectorySeparatorChar + System.IO.Path.DirectorySeparatorChar;
        private static readonly char sSemi = ':';
        //private static readonly string sDotStr = ".";
        private static readonly string sSemiSlash = ":" + sSlashStr;
        private static readonly string sIllegalNameChars = "/\\:*?<>|";

        static internal string RemoveChars(string ioString, string inChars)
        {
            var cc = 0;
            var nn = 0;
            var str = ioString.ToCharArray();
            for (var i = 0; i < ioString.Length; ++i)
            {
                var c = str[nn];
                if (inChars.IndexOf(c) >= 0)
                {
                    nn++;
                }
                else
                {
                    if (cc < nn) str[cc] = c;
                    ++cc;
                    ++nn;
                }
            }

            // If nothing removed just return the incoming string
            if (cc == nn)
                return ioString;

            return new string(str, 0, cc);
        }

        static internal bool ContainsChars(string inString, string inChars)
        {
            foreach (var c in inString)
            {
                if (inChars.IndexOf(c) != -1)
                    return true;
            }
            return false;
        }

        #endregion
        #region Public Statics

        public static readonly Dirname Empty = new Dirname(string.Empty, string.Empty.GetHashCode());

        #endregion Public Statics
        #region Fields

        private int mHashCode;
        private string mFull;

        #endregion
        #region Constructors

        public Dirname(string inRHS)
        {
            mFull = inRHS.Replace(sOtherSlash, sSlash);
            mHashCode = mFull.GetHashCode();
            ChangeFull(mFull);
        }

        private Dirname(string inRHS, int hashcode)
        {
            mFull = inRHS;
            mHashCode = hashcode;
            ChangeFull(mFull);
        }

        public Dirname(Dirname inRHS)
        {
            mFull = inRHS.mFull;
            mHashCode = inRHS.mHashCode;
        }

        #endregion
        #region Properties

        public bool IsEmpty
        {
            get
            {
                return mFull.Length == 0;
            }
        }

        public string Device
        {
            get
            {
                string deviceName;
                bool isNetworkDevice;
                sParseDevice(mFull, out deviceName, out isNetworkDevice);

                if (System.OperatingSystem.IsMacOS())
                {

                }
                else
                {
                    if (deviceName.Length != 0)
                        return (isNetworkDevice) ? sDoubleSlash + deviceName : deviceName + sSemi;
                }

                return string.Empty;
            }
            set
            {
                ChangeDevice(value);
            }
        }

        public string DeviceName
        {
            get
            {
                if (!System.OperatingSystem.IsMacOS())
                {
                    string deviceName;
                    bool isNetworkDevice;
                    sParseDevice(mFull, out deviceName, out isNetworkDevice);
                    return deviceName;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (!System.OperatingSystem.IsMacOS())
                {
                    var device = RemoveChars(value, sIllegalNameChars);
                    ChangeDevice(device);
                }
            }
        }

        public bool IsNetworkDevice
        {
            get
            {
                string deviceName;
                bool isNetworkDevice;
                sParseDevice(mFull, out deviceName, out isNetworkDevice);
                return isNetworkDevice;
            }
            set
            {
                string deviceName;
                bool isNetworkDevice;
                string path;
                int levels;
                sParseFull(mFull, out deviceName, out isNetworkDevice, out path, out levels);

                if (isNetworkDevice != value)
                {
                    sConstructFull(deviceName, value, path, out mFull, out mHashCode);
                }

            }
        }

        public string Path
        {
            get
            {
                string deviceName;
                bool isNetworkDevice;
                string path;
                int levels;
                sParseFull(mFull, out deviceName, out isNetworkDevice, out path, out levels);
                return path;
            }
            set
            {
                ChangePath(value);
            }
        }


        public int Levels
        {
            get
            {
                string name;
                int levels;
                sParseNameAndLevels(mFull, out name, out levels);
                return levels;
            }
        }

        public string Name
        {
            get
            {
                string name;
                int levels;
                sParseNameAndLevels(mFull, out name, out levels);
                return name;
            }
        }

        public string Full
        {
            get
            {
                return mFull;
            }
            set
            {
                ChangeFull(value);
            }
        }

        public bool HasDevice
        {
            get
            {
                string deviceName;
                bool isNetworkDevice;
                sParseDevice(mFull, out deviceName, out isNetworkDevice);
                return deviceName.Length != 0;
            }
        }

        #endregion
        #region Private Static Methods

        private static void sConstructFull(string deviceName, bool isNetworkDevice, string path, out string outFull, out int outHashCode)
        {
            var device = string.Empty;
            if (!System.OperatingSystem.IsMacOS())
            {
                device = isNetworkDevice ? (sDoubleSlash + deviceName) : (deviceName + sSemi);
                outFull = (deviceName.Length != 0) ? (device + sSlashStr + path) : path;
            } else {
                outFull = (deviceName.Length != 0) ? (device + sSlashStr + path) : "/" + path;
            }
            outHashCode = outFull.ToLower().GetHashCode();
        }

        private static void sParseDevice(string inFull, out string outDevice, out bool outIsNetworkDevice)
        {
            if (System.OperatingSystem.IsMacOS())
            {
                outDevice = string.Empty;
                outIsNetworkDevice = false;
                return;
            }

            // Device
            string device;
            var networkDevice = inFull.StartsWith(sDoubleSlash);
            if (networkDevice)
            {
                var slashIndex = inFull.IndexOf(sSlash, 2);
                device = slashIndex >= 0 ? inFull.Substring(2, slashIndex - 2) : inFull.Substring(2);
            }
            else
            {
                var semiSlashIndex = inFull.IndexOf(sSemiSlash);
                device = semiSlashIndex >= 0 ? inFull.Substring(0, semiSlashIndex) : string.Empty;
            }

            outDevice = RemoveChars(device, sIllegalNameChars);
            if (networkDevice)
            {
                outIsNetworkDevice = true;
            }
            else if (outDevice.Length > 0)
            {
                outIsNetworkDevice = false;
            }
            else
            {
                outIsNetworkDevice = false;
                outDevice = string.Empty;
            }
        }

        private static void sParsePath(string inFullWithoutDevice, out string outPath, out int outLevels)
        {
            // Documents\Music.Collection\Beatles.Album
            // Count levels
            var folders = inFullWithoutDevice.Split(new char[] { sSlash }, StringSplitOptions.RemoveEmptyEntries);

            outPath = string.Empty;
            outLevels = 0;
            foreach (var folder in folders)
            {
                if (!ContainsChars(folder, sIllegalNameChars))
                {
                    if (outLevels == 0)
                        outPath = folder;
                    else
                        outPath = outPath + sSlash + folder;
                    ++outLevels;
                }
            }
        }

        private static void sParseNameAndLevels(string inFull, out string outName, out int outLevels)
        {
            bool outIsNetworkDevice;
            sParseDevice(inFull, out outName, out outIsNetworkDevice);
            sRemoveDevice(inFull, outIsNetworkDevice, out outName);

            // Documents\Music.Collection\Beatles.Album
            // Count levels
            // Name of folder
            var folders = outName.Split(new char[] { sSlash }, StringSplitOptions.RemoveEmptyEntries);

            if (folders.Length > 0)
                outName = folders[folders.Length - 1];
            else
                outName = string.Empty;

            outLevels = 0;
            foreach (var folder in folders)
            {
                if (!ContainsChars(folder, sIllegalNameChars))
                {
                    ++outLevels;
                }
            }
        }

        private static void sRemoveDevice(string inPathWithDevice, bool inIsNetworkDevice, out string outPath)
        {
            // Remove device
            outPath = inPathWithDevice;
            if (inIsNetworkDevice)
            {
                var slashIndex = inPathWithDevice.IndexOf(sSlash, 2);
                if (slashIndex >= 0)
                {
                    outPath = inPathWithDevice.Substring(slashIndex + 1);
                }
            }
            else
            {
                if (!System.OperatingSystem.IsMacOS())
                {
                    var semiSlashIndex = inPathWithDevice.IndexOf(sSemiSlash);
                    if (semiSlashIndex >= 0)
                    {
                        outPath = inPathWithDevice.Substring(semiSlashIndex + 2);
                    }
                }
                else
                {
                    outPath = inPathWithDevice;
                }
            }
        }

        private static void sParseFull(string inFull, out string outDeviceName, out bool outIsNetworkDevice, out string outPath, out int outLevels)
        {
            // inFull can be any of these:
            //   - HeyJude.mp3
            //   - Documents\Music\Beatles\HeyJude.mp3
            //   - C:\HeyJude.mp3
            //   - \\cnshaw235\HeyJude.mp3
            //   - C:\Documents\Music\Beatles\HeyJude.mp3
            //   - \\cnshaw235\Documents\Music\Beatles\HeyJude.mp3
            sParseDevice(inFull, out outDeviceName, out outIsNetworkDevice);
            sRemoveDevice(inFull, outIsNetworkDevice, out outPath);
            sParsePath(outPath, out outPath, out outLevels);
        }

        #endregion
        #region Public Methods

        public void Clear()
        {
            mFull = string.Empty;
            mHashCode = mFull.GetHashCode();
        }

        public void ChangeDevice(string inDevice)
        {
            bool isNetworkDevice;
            string deviceName;
            sParseDevice(inDevice, out deviceName, out isNetworkDevice);

            string _deviceName;
            bool _isNetworkDevice;
            string path;
            int levels;
            sParseFull(mFull, out _deviceName, out _isNetworkDevice, out path, out levels);
            sConstructFull(deviceName, isNetworkDevice, path, out mFull, out mHashCode);
        }

        public void ChangePath(string inPath)
        {
            string path;
            int levels;
            sParsePath(inPath, out path, out levels);

            string deviceName;
            bool isNetworkDevice;
            string _path;
            sParseFull(mFull, out deviceName, out isNetworkDevice, out _path, out levels);
            sConstructFull(deviceName, isNetworkDevice, path, out mFull, out mHashCode);
        }

        public void ChangeFull(string deviceAndDirectory)
        {
            // C:\Documents\Music.Collection\Beatles.Album
            // Count levels
            string deviceName;
            bool isNetworkDevice;
            sParseDevice(deviceAndDirectory, out deviceName, out isNetworkDevice);

            string _path;
            sRemoveDevice(deviceAndDirectory, isNetworkDevice, out _path);

            string path;
            int levels;
            sParsePath(_path, out path, out levels);

            string _deviceName;
            bool _isNetworkDevice;
            sParseFull(mFull, out _deviceName, out _isNetworkDevice, out _path, out levels);
            sConstructFull(deviceName, isNetworkDevice, path, out mFull, out mHashCode);
        }

        public static Dirname Add(Dirname deviceAndPath, Dirname path)
        {
            string deviceNameL;
            bool isNetworkDeviceL;
            string pathL;
            int levelsL;
            sParseFull(deviceAndPath.Full, out deviceNameL, out isNetworkDeviceL, out pathL, out levelsL);

            string deviceNameR;
            bool isNetworkDeviceR;
            string pathR;
            int levelsR;
            sParseFull(path.Full, out deviceNameR, out isNetworkDeviceR, out pathR, out levelsR);

            var s = new Dirname();
            sConstructFull(deviceNameL, isNetworkDeviceL, (pathL.Length != 0 && pathR.Length != 0) ? (pathL + sSlash + pathR) : (pathL + pathR), out s.mFull, out s.mHashCode);
            return s;
        }

        public Dirname MakeAbsolute()
        {
            return MakeAbsolute(System.Environment.CurrentDirectory);
        }

        public Dirname MakeAbsolute(string absolutePath)
        {
            string deviceName;
            bool isNetworkDevice;
            string path;
            int levels;
            sParseFull(mFull, out deviceName, out isNetworkDevice, out path, out levels);

            if (deviceName.Length != 0)
                return new Dirname(this);

            if (path.Length != 0)
            {
                if (absolutePath.EndsWith(sSlashStr))
                    absolutePath += path;
                else
                    absolutePath += sSlash + path;
            }

            var newDirname = new Dirname(this);
            newDirname.ChangeFull(absolutePath);
            return newDirname;
        }

        public Dirname MakeRelative()
        {
            return MakeRelative(System.Environment.CurrentDirectory);
        }

        public Dirname MakeRelative(string absolutePath)
        {
            // IN:      C:\My Media\Documents
            // THIS:    C:\My Media\Documents\Music\Beatles

            // RESULT:  Music\Beatles

            string thisDeviceName;
            bool thisIsNetworkDevice;
            string thisPath;
            int thisLevels;
            sParseFull(mFull, out thisDeviceName, out thisIsNetworkDevice, out thisPath, out thisLevels);

            if (thisPath.Length == 0)
                return new Dirname(this);

            var newDirname = new Dirname(this);
            newDirname.ChangeFull(absolutePath);

            string newDeviceName;
            bool newIsNetworkDevice;
            string newPath;
            int newLevels;
            sParseFull(newDirname.mFull, out newDeviceName, out newIsNetworkDevice, out newPath, out newLevels);

            var sameDevice = true;
            if (string.Compare(thisDeviceName, newDeviceName, true) != 0)
                sameDevice = false;

            if (newPath.Length == 0)
            {
                newPath = thisPath;

                if (sameDevice)
                {
                    newIsNetworkDevice = false;
                    newDeviceName = string.Empty;
                }
                else
                {
                    // Incoming absolute path, no device and no path.
                    // Restore to our initial state.
                    newDeviceName = thisDeviceName;
                }
            }
            else
            {
                if (sameDevice)
                {
                    var folders = thisPath.Split(new char[] { sSlash }, StringSplitOptions.RemoveEmptyEntries);
                    var inFolders = newPath.Split(new char[] { sSlash }, StringSplitOptions.RemoveEmptyEntries);

                    var samePath = true;
                    for (var i = 0; i < inFolders.Length && samePath; i++)
                        samePath = string.Compare(inFolders[i], folders[i], sIgnoreCase) == 0;

                    newIsNetworkDevice = false;
                    newDeviceName = string.Empty;
                    if (samePath)
                    {
                        var path = string.Empty;
                        for (var i = newLevels; i < thisLevels; i++)
                        {
                            path = path + (path.Length == 0 ? folders[i] : (sSlash + folders[i]));
                        }

                        newPath = path;
                    }
                    else
                    {
                        newPath = thisPath;
                    }
                }
                else
                {
                    newIsNetworkDevice = thisIsNetworkDevice;
                    newDeviceName = thisDeviceName;
                    newPath = thisPath;
                }
            }

            sConstructFull(newDeviceName, newIsNetworkDevice, newPath, out newDirname.mFull, out newDirname.mHashCode);
            return newDirname;
        }

        public void LevelUp()
        {
            string thisDeviceName;
            bool thisIsNetworkDevice;
            string thisPath;
            int thisLevels;
            sParseFull(mFull, out thisDeviceName, out thisIsNetworkDevice, out thisPath, out thisLevels);

            if (thisPath.Length == 0)
                return;

            var folders = thisPath.Split(new char[] { sSlash }, StringSplitOptions.RemoveEmptyEntries);

            thisPath = string.Empty;
            for (var i = 0; i < (folders.Length - 1); i++)
            {
                thisPath = thisPath + (thisPath.Length == 0 ? folders[i] : (sSlash + folders[i]));
            }
            sConstructFull(thisDeviceName, thisIsNetworkDevice, thisPath, out mFull, out mHashCode);
        }

        public Dirname LeveledUp()
        {
            string thisDeviceName;
            bool thisIsNetworkDevice;
            string thisPath;
            int thisLevels;
            sParseFull(mFull, out thisDeviceName, out thisIsNetworkDevice, out thisPath, out thisLevels);

            if (thisPath.Length == 0)
                return new Dirname(this);

            var folders = thisPath.Split(new char[] { sSlash }, StringSplitOptions.RemoveEmptyEntries);

            thisPath = string.Empty;
            for (var i = 0; i < (folders.Length - 1); i++)
            {
                thisPath = thisPath + (thisPath.Length == 0 ? folders[i] : (sSlash + folders[i]));
            }

            var f = new Dirname();
            sConstructFull(thisDeviceName, thisIsNetworkDevice, thisPath, out f.mFull, out f.mHashCode);
            return f;
        }

        public void LevelDown(string folder)
        {
            if (folder.Length == 0)
                return;

            string thisDeviceName;
            bool thisIsNetworkDevice;
            string thisPath;
            int thisLevels;
            sParseFull(mFull, out thisDeviceName, out thisIsNetworkDevice, out thisPath, out thisLevels);

            thisPath = thisPath + (thisPath.Length == 0 ? folder : (sSlash + folder));

            sConstructFull(thisDeviceName, thisIsNetworkDevice, thisPath, out mFull, out mHashCode);
        }

        public Dirname LeveledDown(string folder)
        {
            if (folder.Length == 0)
                return new Dirname(this);

            string thisDeviceName;
            bool thisIsNetworkDevice;
            string thisPath;
            int thisLevels;
            sParseFull(mFull, out thisDeviceName, out thisIsNetworkDevice, out thisPath, out thisLevels);

            thisPath = thisPath + (thisPath.Length == 0 ? folder : (sSlash + folder));

            var f = new Dirname();
            sConstructFull(thisDeviceName, thisIsNetworkDevice, thisPath, out f.mFull, out f.mHashCode);
            return f;
        }
        #endregion
        #region Object Methods

        public override int GetHashCode()
        {
            return mHashCode;
        }

        public override bool Equals(object o)
        {
            if (o is Dirname)
            {
                var other = (Dirname)o;
                return string.Compare(Full, other.Full, sIgnoreCase) == 0;
            }
            return false;
        }

        /// <summary>
        /// Will return the full filename
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Full;
        }

        #endregion
        #region Operators

        public static bool operator ==(Dirname inLHS, Dirname inRHS)
        {
            return string.Compare(inLHS.Full, inRHS.Full, sIgnoreCase) == 0;
        }

        public static bool operator !=(Dirname inLHS, Dirname inRHS)
        {
            return string.Compare(inLHS.Full, inRHS.Full, sIgnoreCase) != 0;
        }

        public static Dirname operator +(Dirname inLHS, Dirname inRHS)
        {
            return Dirname.Add(inLHS, inRHS);
        }

        public static Filename operator +(Dirname inLHS, Filename inRHS)
        {
            return inRHS.MakeAbsolute(inLHS);
        }

        public static implicit operator string(Dirname f)
        {
            return f.ToString();
        }

        #endregion
        #region UnitTest

        public static bool UnitTest()
        {
            var test1 = new Dirname(@"C:\Temp\Test\Movies");
            var test2 = new Dirname(@"\\cnshaw235\Temp\Test\Movies");
            var test3 = new Dirname(@"Movies");
            var test4 = new Dirname(@"Test\Movies");

            test2 = test2.MakeAbsolute();
            test3 = test3.MakeAbsolute(@"C:\Temp\Test");
            test4 = test4.MakeAbsolute(@"C:\Temp");

            test2.Device = @"C:\";

            return true;
        }

        #endregion
    }
}
