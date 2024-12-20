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
                    foreach (var conversion in Settings.s_charConversions)
                    {
                        if (c == conversion.From)
                        {
                            c = conversion.To;
                            break;
                        }
                    }

                    // check if the character is supported (in any language)
                    if (c >= 0 && c < 256 && Settings.s_charSet[c] != 0)
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
            private const int BeginRow = 15;
            private const int EndRow = 2048;

            public static readonly string s_logLevel = "Info";

            public static readonly Column[] s_columns =
            {
                new Column("ID", 0, BeginRow, EndRow),
                new Column("English", 3, BeginRow, EndRow),
                new Column("French", 4, BeginRow, EndRow),
                new Column("Italian", 5, BeginRow, EndRow),
                new Column("German", 6, BeginRow, EndRow),
                new Column("Spanish", 7, BeginRow, EndRow),
                new Column("English_US", 8, BeginRow, EndRow),
            };

            public static readonly byte[] s_charSet =
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


            public static readonly CharConversion[] s_charConversions =
            {
                new CharConversion((char)0x152, (char)0x8c),
                new CharConversion((char)0x153, (char)0x9c),
                new CharConversion((char)0xBA, (char)0xB0), // replace little round by another similar round
                new CharConversion((char)0x2019, (char)0x27)
            };
        }

        public class CharConversion
        {
            public CharConversion(char from, char to)
            {
                this.From = from;
                this.To = to;
            }

            public char From { get; }

            public char To { get; }
        }

        public class StringTable
        {
            private List<Hash160> _stringHashes = new();
            private uint _offset;
            private List<uint> _stringOffsets = new();

            public void Add(string inString)
            {
                // TODO Handle UTF-8, this means here you need to correctly compute the string length in bytes
                All.Add(inString);
                _stringOffsets.Add(_offset);
                _stringHashes.Add(HashUtility.Compute_UTF8(inString));
                var utf8 = new UTF8Encoding();
                utf8.GetBytes(inString);
                _offset += (uint)(inString.Length + 1);
            }

            public void Clear()
            {
                All = new List<string>();
                _stringOffsets = new List<uint>();
                _stringHashes = new List<Hash160>();
                _offset = 0;
            }

            public int IndexOf(string inString)
            {
                return All.IndexOf(inString);
            }

            public uint OffsetOf(string inString)
            {
                return _stringOffsets[IndexOf(inString)];
            }

            public List<int> Consolidate()
            {
                List<int> map = new();
                var strings = All;
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
                var strings = All;
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
                for (var i = 0; i < All.Count; i++)
                {
                    if (All[i] == string.Empty)
                    {
                        All[i] = master[i];
                        c++;
                    }
                }

                if (c > 0)
                {
                    var strings = All;
                    Clear();

                    foreach (var s in strings)
                        Add(s);
                }

                return true;
            }

            class HashAndIndexPairComparer : IComparer<KeyValuePair<Hash160, int>>
            {
                public int Compare(KeyValuePair<Hash160, int> x, KeyValuePair<Hash160, int> y)
                {
                    return Hash160.Compare(x.Key, y.Key);
                }
            }

            public void GetRemap(out List<int> outRemap)
            {
                var i = 0;
                List<KeyValuePair<Hash160, int>> hashes = new(Count);
                foreach (var hash in _stringHashes)
                    hashes.Add(new KeyValuePair<Hash160, int>(hash, i++));

                // Sort by hash
                hashes.Sort(new HashAndIndexPairComparer());

                // Fill remap
                outRemap = new List<int>(Count);
                foreach (var hashIdx in hashes)
                {
                    var index = hashIdx.Value;
                    outRemap.Add(index);
                }
            }

            public void Remap(List<int> remap)
            {
                var strings = All;
                Clear();

                foreach (var i in remap)
                    Add(strings[i]);
            }

            public bool Read(IBinaryReader reader)
            {
                try
                {
                    GameCore.BinaryReader.Read(reader, out int count);

                    // Hashes
                    for (var i = 0; i < count; ++i)
                        GameCore.BinaryReader.Read(reader, out long h);

                    // Offsets
                    for (var i = 0; i < count; ++i)
                        GameCore.BinaryReader.Read(reader, out int o);

                    // Strings
                    for (var i = 0; i < count; ++i)
                    {
                        GameCore.BinaryReader.Read(reader, out string str);
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
                    GameCore.BinaryWriter.Write(writer,Count);

                    foreach (var hash in _stringHashes)
                        GameCore.BinaryWriter.Write(writer, hash.AsHash64());
                    foreach (var offset in _stringOffsets)
                        GameCore.BinaryWriter.Write(writer,offset);
                    foreach (var s in All)
                        GameCore.BinaryWriter.Write(writer, s);
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            public int Count
            {
                get { return All.Count; }
            }

            public List<string> All { get; private set; } = new();

            public string this[int index]
            {
                get { return All[index]; }
            }
        }

        public class Column
        {
            private readonly int _column;
            private readonly int[] _rowRange;

            public Column(string name, int column, int[] rowRange)
            {
                Name = name;
                _column = column;
                _rowRange = rowRange;
            }

            public Column(string name, int column, int rowBegin, int rowEnd)
            {
                Name = name;
                _column = column;
                _rowRange = new int[2];
                _rowRange[0] = rowBegin;
                _rowRange[1] = rowEnd;
            }

            public int Count
            {
                get { return Table.Count; }
            }

            public string Name { get; }

            public string this[int index]
            {
                get { return Table[index]; }
            }

            public StringTable Table { get; private set; } = null;

            public List<int> Consolidate()
            {
                return Table.Consolidate();
            }

            public bool Consolidate(List<int> map)
            {
                return Table.Consolidate(map);
            }

            public bool SuperImpose(Column c)
            {
                // Cells that are empty are copied from the incoming column
                return Table.SuperImpose(c.Table);
            }

            public bool ValidateUsedCharacters()
            {
                var valid = true;
                for (var i = 0; i < Table.All.Count; i++)
                {
                    var str = Table.All[i];
                    valid = valid && Validation.ValidateString(Name, ref str);
                    Table.All[i] = str;
                }

                return valid;
            }

            public int IndexOf(string cellContent)
            {
                return Table.IndexOf(cellContent);
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
                new List<string>();

                foreach (var worksheet in worksheets)
                {
                    if (ReadColumn(worksheet, _column, _rowRange[0], _rowRange[1], out List<string> columnText))
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


                Table = ConvertToStringTable(columnTextList);
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
                    var writer = ArchitectureUtils.CreateFileWriter(fileStream, LocalizerConfig.Platform);

                    GameCore.BinaryWriter.Write(writer,(int)(magic >> 32));
                    GameCore.BinaryWriter.Write(writer,(int)(magic));

                    var result = Table.Write(writer);

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
                    Console.WriteLine("Exception (" + e + ") while writing language file \"" + Name + "\".");
                    return 0;
                }
            }
        }

        public class IdFile
        {
            private StringTable _strTable = new StringTable();

            public IdFile(string filename, string folder)
            {
                Filename = filename;
                this.Folder = folder;
            }

            public IdFile(string name, string filename, string folder)
            {
                this.Name = name;
                Filename = filename;
                this.Folder = folder;
            }

            public string Filename { get; }

            public string Folder { get; }

            public long Filesize { get; private set; } = 0;

            public string Name { get; } = string.Empty;

            public void Clear()
            {
                _strTable = new StringTable();
            }


            public void Add(StringTable strTable)
            {
                for (var i = 0; i < strTable.Count; ++i)
                    _strTable.Add(strTable[i]);
            }

            public void Add(IdFile idFile)
            {
                for (var i = 0; i < idFile._strTable.Count; ++i)
                    _strTable.Add(idFile._strTable[i]);
            }

            public void GetRemap(out List<int> remap)
            {
                _strTable.GetRemap(out remap);
            }

            public void Remap(List<int> remap)
            {
                _strTable.Remap(remap);
            }

            public bool Load()
            {
                try
                {
                    var fileinfoXlsIds = new FileInfo(Folder + Filename);
                    if (fileinfoXlsIds.Exists)
                    {
                        Filesize = fileinfoXlsIds.Length;

                        var filestreamXlsIds = new FileStream(fileinfoXlsIds.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var filestreamXlsIdsReader = ArchitectureUtils.CreateFileReader(filestreamXlsIds, LocalizerConfig.Platform);

                        GameCore.BinaryReader.Read(filestreamXlsIdsReader, out long magic);
                        _strTable.Read(filestreamXlsIdsReader);

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

            public bool Save(long magic)
            {
                try
                {
                    var writer = ArchitectureUtils.CreateFileWriter(Folder + Filename, Platform.Current);

                    GameCore.BinaryWriter.Write(writer,magic);
                    _strTable.Write(writer);

                    writer.Close();

                    FileInfo fileinfoXlsIds = new(Folder + Filename);
                    if (fileinfoXlsIds.Exists)
                    {
                        Filesize = fileinfoXlsIds.Length;
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

            public bool SaveHeaderFile(int maxFileSize, long magic)
            {
                try
                {
                    var textStream = TextStream.OpenForWrite(Folder + Path.ChangeExtension(Filename, (".h")));

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

                    for (var i = 0; i < _strTable.Count; ++i)
                    {
                        line = string.Format("#define\t\t{0}\t\t\t{1}", _strTable[i], i);
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
                    Console.WriteLine("Exception (" + e.ToString() + ") while writing localization id header file \"" + Filename + "\".");
                    return false;
                }
            }
        }

        public class LocFile
        {
            private StringTable _strTable = new StringTable();

            public LocFile(string filename, string folder)
            {
                Filename = filename;
                Folder = folder;
            }

            public LocFile(string name, string filename, string folder)
            {
                Name = name;
                Filename = filename;
                Folder = folder;
            }

            public string Filename { get; }

            public string Folder { get; }

            public long Filesize { get; private set; } = 0;

            public string Name { get; } = string.Empty;

            public void Clear()
            {
                _strTable = new StringTable();
            }


            public void Add(StringTable strTable)
            {
                for (var i = 0; i < strTable.Count; ++i)
                    _strTable.Add(strTable[i]);
            }

            public void Add(LocFile locFile)
            {
                for (var i = 0; i < locFile._strTable.Count; ++i)
                    _strTable.Add(locFile._strTable[i]);
            }

            public bool Load()
            {
                try
                {
                    FileInfo fileinfoXlsIds = new(Path.Join(Folder, Filename));
                    if (fileinfoXlsIds.Exists)
                    {
                        Filesize = fileinfoXlsIds.Length;

                        var filestreamXlsIds = new FileStream(fileinfoXlsIds.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var filestreamXlsIdsReader = ArchitectureUtils.CreateFileReader(filestreamXlsIds, Platform.Current);

                        //var magic = filestreamXlsIdsReader.ReadInt64();
                        GameCore.BinaryReader.Read(filestreamXlsIdsReader, out long magic);
                        _strTable.Read(filestreamXlsIdsReader);

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

            public void Remap(List<int> map)
            {
                _strTable.Remap(map);
            }

            public bool Save(long magic)
            {
                try
                {
                    var writer = ArchitectureUtils.CreateFileWriter(Path.Join(Folder, Filename), Platform.Current);

                    //writer.Write(magic);
                    GameCore.BinaryWriter.Write(writer,magic);
                    _strTable.Write(writer);

                    writer.Close();

                    FileInfo fileInfoXlsIds = new(Path.Join(Folder, Filename));
                    if (fileInfoXlsIds.Exists)
                    {
                        Filesize = fileInfoXlsIds.Length;
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
            private string _mainFilename;
            private readonly List<LocFile> _locFiles = new();
            private readonly List<LocFile> _masterLocFiles = new();
            private readonly List<IdFile> _idFiles = new();
            private readonly List<IdFile> _masterIdFiles = new();

            private long Magic { get; } = DateTime.Now.Ticks;

            public bool IsModified { get; private set; } = true;

            public void Init(string main)
            {
                _mainFilename = main;

                IsModified = true;
                {
                    DepFile depFile = new(_mainFilename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.MainDepFileExtension;

                    if (depFile.load(LocalizerConfig.DepPath))
                    {
                        IsModified = depFile.isModified();
                    }
                }
            }

            private LocFile NewLoc(string dstSubPath, string name)
            {
                LocFile newLocFile = new(name, dstSubPath + (name + LocalizerConfig.MainLocFileExtension), LocalizerConfig.DstPath);
                return newLocFile;
            }

            private LocFile OpenLoc(string dstSubPath, string name, bool master)
            {
                LocFile newLocFile = null;
                if (master)
                {
                    foreach (var l in _masterLocFiles)
                        if (l.Name == name)
                            return l;
                    newLocFile = NewLoc(dstSubPath, name);
                    _masterLocFiles.Add(newLocFile);
                }
                else
                {
                    foreach (var l in _locFiles)
                        if (l.Name == name)
                            return l;
                    newLocFile = NewLoc(dstSubPath, name);
                    _locFiles.Add(newLocFile);
                }

                return newLocFile;
            }

            private IdFile NewId(string dstSubPath, string name)
            {
                var newIdFile = new IdFile(name, Path.Join(dstSubPath, (name + ".ids")), LocalizerConfig.DstPath);
                return newIdFile;
            }

            private IdFile OpenId(string dstSubPath, string name, bool master)
            {
                IdFile newIdFile = null;
                if (master)
                {
                    foreach (var l in _masterIdFiles)
                    {
                        if (l.Name == name)
                            return l;
                    }

                    newIdFile = NewId(dstSubPath, name);
                    _masterIdFiles.Add(newIdFile);
                }
                else
                {
                    foreach (var l in _idFiles)
                    {
                        if (l.Name == name)
                            return l;
                    }

                    newIdFile = NewId(dstSubPath, name);
                    _idFiles.Add(newIdFile);
                }

                return newIdFile;
            }

            public bool SaveIds(string dstSubPath, string name, StringTable ids, out string idsFilename)
            {
                var masterIdFile = OpenId(dstSubPath, "Localization", true);
                var localIdFile = NewId(dstSubPath, name);
                localIdFile.Add(ids);

                idsFilename = string.Empty;

                if (!localIdFile.Save(Magic))
                    return false;

                masterIdFile.Add(ids);

                idsFilename = localIdFile.Filename;
                return true;
            }

            public bool SaveLoc(string dstSubPath, string name, string language, StringTable ids, out string locFilename)
            {
                var masterLocFile = OpenLoc(dstSubPath, "Localization" + "." + language, true);
                var localLocFile = NewLoc(dstSubPath, name + "." + language);
                localLocFile.Add(ids);

                locFilename = string.Empty;

                if (!localLocFile.Save(Magic))
                    return false;

                masterLocFile.Add(ids);

                locFilename = localLocFile.Filename;
                return true;
            }

            public bool LoadIds(string dstSubPath, string name)
            {
                var masterIdFile = OpenId(dstSubPath, "Localization", true);
                var localIdFile = OpenId(dstSubPath, name, false);
                if (localIdFile.Load())
                {
                    masterIdFile.Add(localIdFile);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool LoadLoc(string dstSubPath, string name, string language)
            {
                var masterLocFile = OpenLoc(dstSubPath, "Localization" + "." + language, true);
                var localLocFile = OpenLoc(dstSubPath, name + "." + language, false);
                if (localLocFile.Load())
                {
                    masterLocFile.Add(localLocFile);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool Save()
            {
                try
                {
                    var allIdsFilename = LocalizerConfig.Excel0;
                    allIdsFilename = Path.ChangeExtension(allIdsFilename, LocalizerConfig.MainLocFileExtension);

                    allIdsFilename = allIdsFilename + ".ids";
                    IdFile allIds = new(allIdsFilename, LocalizerConfig.DstPath);
                    foreach (var l in _masterIdFiles)
                        allIds.Add(l);

                    // The main IdFile needs to be sorted by hash, and the remapping
                    // needs to be used to sort all LocFiles in the same order.
                    allIds.GetRemap(out var remap);
                    allIds.Remap(remap);
                    allIds.Save(Magic);

                    foreach (var l in _masterLocFiles)
                        l.Remap(remap);
                    foreach (var l in _masterLocFiles)
                        l.Save(Magic);

                    long maxFileSize = 0;
                    foreach (var l in _masterLocFiles)
                    {
                        if (l.Filesize > maxFileSize)
                            maxFileSize = l.Filesize;
                    }

                    var languageFilesListFilename = allIds.Filename + ".lst";
                    var fileWithListOfLanguageFiles = TextStream.OpenForWrite(Path.Join(LocalizerConfig.DstPath, languageFilesListFilename));
                    foreach (var l in _masterLocFiles)
                        fileWithListOfLanguageFiles.WriteLine(l.Filename);
                    fileWithListOfLanguageFiles.Close();

                    allIds.SaveHeaderFile((int)maxFileSize, Magic);

                    // The dependency file
                    var depFile = DepFile.sCreate(_mainFilename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.MainDepFileExtension;
                    depFile.addOut(languageFilesListFilename, LocalizerConfig.DstPath, DepInfo.EDepRule.MUST_EXIST);
                    depFile.addOut(allIds.Filename, allIds.Folder, DepInfo.EDepRule.MUST_EXIST);
                    depFile.addOut(allIds.Filename + ".h", allIds.Folder, DepInfo.EDepRule.MUST_EXIST);

                    foreach (var l in _masterLocFiles)
                        depFile.addOut(l.Filename, l.Folder, DepInfo.EDepRule.MUST_EXIST);

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
            private readonly string[] _sheetNames = null;
            private readonly string _filename;
            private readonly List<Excel.Worksheet> _worksheets = new();
            private Column _idColumn = null;
            private List<Column> _columns = new();

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
                _sheetNames = sheetNames;
                _filename = excelFilename;
            }

            public bool IsModified { get; private set; } = false;

            public bool Init(LocDatabase db)
            {
                _columns = null;

                _columns = new List<Column>();
                foreach (var c in Settings.s_columns)
                {
                    if (c.Name == "ID")
                        _idColumn = c;
                    else
                        _columns.Add(c);
                }

                IsModified = true;
                {
                    var depFile = new DepFile(_filename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.SubDepFileExtension;
                    if (depFile.load(LocalizerConfig.DepPath))
                    {
                        IsModified = depFile.isModified();
                    }
                }
                return true;
            }

            public bool Build(LocDatabase db)
            {
                if (IsModified)
                {
                    var sheetNames = _sheetNames;

                    FileInfo xlsFileInfo = new(LocalizerConfig.SrcPath + "\\" + _filename);
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
                                this._worksheets.Add(worksheet);
                            }
                        }
                    }

                    // Compile
                    foreach (var c in Settings.s_columns)
                    {
                        if (null == _worksheets)
                        {
                            Console.WriteLine("There are no worksheets");
                            return false;
                        }

                        if (!c.Read(_worksheets))
                        {
                            Console.WriteLine("Worksheet was unable to read column \"" + c.Name + "\".");
                            return false;
                        }
                    }

                    // Excel objects not needed anymore


                    if (_idColumn == null)
                    {
                        Console.WriteLine("Column ID was not defined!.");
                        return false;
                    }

                    var map = _idColumn.Consolidate();
                    foreach (var c in _columns)
                    {
                        if (!c.Consolidate(map))
                        {
                            Console.WriteLine("Unable to consolidate column \"" + c.Name + "\".");
                            return false;
                        }
                    }

                    for (var i = 1; i < _columns.Count; i++)
                    {
                        if (!_columns[i].SuperImpose(_columns[i - 1]))
                        {
                            Console.WriteLine("Unable to superimpose column \"" + _columns[i].Name + "\".");
                            return false;
                        }
                    }

                    for (var i = 0; i < _columns.Count; i++)
                    {
                        _columns[i].ValidateUsedCharacters();
                    }

                    // Make sure path exists
                    DirUtils.Create(Path.Join(LocalizerConfig.DstPath, _filename));
                    DirUtils.Create(Path.Join(LocalizerConfig.DepPath, _filename));

                    // The dependency file
                    DepFile depFile = new(_filename, LocalizerConfig.SrcPath);
                    depFile.extension = LocalizerConfig.SubDepFileExtension;

                    // Write "filename.ids" file
                    StringTable ids = new();
                    for (var i = 0; i < _idColumn.Count; i++)
                        ids.Add(_idColumn[i]);

                    db.SaveIds(Path.GetDirectoryName(_filename), Path.GetFileName(_filename), ids, out string idsFilename);
                    depFile.addOut(idsFilename, LocalizerConfig.DstPath, DepInfo.EDepRule.MUST_EXIST);

                    // Write all "filename.%LANGUAGE%.loc" files
                    foreach (var c in _columns)
                    {
                        if (!db.SaveLoc(Path.GetDirectoryName(_filename), Path.GetFileName(_filename), c.Name, c.Table, out string locFilename))
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

            public bool Load(LocDatabase db)
            {
                if (!db.LoadIds(Path.GetDirectoryName(_filename), Path.GetFileName(_filename)))
                    return false;

                foreach (var c in _columns)
                {
                    if (c.Name == "ID")
                        continue;

                    var language = c.Name;
                    if (!db.LoadLoc(Path.GetDirectoryName(_filename), Path.GetFileName(_filename), language))
                        return false;
                }

                return true;
            }

            private Column GetColumn(string columnName)
            {
                foreach (var c in _columns)
                    if (c.Name == columnName)
                        return c;
                return null;
            }

            public bool Has(string columnName, string cell)
            {
                var column = GetColumn(columnName);
                if (column != null)
                {
                    var idx = column.IndexOf(cell);
                    return idx >= 0;
                }

                return false;
            }

            public int Count(string columnName)
            {
                var column = GetColumn(columnName);
                if (column != null)
                {
                    return column.Count;
                }

                return 0;
            }

            public int Get(string columnName, string cell)
            {
                var column = GetColumn(columnName);
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
