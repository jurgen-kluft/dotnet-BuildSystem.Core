using System.Collections.Generic;

using GameCore;
using DataBuildSystem;
using Environment = System.Environment;

namespace GameData
{
    public class TextureDataFile : IStruct, ISignature
    {
        private readonly ISignature _signature;

        public TextureDataFile() : this(new NullSignature())
        {
        }

        public TextureDataFile(ISignature signature)
        {
            _signature = signature;
            StructMember = "datafile_t<texture_t>";
        }

        public Hash160 Signature { get { return _signature.Signature; } }

        public int StructAlign => 4; // This is the required memory alignment of the struct
        public int StructSize => 8; // This is the memory size of the struct
        public string StructMember { get; set; }

        public ICode StructCode => new NullCode();

        public void StructWrite(IGameDataWriter writer)
        {
            writer.WriteFileId(Signature);
        }
    }


    public sealed class TextureFileCooker : IDataFile, ISignature
    {
        private string _srcFilename;
        private string _dstFilename;
        private Dependency _dependency;

        public TextureFileCooker() : this(string.Empty, string.Empty)
        {
        }
        public TextureFileCooker(string filename) : this(filename, filename)
        {
        }
        public TextureFileCooker(string srcFilename, string dstFilename)
        {
            _srcFilename = srcFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            _dstFilename = dstFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IWriter stream)
        {
            GameCore.BinaryWriter.Write(stream,"TextureFileCooker");
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
            if (dc is not TextureFileCooker cc) return;

            _srcFilename = cc._srcFilename;
            _dstFilename = cc._dstFilename;
            _dependency = cc._dependency;
        }

        public string CookedFilename => _dstFilename;
        public object CookedObject => new TextureDataFile(this);

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
