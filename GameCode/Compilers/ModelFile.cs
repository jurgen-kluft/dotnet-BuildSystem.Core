using System.Collections.Generic;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class ModelData
    {
        public DataFile StaticMesh;
        public List<TextureDataFile> Textures;

        public ModelData(DataFile staticMesh, List<TextureDataFile> textures)
        {
            StaticMesh = staticMesh;
            Textures = textures;
        }
    }

    // e.g. new FileId(new ModelCompiler("Models/Teapot.glTF"));
    public sealed class ModelDataFile : IDataFile, ISignature
    {
        private string _srcFilename;
        private string _dstFilename;
        private readonly TextureFile[] _textures;
        private Dependency _dependency;

        public ModelDataFile() : this(string.Empty, string.Empty)
        {
        }
        public ModelDataFile(string filename) : this(filename, filename)
        {
        }

        private ModelDataFile(string srcFilename, string dstFilename)
        {
            _srcFilename = srcFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            _dstFilename = dstFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            _textures = new TextureFile[0];
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IWriter stream)
        {
            GameCore.BinaryWriter.Write(stream,"ModelCompiler");
            GameCore.BinaryWriter.Write(stream,_srcFilename);
        }

        public void SaveState(IWriter stream)
        {
            GameCore.BinaryWriter.Write(stream,_srcFilename);
            GameCore.BinaryWriter.Write(stream,_dstFilename);
            _dependency.WriteTo(stream);
        }

        public void LoadState(IBinaryReader stream)
        {
            GameCore.BinaryReader.Read(stream, out _srcFilename);
            GameCore.BinaryReader.Read(stream, out _dstFilename);
            _dependency = Dependency.ReadFrom(stream);
        }

        public void CopyConstruct(IDataFile dc)
        {
            if (dc is not ModelDataFile cc) return;

            _srcFilename = cc._srcFilename;
            _dstFilename = cc._dstFilename;
            _dependency = cc._dependency;
        }

        public string CookedFilename => _dstFilename;
        public object CookedObject
        {
            get
            {
                var textures = new List<TextureDataFile>(_textures.Length);
                foreach (var t in _textures)
                {
                    textures.Add(new TextureDataFile(t));
                }

                return new ModelData(new DataFile(this, "staticmesh_t"), textures);
            }
        }

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

                // Generate the texture data files (these textures need to be cooked)

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
