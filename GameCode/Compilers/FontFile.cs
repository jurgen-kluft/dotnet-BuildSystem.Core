using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class FontDataFile : IDataFile, ISignature
    {
        private string mSrcFilename;
        private string mDstFilename;
        private Dependency mDependency;

        public FontDataFile(string filename) : this(filename, filename)
        {
        }

        private FontDataFile(string srcFilename, string dstFilename)
        {
            mSrcFilename = srcFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            mDstFilename = dstFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IBinaryWriter stream)
        {
            stream.Write("FontDataFile");
            stream.Write(mSrcFilename);
        }

        public void SaveState(IBinaryWriter stream)
        {
            stream.Write(mSrcFilename);
            stream.Write(mDstFilename);
            mDependency.WriteTo(stream);
        }

        public void LoadState(IBinaryReader stream)
        {
            mSrcFilename = stream.ReadString();
            mDstFilename = stream.ReadString();
            mDependency = Dependency.ReadFrom(stream);
        }

        public void CopyConstruct(IDataFile dc)
        {
            if (dc is not FontDataFile cc) return;

            mSrcFilename = cc.mSrcFilename;
            mDstFilename = cc.mDstFilename;
            mDependency = cc.mDependency;
        }

        public string CookedFilename => mDstFilename;

        public object CookedObject => new DataFile(this, "font_t");

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            var result = DataCookResult.None;
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.GameDataSrcPath, mSrcFilename);
                mDependency.Add(1, EGameDataPath.GameDataDstPath, mDstFilename);
                result = DataCookResult.DstMissing;
            }
            else
            {
                var result3 = mDependency.Update(delegate(ushort id, State state)
                {
                    var result2 = DataCookResult.None;
                    if (state == State.Missing)
                    {
                        result2 = id switch
                        {
                            0 => (DataCookResult.SrcMissing),
                            1 => (DataCookResult.DstMissing),
                            _ => (DataCookResult.None),
                        };
                    }
                    else if (state == State.Modified)
                    {
                        result2 |= id switch
                        {
                            0 => (DataCookResult.SrcChanged),
                            1 => (DataCookResult.DstChanged),
                            _ => (DataCookResult.None)
                        };
                    }

                    return result2;
                });

                if (result3 == DataCookResult.UpToDate)
                {
                    result = DataCookResult.UpToDate;
                    return result;
                }
            }

            try
            {
                // Execute the actual purpose of this compiler
                File.Copy(Path.Join(BuildSystemConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemConfig.DstPath, mDstFilename), true);

                // Execution is done, update the dependency to reflect the new state
                result = mDependency.Update(null);
            }
            catch (Exception)
            {
                result = (DataCookResult)(result | DataCookResult.Error);
            }

            // The result returned here is the result that 'caused' this compiler to execute its action and not the 'new' state.
            return result;
        }
    }
}
