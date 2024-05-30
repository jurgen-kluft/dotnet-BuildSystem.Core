using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Excel = Net.Office.Excel;
using GameCore;

namespace DataBuildSystem
{
    namespace Localization
    {
        public static class Validation
        {
            public static bool ValidateString(string inColumnName, ref string text)
            {
                var temp = text.ToCharArray();
                var valid = true;
                for (var i = 0; i < text.Length; i++)
                {
                    var c = text[i];
                    foreach (var conversion in Settings.mCharConversions)
                    {
                        if (c == conversion.from)
                        {
                            c = conversion.to;
                            break;
                        }
                    }

                    // check if the character is supported (in any language)
                    if (c >= 0 && c < 256 && Settings.mCharSet[c] != 0)
                    {
                        // Character is supported
                        temp[i] = c;
                    }
                    else
                    {
                        // Character is not supported, replace with a question mark
                        temp[i] = '?';
                        // and log an error message
                        Console.WriteLine("Character \"{0}\"({1}) in text \"{2}\" for language \"{3}\" is not supported!", c, c.ToString(), text, inColumnName);
                        valid = false;
                    }
                }

                text = new string(temp);

                return valid;
            }
        }

        public static class Settings
        {
            private static readonly int mBeginRow = 15;
            private static readonly int mEndRow = 2048;

            public static readonly string mLogLevel = "Info";

            public static readonly Column[] mColumns =
            {
                new Column("ID", 0, mBeginRow, mEndRow),
                new Column("English", 3, mBeginRow, mEndRow),
                new Column("French", 4, mBeginRow, mEndRow),
                new Column("Italian", 5, mBeginRow, mEndRow),
                new Column("German", 6, mBeginRow, mEndRow),
                new Column("Spanish", 7, mBeginRow, mEndRow),
                new Column("English_US", 8, mBeginRow, mEndRow),
            };

