using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class ModelData
    {
        public DataFile StaticMesh;
        public DataFile[] Textures;

        public ModelData(DataFile staticMesh, DataFile[] textures)
        {
            StaticMesh = staticMesh;
            Textures = textures;
        }
    }

    // e.g. new FileId(new ModelCompiler("Models/Teapot.glTF"));
    public sealed class ModelCompiler : IDataFile, ISignature
    {
        private string _srcFilename;
        private string _dstFilename;
        private readonly DataFile[] _textures;
        private Dependency _dependency;

        public ModelCompiler(string filename) : this(filename, filename)
        {
        }

        private ModelCompiler(string srcFilename, string dstFilename)
        {
            _srcFilename = srcFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            _dstFilename = dstFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            _textures = [];
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IBinaryWriter stream)
        {
            stream.Write("ModelCompiler");
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
            if (dc is not ModelCompiler cc) return;

            _srcFilename = cc._srcFilename;
            _dstFilename = cc._dstFilename;
            _dependency = cc._dependency;
        }

        public string CookedFilename => _dstFilename;
        public object CookedObject
        {
            get
            {
                return new ModelData(new DataFile(this, "staticmesh_t"), _textures);
            }
        }

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            var result = DataCookResult.None;
            if (_dependency == null)
            {
                _dependency = new Dependency(EGameDataPath.Src, _srcFilename);
                _dependency.Add(1, EGameDataPath.Dst, _dstFilename);
                result = DataCookResult.DstMissing;
            }
            else
            {
                var result3 = _dependency.Update(delegate(short id, State state)
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
                File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, _srcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, _dstFilename), true);

                // Get the texture, material datafiles

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
