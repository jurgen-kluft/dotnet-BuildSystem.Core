using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class ShaderFileCooker : IDataFile, ISignature
    {
        private string _srcFilename;
        private string _dstFilename;
        private Dependency _dependency;

        public ShaderFileCooker() : this(string.Empty, string.Empty)
        {
        }
        public ShaderFileCooker(string filename) : this(filename, filename)
        {
        }
        public ShaderFileCooker(string srcFilename, string dstFilename)
        {
            _srcFilename = srcFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            _dstFilename = dstFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IWriter writer)
        {
            GameCore.BinaryWriter.Write(writer,"ShaderFileCooker");
            GameCore.BinaryWriter.Write(writer,_srcFilename);
        }

        public void SaveState(IWriter writer)
        {
            GameCore.BinaryWriter.Write(writer,_srcFilename);
            GameCore.BinaryWriter.Write(writer,_dstFilename);
            _dependency.WriteTo(writer);
        }

        public void LoadState(IBinaryReader reader)
        {
            GameCore.BinaryReader.Read(reader, out _srcFilename);
            GameCore.BinaryReader.Read(reader, out _dstFilename);
            _dependency = Dependency.ReadFrom(reader);
        }

        public void CopyConstruct(IDataFile dc)
        {
            if (dc is not ShaderFileCooker cc) return;

            _srcFilename = cc._srcFilename;
            _dstFilename = cc._dstFilename;
            _dependency = cc._dependency;
        }

        public string CookedFilename => _dstFilename;
        public object CookedObject => new DataFile(this, "shader_t");

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            var result = DataCookResult.None;
            if (_dependency == null)
            {
                _dependency = new Dependency(EGameDataPath.GameDataSrcPath, _srcFilename);
                _dependency.Add(1, EGameDataPath.GameDataDstPath, _dstFilename);
                result = DataCookResult.DstMissing;
            }
            else
            {
                var result3 = _dependency.Update(delegate(ushort id, State state)
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
                File.Copy(Path.Join(BuildSystemConfig.SrcPath, _srcFilename), Path.Join(BuildSystemConfig.DstPath, _dstFilename), true);

                // Note: On Windows we have different tools than on Mac/Linux
                // Note: Shaders are in HLSL and for Mac (Metal) need to be compiled

                // Execution is done, update the dependency to reflect the new state
                result = _dependency.Update(null);
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
