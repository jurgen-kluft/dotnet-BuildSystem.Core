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
            #region Methods

            public static bool ValidateString(string inColumnName, ref string text)
            {
                char[] temp = text.ToCharArray();
                bool valid = true;
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    foreach (CharConversion conversion in Settings.mCharConversions)
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

            #endregion
        }

        public static class Settings
        {
            #region Fields

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

            #endregion
        }

        public class CharConversion
        {
            #region Fields

            private readonly char mFrom;
            private readonly char mTo;

            #endregion

            #region Constructor

            public CharConversion(char from, char to)
            {
                mFrom = from;
                mTo = to;
            }

            #endregion

            #region Properties

            public char from
            {
                get { return mFrom; }
            }

            public char to
            {
                get { return mTo; }
            }

            #endregion
        }

        public class StringTable
        {
            #region Fields

            private List<string> mStrings = new();
            private List<UInt64> mStringHashes = new();
            private uint mOffset = 0;
            private List<uint> mStringOffsets = new();

            #endregion

            #region Methods

            public void Add(string inString)
            {
                // TODO Handle UTF-8, this means here you need to correctly compute the string length in bytes
                mStrings.Add(inString);
                mStringOffsets.Add(mOffset);
                mStringHashes.Add(Hashing64.ComputeMurmur64(inString));
                UTF8Encoding utf8 = new UTF8Encoding();
                utf8.GetBytes(inString);
                mOffset += (uint)(inString.Length + 1);
            }

            public void Clear()
            {
                mStrings = new List<string>();
                mStringOffsets = new List<uint>();
                mStringHashes = new List<UInt64>();
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
                List<string> strings = mStrings;
                Clear();

                int c = 0;
                foreach (string s in strings)
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
                List<string> strings = mStrings;
                Clear();

                foreach (int i in map)
                {
                    Add(strings[i]);
                }

                return true;
            }

            public bool SuperImpose(StringTable master)
            {
                int c = 0;
                for (int i = 0; i < mStrings.Count; i++)
                {
                    if (mStrings[i] == string.Empty)
                    {
                        mStrings[i] = master[i];
                        c++;
                    }
                }

                if (c > 0)
                {
                    List<string> strings = mStrings;
                    Clear();

                    foreach (string s in strings)
                        Add(s);
                }

                return true;
            }

            class HashAndIndexPairComparer : IComparer<KeyValuePair<UInt64, Int32>>
            {
                #region IComparer<KeyValuePair<ulong,int>> Members

                public int Compare(KeyValuePair<ulong, int> x, KeyValuePair<ulong, int> y)
                {
                    if (x.Key < y.Key)
                        return -1;
                    else if (x.Key > y.Key)
                        return 1;
                    else
                        return 0;
                }

                #endregion
            }

            public void GetRemap(out List<int> outRemap)
            {
                int i = 0;
                List<KeyValuePair<UInt64, Int32>> hashes = new(Count);
                foreach (UInt64 hash in mStringHashes)
                    hashes.Add(new KeyValuePair<UInt64, Int32>(hash, i++));

                // Sort by hash
                hashes.Sort(new HashAndIndexPairComparer());

                // Fill remap
                outRemap = new List<int>(Count);
                foreach (KeyValuePair<UInt64, Int32> hash_idx in hashes)
                {
                    int index = hash_idx.Value;
                    outRemap.Add(index);
                }
            }

            public void Remap(List<int> remap)
            {
                List<string> strings = mStrings;
                Clear();

                foreach (int i in remap)
                    Add(strings[i]);
            }

            public bool Read(IBinaryReader reader)
            {
                try
                {
                    int count = reader.ReadInt32();

                    // Hashes
                    for (int i = 0; i < count; ++i)
                        reader.ReadUInt64();
                    // Offsets
                    for (int i = 0; i < count; ++i)
                        reader.ReadInt32();

                    // Strings
                    for (int i = 0; i < count; ++i)
                    {
                        string str = reader.ReadString();
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

                    foreach (UInt64 hash in mStringHashes)
                        writer.Write(hash);
                    foreach (uint offset in mStringOffsets)
                        writer.Write(offset);
                    foreach (string s in mStrings)
                        writer.Write(s);
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            #endregion

            #region Properties

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

            #endregion
        }

        public class Column
        {
            #region Fields

            private readonly string mName;
            private readonly int mColumn;
            private readonly int[] mRowRange;
            private StringTable mStringTable = null;

            #endregion

            #region Constructor

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

            #endregion

            #region Properties

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

            #endregion

            #region Methods

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
                bool valid = true;
                for (int i = 0; i < mStringTable.All.Count; i++)
                {
                    string str = mStringTable.All[i];
                    valid = valid && Validation.ValidateString(mName, ref str);
                    mStringTable.All[i] = str;
                }

                return valid;
            }

            public int IndexOf(string cellContent)
            {
                return mStringTable.IndexOf(cellContent);
            }

            #endregion

            #region Read

            public bool ReadColumn(Excel.Worksheet worksheet, int columnNumber, int rowStart, int rowEnd, out List<string> columnText)
            {
                columnText = new List<string>();
                for (int r = rowStart; r < rowEnd; r++)
                {
                    Excel.Row row = worksheet.Rows[(ushort)r];
                    if (row != null)
                    {
                        Excel.Cell cell = row.Cells[(byte)columnNumber];
                        if (cell != null)
                        {
                            string textcell = cell.FormattedValue();
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
                List<string> columnTextList = new List<string>();
                List<string> columnText = new List<string>();

                foreach (Excel.Worksheet worksheet in worksheets)
                {
                    if (ReadColumn(worksheet, mColumn, mRowRange[0], mRowRange[1], out columnText))
                    {
                        foreach (string s in columnText)
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
                StringTable strTable = new StringTable();
                foreach (string s in columnText)
                    strTable.Add(s);
                return strTable;
            }

            #endregion

            #region Save

            // Return the written data size
            public int Save(string filename, Int64 magic)
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
                    IBinaryStreamWriter writer = EndianUtils.CreateBinaryWriter(fileStream, LocalizerConfig.Platform);

                    writer.Write((Int32)(magic >> 32));
                    writer.Write((Int32)(magic));

                    bool result = mStringTable.Write(writer);

                    if (!result)
                    {
                        Console.WriteLine("Error occurred when writing language file \"" + Name + "\".");
                    }

                    int size = (int)writer.Position;

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

            #endregion
        }

        #region IdFile

        public class IdFile
        {
            #region Fields

            private string mName = string.Empty;
            private string mFilename = string.Empty;
            private string mFolder = string.Empty;
            private Int64 mFileSize = 0;
            private StringTable mStrTable = new StringTable();

            #endregion

            #region Constructors

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

            #endregion

            #region Properties

            public string filename
            {
                get { return mFilename; }
            }

            public string folder
            {
                get { return mFolder; }
            }

            public Int64 filesize
            {
                get { return mFileSize; }
            }

            public string name
            {
                get { return mName; }
            }

            #endregion

            #region Methods

            public void clear()
            {
                mStrTable = new StringTable();
            }


            public void add(StringTable strTable)
            {
                for (int i = 0; i < strTable.Count; ++i)
                    mStrTable.Add(strTable[i]);
            }

            public void add(IdFile idFile)
            {
                for (int i = 0; i < idFile.mStrTable.Count; ++i)
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

            #endregion

            #region load

            public bool load()
            {
                try
                {
                    FileInfo fileinfoXlsIds = new FileInfo(mFolder + mFilename);
                    if (fileinfoXlsIds.Exists)
                    {
                        mFileSize = fileinfoXlsIds.Length;

                        FileStream filestreamXlsIds = new FileStream(fileinfoXlsIds.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        IBinaryStreamReader filestreamXlsIdsReader = EndianUtils.CreateBinaryReader(filestreamXlsIds, LocalizerConfig.Platform);

                        Int64 magic = filestreamXlsIdsReader.ReadInt64();
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

            #endregion

            #region save

            public bool save(Int64 magic)
            {
                try
                {
                    IBinaryStreamWriter writer = EndianUtils.CreateBinaryWriter(mFolder + mFilename, Platform.Current);

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

            #endregion

            #region C Header File

            public bool saveHeaderFile(int maxFileSize, Int64 magic)
            {
                try
                {
                    TextStream textStream = new(mFolder + Path.ChangeExtension(mFilename, (".h")));
                    textStream.Open(TextStream.EMode.Write);

                    string line;

                    textStream.Writer.WriteLine("#ifndef __LOCALIZATION_IDS_H__");
                    textStream.Writer.WriteLine("#define __LOCALIZATION_IDS_H__");
                    textStream.Writer.WriteLine("");
                    textStream.Writer.WriteLine("");
                    line = String.Format("#define\t\t{0}\t\t\t0x{1:X8}", "LOCALIZATION_VERSION_H", (Int32)(magic >> 32));
                    textStream.Writer.WriteLine(line);
                    line = String.Format("#define\t\t{0}\t\t\t0x{1:X8}", "LOCALIZATION_VERSION_L", (Int32)(magic));
                    textStream.Writer.WriteLine(line);
                    textStream.Writer.WriteLine("");
                    textStream.Writer.WriteLine("");
                    line = String.Format("#define\t\t{0}\t\t\t0x{1:X8}", "LOCALIZATION_MAX_FILE_SIZE", maxFileSize);
                    textStream.Writer.WriteLine(line);
                    textStream.Writer.WriteLine("");

                    for (int i = 0; i < mStrTable.Count; ++i)
                    {
                        line = String.Format("#define\t\t{0}\t\t\t{1}", mStrTable[i], i);
                        textStream.Writer.WriteLine(line);
                    }

                    textStream.Writer.WriteLine("");
                    textStream.Writer.WriteLine("");
                    textStream.Writer.WriteLine("#endif ///< __LOCALIZATION_IDS_H__");

                    textStream.Close();

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception (" + e.ToString() + ") while writing localization id header file \"" + filename + "\".");
                    return false;
                }
            }

            #endregion
        }

        #endregion

        #region LocFile

        public class LocFile
        {
            #region Fields

            private string mName = string.Empty;
            private string mFilename = string.Empty;
            private string mFolder = string.Empty;
            private Int64 mFileSize = 0;
            private StringTable mStrTable = new StringTable();

            #endregion

            #region Constructors

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

            #endregion

            #region Properties

            public string filename
            {
                get { return mFilename; }
            }

            public string folder
            {
                get { return mFolder; }
            }

            public Int64 filesize
            {
                get { return mFileSize; }
            }

            public string name
            {
                get { return mName; }
            }

            #endregion

            #region Methods

            public void clear()
            {
                mStrTable = new StringTable();
            }


            public void add(StringTable strTable)
            {
                for (int i = 0; i < strTable.Count; ++i)
                    mStrTable.Add(strTable[i]);
            }

            public void add(LocFile locFile)
            {
                for (int i = 0; i < locFile.mStrTable.Count; ++i)
                    mStrTable.Add(locFile.mStrTable[i]);
            }

            #endregion

            #region load

            public bool load()
            {
                try
                {
                    FileInfo fileinfoXlsIds = new(Path.Join(mFolder, mFilename));
                    if (fileinfoXlsIds.Exists)
                    {
                        mFileSize = fileinfoXlsIds.Length;

                        FileStream filestreamXlsIds = new(fileinfoXlsIds.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        IBinaryStreamReader filestreamXlsIdsReader = EndianUtils.CreateBinaryReader(filestreamXlsIds, Platform.Current);

                        Int64 magic = filestreamXlsIdsReader.ReadInt64();
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

            #endregion

            #region Remap

            public void remap(List<int> map)
            {
                mStrTable.Remap(map);
            }

            #endregion

            #region save

            public bool save(Int64 magic)
            {
                try
                {
                    IBinaryStreamWriter writer = EndianUtils.CreateBinaryWriter(Path.Join(mFolder, mFilename), Platform.Current);

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

            #endregion
        }

        #endregion

        #region LocDatabase

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
            private readonly Int64 mMagic = DateTime.Now.Ticks;
            private readonly List<LocFile> mLocFiles = new();
            private readonly List<LocFile> mMasterLocFiles = new();
            private readonly List<IdFile> mIDFiles = new();
            private readonly List<IdFile> mMasterIDFiles = new();

            public Int64 magic
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
                    foreach (LocFile l in mMasterLocFiles)
                        if (l.name == name)
                            return l;
                    newLocFile = newLoc(dstSubPath, name);
                    mMasterLocFiles.Add(newLocFile);
                }
                else
                {
                    foreach (LocFile l in mLocFiles)
                        if (l.name == name)
                            return l;
                    newLocFile = newLoc(dstSubPath, name);
                    mLocFiles.Add(newLocFile);
                }

                return newLocFile;
            }

            private IdFile newId(string dstSubPath, string name)
            {
                IdFile newIdFile = new IdFile(name, Path.Join(dstSubPath, (name + ".ids")), LocalizerConfig.DstPath);
                return newIdFile;
            }

            private IdFile openId(string dstSubPath, string name, bool master)
            {
                IdFile newIdFile = null;
                if (master)
                {
                    foreach (IdFile l in mMasterIDFiles)
                    {
                        if (l.name == name)
                            return l;
                    }

                    newIdFile = newId(dstSubPath, name);
                    mMasterIDFiles.Add(newIdFile);
                }
                else
                {
                    foreach (IdFile l in mIDFiles)
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
                IdFile masterIdFile = openId(dstSubPath, "Localization", true);
                IdFile localIdFile = newId(dstSubPath, name);
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
                LocFile masterLocFile = openLoc(dstSubPath, "Localization" + "." + language, true);
                LocFile localLocFile = newLoc(dstSubPath, name + "." + language);
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
                IdFile masterIdFile = openId(dstSubPath, "Localization", true);
                IdFile localIdFile = openId(dstSubPath, name, false);
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
                LocFile masterLocFile = openLoc(dstSubPath, "Localization" + "." + language, true);
                LocFile localLocFile = openLoc(dstSubPath, name + "." + language, false);
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
                    string allIdsFilename = LocalizerConfig.Excel0;
                    allIdsFilename = Path.ChangeExtension(allIdsFilename, LocalizerConfig.MainLocFileExtension);

                    allIdsFilename = allIdsFilename + ".ids";
                    IdFile allIds = new(allIdsFilename, LocalizerConfig.DstPath);
                    foreach (IdFile l in mMasterIDFiles)
                        allIds.add(l);

                    // The main IdFile needs to be sorted by hash, and the remapping
                    // needs to be used to sort all LocFiles in the same order.
                    allIds.getRemap(out List<int> remap);
                    allIds.remap(remap);
                    allIds.save(mMagic);

                    foreach (LocFile l in mMasterLocFiles)
                        l.remap(remap);
                    foreach (LocFile l in mMasterLocFiles)
                        l.save(mMagic);

                    Int64 maxFileSize = 0;
                    foreach (LocFile l in mMasterLocFiles)
                    {
                        if (l.filesize > maxFileSize)
                            maxFileSize = l.filesize;
                    }

                    string languageFilesListFilename = allIds.filename + ".lst";
                    TextStream fileWithListOfLanguageFiles = new(Path.Join(LocalizerConfig.DstPath, languageFilesListFilename));
                    fileWithListOfLanguageFiles.Open(TextStream.EMode.Write);
                    foreach (LocFile l in mMasterLocFiles)
                        fileWithListOfLanguageFiles.Writer.WriteLine(l.filename);
                    fileWithListOfLanguageFiles.Close();

                    allIds.saveHeaderFile((Int32)maxFileSize, mMagic);

                    // The dependency file
                    DepFile depFile = DepFile.sCreate(mMainFilename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.MainDepFileExtension;
                    depFile.addOut(languageFilesListFilename, LocalizerConfig.DstPath, DepInfo.EDepRule.MUST_EXIST);
                    depFile.addOut(allIds.filename, allIds.folder, DepInfo.EDepRule.MUST_EXIST);
                    depFile.addOut(allIds.filename + ".h", allIds.folder, DepInfo.EDepRule.MUST_EXIST);

                    foreach (LocFile l in mMasterLocFiles)
                        depFile.addOut(l.filename, l.folder, DepInfo.EDepRule.MUST_EXIST);

                    return depFile.save(LocalizerConfig.DepPath);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        #endregion

        /// <summary>
        /// The localization builder
        ///
        /// Can build localization files from one excel file and multiple worksheets.
        /// Uses the LocDatabase to load/save Loc and Id files.
        /// </summary>
        public class Builder
        {
            #region Fields

            private readonly string[] mSheetNames = null;
            private readonly string mFilename = string.Empty;
            private bool mIsModified = false;
            private List<Excel.Worksheet> mWorksheets = new();
            private Column mIDColumn = null;
            private List<Column> mColumns = new();

            #endregion

            #region Exceptions

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

            #endregion

            #region Constructor

            public Builder(string excelFilename, string[] sheetNames)
            {
                mSheetNames = sheetNames;
                mFilename = excelFilename;
            }

            #endregion

            #region Properties

            public bool isModified
            {
                get { return mIsModified; }
            }

            #endregion

            #region Methods

            public bool init(LocDatabase db)
            {
                mColumns = null;

                mColumns = new List<Column>();
                foreach (Column c in Settings.mColumns)
                {
                    if (c.Name == "ID")
                        mIDColumn = c;
                    else
                        mColumns.Add(c);
                }

                mIsModified = true;
                {
                    DepFile depFile = new DepFile(mFilename, LocalizerConfig.SrcPath);
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
                    string[] sheetNames = mSheetNames;

                    FileInfo xlsFileInfo = new(LocalizerConfig.SrcPath + "\\" + mFilename);
                    if (!xlsFileInfo.Exists)
                    {
                        Console.WriteLine("Localization file \"" + xlsFileInfo.FullName + "\" could not be found.");
                        return false;
                    }

                    Excel.Workbook workbook = new(xlsFileInfo.FullName);

                    foreach (Excel.Worksheet worksheet in workbook.Sheets)
                    {
                        foreach (string s in sheetNames)
                        {
                            if (worksheet.Name.Equals(s))
                            {
                                this.mWorksheets.Add(worksheet);
                            }
                        }
                    }

                    // Compile
                    foreach (Column c in Settings.mColumns)
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

                    List<int> map = mIDColumn.Consolidate();
                    foreach (Column c in mColumns)
                    {
                        if (!c.Consolidate(map))
                        {
                            Console.WriteLine("Unable to consolidate column \"" + c.Name + "\".");
                            return false;
                        }
                    }

                    for (int i = 1; i < mColumns.Count; i++)
                    {
                        if (!mColumns[i].SuperImpose(mColumns[i - 1]))
                        {
                            Console.WriteLine("Unable to superimpose column \"" + mColumns[i].Name + "\".");
                            return false;
                        }
                    }

                    for (int i = 0; i < mColumns.Count; i++)
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
                    for (int i = 0; i < mIDColumn.Count; i++)
                        ids.Add(mIDColumn[i]);

                    string idsFilename;
                    db.saveIds(Path.GetDirectoryName(mFilename), Path.GetFileName(mFilename), ids, out idsFilename);
                    depFile.addOut(idsFilename, LocalizerConfig.DstPath, DepInfo.EDepRule.MUST_EXIST);

                    // Write all "filename.%LANGUAGE%.loc" files
                    foreach (Column c in mColumns)
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

                foreach (Column c in mColumns)
                {
                    if (c.Name == "ID")
                        continue;

                    string language = c.Name;
                    if (!db.loadLoc(Path.GetDirectoryName(mFilename), Path.GetFileName(mFilename), language))
                        return false;
                }

                return true;
            }

            #endregion

            #region Column Methods

            private Column getColumn(string columnName)
            {
                foreach (Column c in mColumns)
                    if (c.Name == columnName)
                        return c;
                return null;
            }

            public bool has(string columnName, string cell)
            {
                Column column = getColumn(columnName);
                if (column != null)
                {
                    int idx = column.IndexOf(cell);
                    return idx >= 0;
                }

                return false;
            }

            public int count(string columnName)
            {
                Column column = getColumn(columnName);
                if (column != null)
                {
                    return column.Count;
                }

                return 0;
            }

            public int get(string columnName, string cell)
            {
                Column column = getColumn(columnName);
                if (column != null)
                {
                    int idx = column.IndexOf(cell);
                    return idx;
                }

                return -1;
            }

            #endregion
        }
    }
}
