using GameCore;
using DataBuildSystem;

namespace GameData
{
    public enum ELanguage : short
    {
        LanguageInvalid = -1,
        LanguageEnglish = 0,
        LanguageChinese = 1,
        LanguageItalian = 2,
        LanguageGerman = 3,
        LanguageDutch = 4,
        LanguageEnglishUs = 5,
        LanguageSpanish = 6,
        LanguageFrenchUs = 7,
        LanguagePortuguese = 8,
        LanguageBrazilian = 9, // Brazilian Portuguese
        LanguageJapanese = 10, //
        LanguageKorean = 11, // Korean
        LanguageRussian = 12, // Russian
        LanguageGreek = 13,
        LanguageChineseT = 14, // Traditional Chinese
        LanguageChineseS = 15, // Simplified Chinese
        LanguageFinnish = 16,
        LanguageSwedish = 17,
        LanguageDanish = 18,
        LanguageNorwegian = 19,
        LanguagePolish = 20,
        LanguageCount,
        LanguageDefault = LanguageEnglish
    };

    public sealed class Languages
    {
        public ELanguage DefaultLanguage = ELanguage.LanguageDefault;
        public LanguageDataFile[] LanguageArray = new LanguageDataFile[(int)ELanguage.LanguageCount];
    }

    public sealed class LocalizationDataFile : IDataFile, ISignature
    {
        private string _srcFilename;
        private readonly List<string> _srcFilenames;
        private readonly List<string> _dstFilenames;
        private readonly LanguageDataFile[] _languageDataFiles;
        private Dependency _dependency;

        public LocalizationDataFile() : this("Localization.loc")
        {
        }
        public LocalizationDataFile(string localizationFile)
        {
            _srcFilename = Path.ChangeExtension(localizationFile, ".loc") + ".ids" + ".lst";
            _srcFilenames = new List<string>();
            _dstFilenames = new List<string>((int)ELanguage.LanguageCount);
            _languageDataFiles= new LanguageDataFile[(int)ELanguage.LanguageCount];
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IBinaryWriter stream)
        {
            stream.Write("LocalizationCompiler");
            stream.Write(_srcFilename);
        }

        public void SaveState(IBinaryWriter stream)
        {
            stream.Write(_srcFilename);

            stream.Write(_srcFilenames.Count);
            foreach (var filename in _srcFilenames)
            {
                stream.Write(filename);
            }

            stream.Write(_dstFilenames.Count);
            foreach (var filename in _dstFilenames)
            {
                stream.Write(filename);
            }

            _dependency.WriteTo(stream);
        }

        public void LoadState(IBinaryReader stream)
        {
            _srcFilename = stream.ReadString();

            var srcCount = stream.ReadInt32();
            _srcFilenames.Clear();
            _srcFilenames.Capacity = srcCount;
            for (var i = 0; i < srcCount; i++)
            {
                _srcFilenames.Add(stream.ReadString());
            }

            var dstCount = stream.ReadInt32();
            _dstFilenames.Clear();
            _dstFilenames.Capacity = dstCount;
            for (var i = 0; i < dstCount; i++)
            {
                _dstFilenames.Add(stream.ReadString());
            }

            _dependency = Dependency.ReadFrom(stream);
        }

        public void CopyConstruct(IDataFile dc)
        {
            if (dc is LocalizationDataFile lc)
            {
                _srcFilename = lc._srcFilename;
                _srcFilenames.Clear();
                _srcFilenames.AddRange(lc._srcFilenames);
                _dstFilenames.Clear();
                _dstFilenames.AddRange(lc._dstFilenames);
                _dependency = lc._dependency;
            }
        }

        public string CookedFilename => string.Empty;
        public object CookedObject => new Languages() { LanguageArray = _languageDataFiles};

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            DataCookResult result;
            if (_dependency == null)
            {
                _dependency = new Dependency(EGameDataPath.GameDataSrcPath, _srcFilename);
                // Load 'languages list' file
                try
                {
                    var reader = TextStream.OpenForRead(_srcFilename);
                    while (!reader.EndOfStream)
                    {
                        var filename = reader.ReadLine();
                        if (string.IsNullOrEmpty(filename))
                        {
                            // Decode language from filename
                            Enum.TryParse(Path.GetFileNameWithoutExtension(filename), out ELanguage lang);
                            _dstFilenames.Add(filename);
                            _languageDataFiles[(int)lang] = new LanguageDataFile(filename);
                        }
                    }
                    reader.Close();
                }
                catch (Exception)
                {
                    result = DataCookResult.Error;
                }
                finally
                {
                    result = DataCookResult.DstMissing;
                }
            }
            else
            {
                result = _dependency.Update(delegate (ushort id, State state)
                {
                    var result2 = DataCookResult.UpToDate;
                    if (state == State.Missing)
                    {
                        switch (id)
                        {
                            case 0: result2 = DataCookResult.SrcMissing; break;
                            default: result2 = DataCookResult.DstMissing; break;
                        }
                    }
                    else if (state == State.Modified)
                    {
                        switch (id)
                        {
                            case 0: result2 = DataCookResult.SrcChanged; break;
                            default: result2 = DataCookResult.DstChanged; break;
                        }
                    }

                    return result2;
                });
            }

            return result;
        }
    }

    public sealed class LanguageDataFile : IDataFile, ISignature
    {
        private string _srcFilename;
        private string _dstFilename;
        private Dependency _dependency;

        public LanguageDataFile()
        {
            _srcFilename = string.Empty;
            _dstFilename = string.Empty;
        }

        public LanguageDataFile(string localizationFile)
        {
            _srcFilename = localizationFile;
            _dstFilename = localizationFile;
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IBinaryWriter stream)
        {
            stream.Write("LanguageCompiler");
            stream.Write(_srcFilename);
        }

        public void SaveState(IBinaryWriter stream)
        {
            stream.Write(_srcFilename);
            stream.Write(_dstFilename);
            _dependency.WriteTo(stream);
        }

        public void LoadState(IBinaryReader stream)
        {
            _srcFilename = stream.ReadString();
            _dstFilename = stream.ReadString();
            _dependency = Dependency.ReadFrom(stream);
        }

        public void CopyConstruct(IDataFile dc)
        {
            if (dc is LanguageDataFile lc)
            {
                _srcFilename = lc._srcFilename;
                _dstFilename = lc._dstFilename;
                _dependency = lc._dependency;
            }
        }

        public string CookedFilename => _dstFilename;
        public object CookedObject => new DataFile(this, "strtable_t");

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            DataCookResult result = DataCookResult.None;
            return result;
        }
    }
}