            public static readonly byte[] mCharSet =
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, // 000-015
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 016-031 =
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 032-047 =  !"#$%&'()*+,-./
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 048-063 = 0123456789:;<=>?
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 064-079 = @ABCDEFGHIJKLMNO
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 080-095 = PQRSTUVWXYZ[\]^_
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 096-111 = `abcdefghijklmno
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 112-127 = pqrstuvwxyz{|}~?
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, // 128-143 = € ‚ƒ„…†‡ˆ‰Š‹??
                0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, // 144-159 =  ‘’“”•–—˜™š›œ žŸ
                0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0, // 160-175 =  ¡¢£¤¥¦§¨©ª«¬-®¯
                1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, // 176-191 = °±²³´µ¶·¸¹º»¼½¾¿
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 192-207 = ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏ
                1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, // 208-223 = ÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 224-239 = àáâãäåæçèéêëìíîï
                1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1
            }; // 240-255 = ðñòóôõö÷øùúûüý?


            public static readonly CharConversion[] mCharConversions =
            {
                new CharConversion((char)0x152, (char)0x8c),
                new CharConversion((char)0x153, (char)0x9c),
                new CharConversion((char)0xBA, (char)0xB0), // replace little round by another similar round
                new CharConversion((char)0x2019, (char)0x27)
            };
        }

        public class CharConversion
        {
            private readonly char mFrom;
            private readonly char mTo;

            public CharConversion(char from, char to)
            {
                mFrom = from;
                mTo = to;
            }

            public char from
            {
                get { return mFrom; }
            }

            public char to
            {
                get { return mTo; }
            }
        }

        public class StringTable
        {
            private List<string> mStrings = new();
            private List<ulong> mStringHashes = new();
            private uint mOffset = 0;
            private List<uint> mStringOffsets = new();

            public void Add(string inString)
            {
                // TODO Handle UTF-8, this means here you need to correctly compute the string length in bytes
                mStrings.Add(inString);
                mStringOffsets.Add(mOffset);
                mStringHashes.Add(Hashing64.ComputeMurmur64(inString));
                var utf8 = new UTF8Encoding();
                utf8.GetBytes(inString);
                mOffset += (uint)(inString.Length + 1);
            }

            public void Clear()
            {
                mStrings = new List<string>();
                mStringOffsets = new List<uint>();
                mStringHashes = new List<ulong>();
                mOffset = 0;
            }

            public int IndexOf(string inString)
            {
                return mStrings.IndexOf(inString);
            }

            public uint OffsetOf(string inString)
            {
                return mStringOffsets[IndexOf(inString)];
            }

            public List<int> Consolidate()
            {
                List<int> map = new();
                var strings = mStrings;
                Clear();

                var c = 0;
                foreach (var s in strings)
                {
                    if (s != string.Empty)
                    {
                        Add(s);
                        map.Add(c);
                        ++c;
                    }
                }

                return map;
            }

            public bool Consolidate(List<int> map)
            {
                var strings = mStrings;
                Clear();

                foreach (var i in map)
                {
                    Add(strings[i]);
                }

                return true;
            }

            public bool SuperImpose(StringTable master)
            {
                var c = 0;
                for (var i = 0; i < mStrings.Count; i++)
                {
                    if (mStrings[i] == string.Empty)
                    {
                        mStrings[i] = master[i];
                        c++;
                    }
                }

                if (c > 0)
                {
                    var strings = mStrings;
                    Clear();

                    foreach (var s in strings)
                        Add(s);
                }

                return true;
            }

            class HashAndIndexPairComparer : IComparer<KeyValuePair<ulong, int>>
            {
                public int Compare(KeyValuePair<ulong, int> x, KeyValuePair<ulong, int> y)
                {
                    if (x.Key < y.Key)
                        return -1;
                    else if (x.Key > y.Key)
                        return 1;
                    else
                        return 0;
                }
            }

            public void GetRemap(out List<int> outRemap)
            {
                var i = 0;
                List<KeyValuePair<ulong, int>> hashes = new(Count);
                foreach (var hash in mStringHashes)
                    hashes.Add(new KeyValuePair<ulong, int>(hash, i++));

                // Sort by hash
                hashes.Sort(new HashAndIndexPairComparer());

                // Fill remap
                outRemap = new List<int>(Count);
                foreach (var hash_idx in hashes)
                {
                    var index = hash_idx.Value;
                    outRemap.Add(index);
                }
            }

            public void Remap(List<int> remap)
            {
                var strings = mStrings;
                Clear();

                foreach (var i in remap)
                    Add(strings[i]);
            }

            public bool Read(IBinaryReader reader)
            {
                try
                {
                    var count = reader.ReadInt32();

                    // Hashes
                    for (var i = 0; i < count; ++i)
                        reader.ReadUInt64();
                    // Offsets
                    for (var i = 0; i < count; ++i)
                        reader.ReadInt32();

                    // Strings
                    for (var i = 0; i < count; ++i)
                    {
                        var str = reader.ReadString();
                        Add(str);
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            public bool Write(IBinaryWriter writer)
            {
                try
                {
                    writer.Write(Count);

                    foreach (var hash in mStringHashes)
                        writer.Write(hash);
                    foreach (var offset in mStringOffsets)
                        writer.Write(offset);
                    foreach (var s in mStrings)
                        writer.Write(s);
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            public int Count
            {
                get { return mStrings.Count; }
            }

            public List<string> All
            {
                get { return mStrings; }
            }

            public string this[int index]
            {
                get { return mStrings[index]; }
            }
        }

        public class Column
        {
            private readonly string mName;
            private readonly int mColumn;
            private readonly int[] mRowRange;
            private StringTable mStringTable = null;

            public Column(string name, int column, int[] rowRange)
            {
                mName = name;
                mColumn = column;
                mRowRange = rowRange;
            }

            public Column(string name, int column, int rowBegin, int rowEnd)
            {
                mName = name;
                mColumn = column;
                mRowRange = new int[2];
                mRowRange[0] = rowBegin;
                mRowRange[1] = rowEnd;
            }

            public int Count
            {
                get { return mStringTable.Count; }
            }

            public string Name
            {
                get { return mName; }
            }

            public string this[int index]
            {
                get { return mStringTable[index]; }
            }

            public StringTable Table
            {
                get { return mStringTable; }
            }

            public List<int> Consolidate()
            {
                return mStringTable.Consolidate();
            }

            public bool Consolidate(List<int> map)
            {
                return mStringTable.Consolidate(map);
            }

            public bool SuperImpose(Column c)
            {
                // Cells that are empty are copied from the incoming column
                return mStringTable.SuperImpose(c.mStringTable);
            }

            public bool ValidateUsedCharacters()
            {
                var valid = true;
                for (var i = 0; i < mStringTable.All.Count; i++)
                {
                    var str = mStringTable.All[i];
                    valid = valid && Validation.ValidateString(mName, ref str);
                    mStringTable.All[i] = str;
                }

                return valid;
            }

            public int IndexOf(string cellContent)
            {
                return mStringTable.IndexOf(cellContent);
            }

            public bool ReadColumn(Excel.Worksheet worksheet, int columnNumber, int rowStart, int rowEnd, out List<string> columnText)
            {
                columnText = new List<string>();
                for (var r = rowStart; r < rowEnd; r++)
                {
                    var row = worksheet.Rows[(ushort)r];
                    if (row != null)
                    {
                        var cell = row.Cells[(byte)columnNumber];
                        if (cell != null)
                        {
                            var textcell = cell.FormattedValue();
                            columnText.Add(textcell.Replace("\\n", "\n"));
                        }
                        else
                            columnText.Add(string.Empty);
                    }
                }

                return true;
            }

            public bool Read(List<Excel.Worksheet> worksheets)
            {
                var columnTextList = new List<string>();
                var columnText = new List<string>();

                foreach (var worksheet in worksheets)
                {
                    if (ReadColumn(worksheet, mColumn, mRowRange[0], mRowRange[1], out columnText))
                    {
                        foreach (var s in columnText)
                            columnTextList.Add(s);
                    }
                    else
                    {
                        return false;
                    }
                }

                Console.WriteLine("Column string number:{0}", columnTextList.Count);


                mStringTable = ConvertToStringTable(columnTextList);
                return true;
            }

            private static StringTable ConvertToStringTable(List<string> columnText)
            {
                var strTable = new StringTable();
                foreach (var s in columnText)
                    strTable.Add(s);
                return strTable;
            }

            // Return the written data size
            public int Save(string filename, long magic)
            {
                try
                {
                    FileInfo fileInfo = new(filename);
                    if (fileInfo.Exists)
                    {
                        fileInfo.Delete();
                        fileInfo = new(filename);
                    }

                    FileStream fileStream = new(fileInfo.FullName, FileMode.OpenOrCreate, FileAccess.Write);
                    var writer = ArchitectureUtils.CreateBinaryWriter(fileStream, LocalizerConfig.Platform);

                    writer.Write((int)(magic >> 32));
                    writer.Write((int)(magic));

                    var result = mStringTable.Write(writer);

                    if (!result)
                    {
                        Console.WriteLine("Error occurred when writing language file \"" + Name + "\".");
                    }

                    var size = (int)writer.Position;

                    fileStream.Close();
                    writer.Close();

                    return size;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception (" + e.ToString() + ") while writing language file \"" + Name + "\".");
                    return 0;
                }
            }
        }

        public class IdFile
        {
            private string mName = string.Empty;
            private string mFilename = string.Empty;
            private string mFolder = string.Empty;
            private long mFileSize = 0;
            private StringTable mStrTable = new StringTable();

            public IdFile(string filename, string folder)
            {
                mFilename = filename;
                mFolder = folder;
            }

            public IdFile(string name, string filename, string folder)
            {
                mName = name;
                mFilename = filename;
                mFolder = folder;
            }

            public string filename
            {
                get { return mFilename; }
            }

            public string folder
            {
                get { return mFolder; }
            }

            public long filesize
            {
                get { return mFileSize; }
            }

            public string name
            {
                get { return mName; }
            }

            public void clear()
            {
                mStrTable = new StringTable();
            }


            public void add(StringTable strTable)
            {
                for (var i = 0; i < strTable.Count; ++i)
                    mStrTable.Add(strTable[i]);
            }

            public void add(IdFile idFile)
            {
                for (var i = 0; i < idFile.mStrTable.Count; ++i)
                    mStrTable.Add(idFile.mStrTable[i]);
            }

            public void getRemap(out List<int> remap)
            {
                mStrTable.GetRemap(out remap);
            }

            public void remap(List<int> remap)
            {
                mStrTable.Remap(remap);
            }

            public bool load()
            {
                try
                {
                    var fileinfoXlsIds = new FileInfo(mFolder + mFilename);
                    if (fileinfoXlsIds.Exists)
                    {
                        mFileSize = fileinfoXlsIds.Length;

                        var filestreamXlsIds = new FileStream(fileinfoXlsIds.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var filestreamXlsIdsReader = ArchitectureUtils.CreateBinaryReader(filestreamXlsIds, LocalizerConfig.Platform);

                        var magic = filestreamXlsIdsReader.ReadInt64();
                        mStrTable.Read(filestreamXlsIdsReader);

                        filestreamXlsIdsReader.Close();
                        filestreamXlsIds.Close();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool save(long magic)
            {
                try
                {
                    var writer = ArchitectureUtils.CreateBinaryWriter(mFolder + mFilename, Platform.Current);

                    writer.Write(magic);
                    mStrTable.Write(writer);

                    writer.Close();

                    FileInfo fileinfoXlsIds = new(mFolder + mFilename);
                    if (fileinfoXlsIds.Exists)
                    {
                        mFileSize = fileinfoXlsIds.Length;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool saveHeaderFile(int maxFileSize, long magic)
            {
                try
                {
                    var textStream = TextStream.OpenForWrite(mFolder + Path.ChangeExtension(mFilename, (".h")));

                    string line;

                    textStream.WriteLine("#ifndef __LOCALIZATION_IDS_H__");
                    textStream.WriteLine("#define __LOCALIZATION_IDS_H__");
                    textStream.WriteLine("");
                    textStream.WriteLine("");
                    line = string.Format("#define\t\t{0}\t\t\t0x{1:X8}", "LOCALIZATION_VERSION_H", (int)(magic >> 32));
                    textStream.WriteLine(line);
                    line = string.Format("#define\t\t{0}\t\t\t0x{1:X8}", "LOCALIZATION_VERSION_L", (int)(magic));
                    textStream.WriteLine(line);
                    textStream.WriteLine("");
                    textStream.WriteLine("");
                    line = string.Format("#define\t\t{0}\t\t\t0x{1:X8}", "LOCALIZATION_MAX_FILE_SIZE", maxFileSize);
                    textStream.WriteLine(line);
                    textStream.WriteLine("");

                    for (var i = 0; i < mStrTable.Count; ++i)
                    {
                        line = string.Format("#define\t\t{0}\t\t\t{1}", mStrTable[i], i);
                        textStream.WriteLine(line);
                    }

                    textStream.WriteLine("");
                    textStream.WriteLine("");
                    textStream.WriteLine("#endif ///< __LOCALIZATION_IDS_H__");

                    textStream.Close();

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception (" + e.ToString() + ") while writing localization id header file \"" + filename + "\".");
                    return false;
                }
            }
        }

        public class LocFile
        {
            private string mName = string.Empty;
            private string mFilename = string.Empty;
            private string mFolder = string.Empty;
            private long mFileSize = 0;
            private StringTable mStrTable = new StringTable();

            public LocFile(string filename, string folder)
            {
                mFilename = filename;
                mFolder = folder;
            }

            public LocFile(string name, string filename, string folder)
            {
                mName = name;
                mFilename = filename;
                mFolder = folder;
            }

            public string filename
            {
                get { return mFilename; }
            }

            public string folder
            {
                get { return mFolder; }
            }

            public long filesize
            {
                get { return mFileSize; }
            }

            public string name
            {
                get { return mName; }
            }

            public void clear()
            {
                mStrTable = new StringTable();
            }


            public void add(StringTable strTable)
            {
                for (var i = 0; i < strTable.Count; ++i)
                    mStrTable.Add(strTable[i]);
            }

            public void add(LocFile locFile)
            {
                for (var i = 0; i < locFile.mStrTable.Count; ++i)
                    mStrTable.Add(locFile.mStrTable[i]);
            }

            public bool load()
            {
                try
                {
                    FileInfo fileinfoXlsIds = new(Path.Join(mFolder, mFilename));
                    if (fileinfoXlsIds.Exists)
                    {
                        mFileSize = fileinfoXlsIds.Length;

                        var filestreamXlsIds = new FileStream(fileinfoXlsIds.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var filestreamXlsIdsReader = ArchitectureUtils.CreateBinaryReader(filestreamXlsIds, Platform.Current);

                        var magic = filestreamXlsIdsReader.ReadInt64();
                        mStrTable.Read(filestreamXlsIdsReader);

                        filestreamXlsIdsReader.Close();
                        filestreamXlsIds.Close();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public void remap(List<int> map)
            {
                mStrTable.Remap(map);
            }

            public bool save(long magic)
            {
                try
                {
                    var writer = ArchitectureUtils.CreateBinaryWriter(Path.Join(mFolder, mFilename), Platform.Current);

                    writer.Write(magic);
                    mStrTable.Write(writer);

                    writer.Close();

                    FileInfo fileinfoXlsIds = new(Path.Join(mFolder, mFilename));
                    if (fileinfoXlsIds.Exists)
                    {
                        mFileSize = fileinfoXlsIds.Length;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The main localization database.
        ///
        /// Manages (new, open, load, save) all Loc and Id files and finally combines them all into one LocFile per language and one LocId file.
        ///
        /// </summary>
        public class LocDatabase
        {
            private string mMainFilename;
            private bool mIsModified = true;
            private readonly long mMagic = DateTime.Now.Ticks;
            private readonly List<LocFile> mLocFiles = new();
            private readonly List<LocFile> mMasterLocFiles = new();
            private readonly List<IdFile> mIDFiles = new();
            private readonly List<IdFile> mMasterIDFiles = new();

            public long magic
            {
                get { return mMagic; }
            }

            public bool isModified
            {
                get { return mIsModified; }
            }

            public void init(string main)
            {
                mMainFilename = main;

                mIsModified = true;
                {
                    DepFile depFile = new(mMainFilename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.MainDepFileExtension;

                    if (depFile.load(LocalizerConfig.DepPath))
                    {
                        mIsModified = depFile.isModified();
                    }
                }
            }

            private LocFile newLoc(string dstSubPath, string name)
            {
                LocFile newLocFile = new(name, dstSubPath + (name + LocalizerConfig.MainLocFileExtension), LocalizerConfig.DstPath);
                return newLocFile;
            }

            private LocFile openLoc(string dstSubPath, string name, bool master)
            {
                LocFile newLocFile = null;
                if (master)
                {
                    foreach (var l in mMasterLocFiles)
                        if (l.name == name)
                            return l;
                    newLocFile = newLoc(dstSubPath, name);
                    mMasterLocFiles.Add(newLocFile);
                }
                else
                {
                    foreach (var l in mLocFiles)
                        if (l.name == name)
                            return l;
                    newLocFile = newLoc(dstSubPath, name);
                    mLocFiles.Add(newLocFile);
                }

                return newLocFile;
            }

            private IdFile newId(string dstSubPath, string name)
            {
                var newIdFile = new IdFile(name, Path.Join(dstSubPath, (name + ".ids")), LocalizerConfig.DstPath);
                return newIdFile;
            }

            private IdFile openId(string dstSubPath, string name, bool master)
            {
                IdFile newIdFile = null;
                if (master)
                {
                    foreach (var l in mMasterIDFiles)
                    {
                        if (l.name == name)
                            return l;
                    }

                    newIdFile = newId(dstSubPath, name);
                    mMasterIDFiles.Add(newIdFile);
                }
                else
                {
                    foreach (var l in mIDFiles)
                    {
                        if (l.name == name)
                            return l;
                    }

                    newIdFile = newId(dstSubPath, name);
                    mIDFiles.Add(newIdFile);
                }

                return newIdFile;
            }

            public bool saveIds(string dstSubPath, string name, StringTable ids, out string idsFilename)
            {
                var masterIdFile = openId(dstSubPath, "Localization", true);
                var localIdFile = newId(dstSubPath, name);
                localIdFile.add(ids);

                idsFilename = string.Empty;

                if (!localIdFile.save(mMagic))
                    return false;

                masterIdFile.add(ids);

                idsFilename = localIdFile.filename;
                return true;
            }

            public bool saveLoc(string dstSubPath, string name, string language, StringTable ids, out string locFilename)
            {
                var masterLocFile = openLoc(dstSubPath, "Localization" + "." + language, true);
                var localLocFile = newLoc(dstSubPath, name + "." + language);
                localLocFile.add(ids);

                locFilename = string.Empty;

                if (!localLocFile.save(mMagic))
                    return false;

                masterLocFile.add(ids);

                locFilename = localLocFile.filename;
                return true;
            }

            public bool loadIds(string dstSubPath, string name)
            {
                var masterIdFile = openId(dstSubPath, "Localization", true);
                var localIdFile = openId(dstSubPath, name, false);
                if (localIdFile.load())
                {
                    masterIdFile.add(localIdFile);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool loadLoc(string dstSubPath, string name, string language)
            {
                var masterLocFile = openLoc(dstSubPath, "Localization" + "." + language, true);
                var localLocFile = openLoc(dstSubPath, name + "." + language, false);
                if (localLocFile.load())
                {
                    masterLocFile.add(localLocFile);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool save()
            {
                try
                {
                    var allIdsFilename = LocalizerConfig.Excel0;
                    allIdsFilename = Path.ChangeExtension(allIdsFilename, LocalizerConfig.MainLocFileExtension);

                    allIdsFilename = allIdsFilename + ".ids";
                    IdFile allIds = new(allIdsFilename, LocalizerConfig.DstPath);
                    foreach (var l in mMasterIDFiles)
                        allIds.add(l);

                    // The main IdFile needs to be sorted by hash, and the remapping
                    // needs to be used to sort all LocFiles in the same order.
                    allIds.getRemap(out var remap);
                    allIds.remap(remap);
                    allIds.save(mMagic);

                    foreach (var l in mMasterLocFiles)
                        l.remap(remap);
                    foreach (var l in mMasterLocFiles)
                        l.save(mMagic);

                    long maxFileSize = 0;
                    foreach (var l in mMasterLocFiles)
                    {
                        if (l.filesize > maxFileSize)
                            maxFileSize = l.filesize;
                    }

                    var languageFilesListFilename = allIds.filename + ".lst";
                    var fileWithListOfLanguageFiles = TextStream.OpenForWrite(Path.Join(LocalizerConfig.DstPath, languageFilesListFilename));
                    foreach (var l in mMasterLocFiles)
                        fileWithListOfLanguageFiles.WriteLine(l.filename);
                    fileWithListOfLanguageFiles.Close();

                    allIds.saveHeaderFile((int)maxFileSize, mMagic);

                    // The dependency file
                    var depFile = DepFile.sCreate(mMainFilename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.MainDepFileExtension;
                    depFile.addOut(languageFilesListFilename, LocalizerConfig.DstPath, DepInfo.EDepRule.MUST_EXIST);
                    depFile.addOut(allIds.filename, allIds.folder, DepInfo.EDepRule.MUST_EXIST);
                    depFile.addOut(allIds.filename + ".h", allIds.folder, DepInfo.EDepRule.MUST_EXIST);

                    foreach (var l in mMasterLocFiles)
                        depFile.addOut(l.filename, l.folder, DepInfo.EDepRule.MUST_EXIST);

                    return depFile.save(LocalizerConfig.DepPath);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The localization builder
        ///
        /// Can build localization files from one excel file and multiple worksheets.
        /// Uses the LocDatabase to load/save Loc and Id files.
        /// </summary>
        public class Builder
        {
            private readonly string[] mSheetNames = null;
            private readonly string mFilename = string.Empty;
            private bool mIsModified = false;
            private List<Excel.Worksheet> mWorksheets = new();
            private Column mIDColumn = null;
            private List<Column> mColumns = new();

            /// <summary>Base class for all the library Exceptions.</summary>
            public class LocalizationBuilderException : Exception
            {
                /// <summary>Basic constructor of the exception.</summary>
                /// <param name="message">Message of the exception.</param>
                public LocalizationBuilderException(string message)
                    : base(message)
                {
                }

                /// <summary>Basic constructor of the exception.</summary>
                /// <param name="message">Message of the exception.</param>
                /// <param name="innerEx">The inner Exception.</param>
                public LocalizationBuilderException(string message, Exception innerEx)
                    : base(message, innerEx)
                {
                }
            }

            /// <summary>Indicates the wrong usage of the library.</summary>
            public class BadUsageException : LocalizationBuilderException
            {
                /// <summary>Creates an instance of an BadUsageException.</summary>
                /// <param name="message">The exception Message</param>
                protected internal BadUsageException(string message)
                    : base(message)
                {
                }
            }

            /// <summary>Indicates the wrong usage of the ExcelStorage of the library.</summary>
            public class ExcelBadUsageException : BadUsageException
            {
                /// <summary>Creates an instance of an ExcelBadUsageException.</summary>
                /// <param name="message">The exception Message</param>
                internal ExcelBadUsageException(string message)
                    : base(message)
                {
                }
            }

            public Builder(string excelFilename, string[] sheetNames)
            {
                mSheetNames = sheetNames;
                mFilename = excelFilename;
            }

            public bool isModified
            {
                get { return mIsModified; }
            }

            public bool init(LocDatabase db)
            {
                mColumns = null;

                mColumns = new List<Column>();
                foreach (var c in Settings.mColumns)
                {
                    if (c.Name == "ID")
                        mIDColumn = c;
                    else
                        mColumns.Add(c);
                }

                mIsModified = true;
                {
                    var depFile = new DepFile(mFilename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.SubDepFileExtension;
                    if (depFile.load(LocalizerConfig.DepPath))
                    {
                        mIsModified = depFile.isModified();
                    }
                }
                return true;
            }

            public bool build(LocDatabase db)
            {
                if (mIsModified)
                {
                    var sheetNames = mSheetNames;

                    FileInfo xlsFileInfo = new(LocalizerConfig.SrcPath + "\\" + mFilename);
                    if (!xlsFileInfo.Exists)
                    {
                        Console.WriteLine("Localization file \"" + xlsFileInfo.FullName + "\" could not be found.");
                        return false;
                    }

                    Excel.Workbook workbook = new(xlsFileInfo.FullName);

                    foreach (var worksheet in workbook.Sheets)
                    {
                        foreach (var s in sheetNames)
                        {
                            if (worksheet.Name.Equals(s))
                            {
                                this.mWorksheets.Add(worksheet);
                            }
                        }
                    }

                    // Compile
                    foreach (var c in Settings.mColumns)
                    {
                        if (null == mWorksheets)
                        {
                            Console.WriteLine("There are no worksheets");
                            return false;
                        }

                        if (!c.Read(mWorksheets))
                        {
                            Console.WriteLine("Worksheet was unable to read column \"" + c.Name + "\".");
                            return false;
                        }
                    }

                    // Excel objects not needed anymore


                    if (mIDColumn == null)
                    {
                        Console.WriteLine("Column ID was not defined!.");
                        return false;
                    }

                    var map = mIDColumn.Consolidate();
                    foreach (var c in mColumns)
                    {
                        if (!c.Consolidate(map))
                        {
                            Console.WriteLine("Unable to consolidate column \"" + c.Name + "\".");
                            return false;
                        }
                    }

                    for (var i = 1; i < mColumns.Count; i++)
                    {
                        if (!mColumns[i].SuperImpose(mColumns[i - 1]))
                        {
                            Console.WriteLine("Unable to superimpose column \"" + mColumns[i].Name + "\".");
                            return false;
                        }
                    }

                    for (var i = 0; i < mColumns.Count; i++)
                    {
                        mColumns[i].ValidateUsedCharacters();
                    }

                    // Make sure path exists
                    DirUtils.Create(Path.Join(LocalizerConfig.DstPath, mFilename));
                    DirUtils.Create(Path.Join(LocalizerConfig.DepPath, mFilename));

                    // The dependency file
                    DepFile depFile = new(mFilename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.SubDepFileExtension;

                    // Write "filename.ids" file
                    StringTable ids = new();
                    for (var i = 0; i < mIDColumn.Count; i++)
                        ids.Add(mIDColumn[i]);

                    string idsFilename;
                    db.saveIds(Path.GetDirectoryName(mFilename), Path.GetFileName(mFilename), ids, out idsFilename);
                    depFile.addOut(idsFilename, LocalizerConfig.DstPath, DepInfo.EDepRule.MUST_EXIST);

                    // Write all "filename.%LANGUAGE%.loc" files
                    foreach (var c in mColumns)
                    {
                        string locFilename;
                        if (!db.saveLoc(Path.GetDirectoryName(mFilename), Path.GetFileName(mFilename), c.Name, c.Table, out locFilename))
                        {
                            Console.WriteLine("Unable to save column \"" + c.Name + "\".");
                            return false;
                        }

                        depFile.addOut(locFilename, LocalizerConfig.DstPath, DepInfo.EDepRule.MUST_EXIST);
                    }

                    depFile.save(LocalizerConfig.DepPath);
                }

                return true;
            }

            public bool load(LocDatabase db)
            {
                if (!db.loadIds(Path.GetDirectoryName(mFilename), Path.GetFileName(mFilename)))
                    return false;

                foreach (var c in mColumns)
                {
                    if (c.Name == "ID")
                        continue;

                    var language = c.Name;
                    if (!db.loadLoc(Path.GetDirectoryName(mFilename), Path.GetFileName(mFilename), language))
                        return false;
                }

                return true;
            }

            private Column getColumn(string columnName)
            {
                foreach (var c in mColumns)
                    if (c.Name == columnName)
                        return c;
                return null;
            }

            public bool has(string columnName, string cell)
            {
                var column = getColumn(columnName);
                if (column != null)
                {
                    var idx = column.IndexOf(cell);
                    return idx >= 0;
                }

                return false;
            }

            public int count(string columnName)
            {
                var column = getColumn(columnName);
                if (column != null)
                {
                    return column.Count;
                }

                return 0;
            }

            public int get(string columnName, string cell)
            {
                var column = getColumn(columnName);
                if (column != null)
                {
                    var idx = column.IndexOf(cell);
                    return idx;
                }

                return -1;
            }
        }
    }
}
