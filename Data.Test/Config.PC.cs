using System;
using GameData;
using DataBuildSystem;

namespace DataBuildSystem
{

    public class BigfileCustomConfigPC : IBigfileConfig
    {
        public EPlatform Platform { get { return EPlatform.Win64; } }
        public string BigfileName { get { return "MJ"; } }
        public string BigFileExtension { get { return ".bfd"; } }
        public string BigFileTocExtension { get { return ".bft"; } }
        public string BigFileFdbExtension { get { return ".bff"; } }
		public string BigFileHdbExtension { get { return ".bfh"; } }
        public Int64 FileAlignment { get { return 1024; } }
        public bool AllowDuplicateFiles { get { return false; } }
        public bool WriteAsync { get { return true; } }
        public UInt32 ReadBufferSize { get { return 8 * 1024 * 1024; } }
        public UInt32 WriteBufferSize { get { return 48 * 1024 * 1024; } }
    }
	
    public class BuildSystemCompilerCustomConfigPC : IBuildSystemCompilerConfig
    {
        public EPlatform Platform { get { return EPlatform.Win64; } }
        public string DataFilename(string name) { return name; }
        public string DataFileExtension { get { return ".gdf"; } }
        public string DataRelocFileExtension { get { return ".gdr"; } }
        public EGenericFormat DataFormat { get { return EGenericFormat.STD_FLAT; } }
		public bool EnumIsInt32 { get { return true; } }
		public int SizeOfBool { get { return 4; } }
        public bool ForceBuildingBigfile { get { return false; } }
    }
	
    public class BuildSystemLocalizerConfigPC : IBuildSystemLocalizerConfig
    {
        public EPlatform Platform { get { return EPlatform.Win64; } }
        public bool Unicode { get { return false; } }
        public string SubDepFileExtension { get { return ".sdep"; } }
        public string MainDepFileExtension { get { return ".dep"; } }
        public string SubLocFileExtension { get { return ".sloc"; } }
        public string MainLocFileExtension { get { return ".loc"; } }
    }	
}
