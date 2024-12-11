using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class CookedLanguageDataFile : IDataFile
    {
        private string mDstFilename;

        public CookedLanguageDataFile(string cookedLocalizationFile)
        {
            mDstFilename = cookedLocalizationFile;
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IBinaryWriter stream)
        {
            stream.Write("CookedLanguageDataFile");
            stream.Write(mDstFilename);
        }

        public void SaveState(IBinaryWriter stream)
        {
            stream.Write(mDstFilename);
        }

        public void LoadState(IBinaryReader stream)
        {
            mDstFilename = stream.ReadString();
        }

        public void CopyConstruct(IDataFile dc)
        {
            if (dc is CookedLanguageDataFile lc)
            {
                mDstFilename = lc.mDstFilename;
            }
        }

        public string CookedFilename => mDstFilename;

        public object CookedObject => new DataFile(Signature, "language_t");

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            return DataCookResult.UpToDate;
        }
    }

    public sealed class GameLanguage
    {
        public string m_language;
        public IDataFile m_data;

        public GameLanguage(string language, CookedLanguageDataFile data)
        {
            m_language = language;
            m_data = data;
        }
    }

    public sealed class GameLanguages
    {
        public GameLanguage m_default;
        public List<GameLanguage> m_languages = new List<GameLanguage>();
    }

    public sealed class LocalizationCompiler : IDataFile
    {
        private string mSrcFilename;
        private List<string> mDstFilenames;
        private Dependency mDependency;

        public LocalizationCompiler(string localizationFile)
        {
            mSrcFilename = Path.ChangeExtension(localizationFile, ".loc") + ".ids" + ".lst";
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IBinaryWriter stream)
        {
            stream.Write("LocalizationCompiler");
            stream.Write(mSrcFilename);
        }

        public void SaveState(IBinaryWriter stream)
        {
            stream.Write(mSrcFilename);
            stream.Write(mDstFilenames.Count);
            foreach (var filename in mDstFilenames)
            {
                stream.Write(filename);
            }
            mDependency.WriteTo(stream);
        }

        public void LoadState(IBinaryReader stream)
        {
            mSrcFilename = stream.ReadString();
            var count = stream.ReadInt32();
            mDstFilenames = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                mDstFilenames.Add(stream.ReadString());
            }

            mDependency = Dependency.ReadFrom(stream);
        }

        public void CopyConstruct(IDataFile dc)
        {
            if (dc is LocalizationCompiler lc)
            {
                mSrcFilename = lc.mSrcFilename;
                mDstFilenames = lc.mDstFilenames;
                mDependency = lc.mDependency;
            }
        }

        public string CookedFilename => mDstFilenames[0];

        public object CookedObject
        {
            get
            {
                var languages = new GameLanguages();
                languages.m_default = new GameLanguage("en", new CookedLanguageDataFile(mDstFilenames[0]));
                foreach (var filename in mDstFilenames)
                {
                    var language = Path.GetFileNameWithoutExtension(filename);
                    languages.m_languages.Add(new GameLanguage(language, new CookedLanguageDataFile(filename)));
                }
                return languages;
            }
        }

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            DataCookResult result;
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.Src, mSrcFilename);
                // Load 'languages list' file
                try
                {
                    var reader = TextStream.OpenForRead(mSrcFilename);
                    while (!reader.EndOfStream)
                    {
                        var filename = reader.ReadLine();
                        if (string.IsNullOrEmpty(filename))
                        {
                            mDstFilenames.Add(filename);
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
                result = mDependency.Update(delegate (short id, State state)
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
}
