using System;
using xCore;

namespace DataBuildSystem
{
    public class DependencySystemCustomConfig : IDependencySystemConfig
    {
        public string	Extension				{ get { return ".dep"; } }
        public bool		DateTimeComparison		{ get { return true; } }
        public bool		BinaryComparison		{ get { return false; } }
        public bool		FolderFilter(string folder) { return false; }
        public bool		FileFilter(string folder)	{ return false; }
    }

    public class BigfileCustomConfig : IBigfileConfig
    {
        public string BigfileName { get { return "MJ"; } }
        public string BigFileExtension { get { return ".bfd"; } }
        public string BigFileTocExtension { get { return ".bft"; } }
        public string BigFileFdbExtension { get { return ".bff"; } }
		public string BigFileHdbExtension { get { return ".bfh"; } }
		public string BigFileNodeExtension { get { return ".bfn"; } }
        public bool LittleEndian { get { return true; } }
        public UInt32 FileAlignment { get { return 1024; } }
        public bool AllowDuplicateFiles { get { return false; } }
        public bool WriteAsync { get { return true; } }
        public UInt32 ReadBufferSize { get { return 8 * 1024 * 1024; } }
        public UInt32 WriteBufferSize { get { return 48 * 1024 * 1024; } }
    }
	
    public class BuildSystemCompilerCustomConfig : IBuildSystemCompilerConfig
    {
        public string DataFilename(string name) { return name; }
        public string DataFileExtension { get { return ".gdf"; } }
        public string DataRelocFileExtension { get { return ".gdr"; } }
		public bool LittleEndian { get { return true; } }
        public EGenericFormat DataFormat { get { return EGenericFormat.STD_FLAT; } }
		public bool EnumIsInt32 { get { return true; } }
		public int SizeOfBool { get { return 4; } }
        public bool ForceBuildingBigfile { get { return false; } }
    }
	
    public class BuildSystemLocalizerConfig : IBuildSystemLocalizerConfig
    {
        public bool LittleEndian { get { return true; } }
        public bool Unicode { get { return false; } }
        public string SubDepFileExtension { get { return ".sdep"; } }
        public string MainDepFileExtension { get { return ".dep"; } }
        public string SubLocFileExtension { get { return ".sloc"; } }
        public string MainLocFileExtension { get { return ".loc"; } }
    }	
}