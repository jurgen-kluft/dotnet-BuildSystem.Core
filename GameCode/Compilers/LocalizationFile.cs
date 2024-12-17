using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class Language
    {
        public string Name;
        public DataFile Data;

        public Language()
        {
            Name = "en";
            Data = new DataFile(null, "language_t");
        }

        public Language(string language, DataFile data)
        {
            Name = language;
            Data = data;
        }
    }

    public sealed class Languages
    {
        public List<Language> Language_Array = [];
    }

    public sealed class LocalizationDataFile : IDataFile, ISignature
    {
        private string _srcFilename;
        private readonly List<string> _srcFilenames;
        private readonly List<string> _dstFilenames;
        private readonly List<LanguageDataFile> _languageDataFiles;
        private Dependency _dependency;

        public LocalizationDataFile() : this("Localization.loc")
        {
        }
        public LocalizationDataFile(string localizationFile)
        {
            _srcFilename = Path.ChangeExtension(localizationFile, ".loc") + ".ids" + ".lst";
            _srcFilenames = [];
            _dstFilenames = [];
            _languageDataFiles = [];
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

        public object CookedObject
        {
            get
            {
                var languages = new Languages();
                if (_languageDataFiles.Count == 0)
                    return languages;
                foreach (var languageDataFile in _languageDataFiles)
                {
                    languages.Language_Array.Add(new Language(languageDataFile.Language, new DataFile( languageDataFile, "language_t")));
                }
                return languages;
            }
        }

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
                            LanguageDataFile language = new LanguageDataFile(filename);
                            language.Language = Path.GetFileNameWithoutExtension(filename);
                            _dstFilenames.Add(filename);
                            _languageDataFiles.Add(language);
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

        public LanguageDataFile(string localizationFile)
        {
            _srcFilename = localizationFile;
            _dstFilename = localizationFile;
        }

        public Hash160 Signature { get; set; }

        public string Language { get; set; }

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
        public object CookedObject => new DataFile(this, "language_t");

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            DataCookResult result = DataCookResult.None;
            return result;
        }
    }
}
