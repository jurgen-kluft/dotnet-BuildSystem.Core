using System;
using GameData;

namespace DataBuildSystem
{
    public class BigfileCustomConfigWii : IBigfileConfig
    {
        public string Platform { get { return "WII"; } }
        public string BigfileName { get { return "MJ"; } }
        public string BigFileExtension { get { return ".gda"; } }
        public string BigFileTocExtension { get { return ".gdt"; } }
        public string BigFileFdbExtension { get { return ".gdf"; } }
		public string BigFileHdbExtension { get { return ".bfh"; } }
		public string BigFileNodeExtension { get { return ".bfn"; } }
        public bool LittleEndian { get { return false; } }
        public Int64 FileAlignment { get { return 2048; } }
        public bool AllowDuplicateFiles { get { return false; } }
        public bool WriteAsync { get { return true; } }
        public UInt32 ReadBufferSize { get { return 64 * 1024; } }
        public UInt32 WriteBufferSize { get { return 48 * 1024 * 1024; } }
    }
	
    public class BuildSystemCompilerCustomConfigWii : IBuildSystemCompilerConfig
    {
        public string Platform { get { return "WII"; } }
        public string DataFilename(string name) { return name; }
        public string DataFileExtension { get { return ".rdf"; } }
        public string DataRelocFileExtension { get { return ".raf"; } }
		public bool LittleEndian { get { return false; } }
        public EGenericFormat DataFormat { get { return EGenericFormat.STD_FLAT; } }
		public bool EnumIsInt32 { get { return true; } }
		public int SizeOfBool { get { return 4; } }
        public bool ForceBuildingBigfile { get { return false; } }
    }

    public class BuildSystemLocalizerConfigWii : IBuildSystemLocalizerConfig
    {
        public string Platform { get { return "WII"; } }
        public bool LittleEndian { get { return false; } }
        public bool Unicode { get { return false; } }
        public string SubDepFileExtension { get { return ".sdep"; } }
        public string MainDepFileExtension { get { return ".dep"; } }
        public string SubLocFileExtension { get { return ".sloc"; } }
        public string MainLocFileExtension { get { return ".loc"; } }
    }
}
