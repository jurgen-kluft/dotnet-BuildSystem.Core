using GameCore;
using BigfileBuilder;
using DataBuildSystem;

namespace GameData
{
    public sealed class BigfileConfigPc : IBigfileConfig
    {
        public EPlatform Platform { get { return EPlatform.Win64; } }
        public string BigfileName { get { return "TestGame"; } }
        public string BigFileExtension { get { return ".bfd"; } }
        public string BigFileTocExtension { get { return ".bft"; } }
        public string BigFileFdbExtension { get { return ".bff"; } }
        public string BigFileHdbExtension { get { return ".bfh"; } }
        public uint FileAlignment { get { return 256; } }
        public bool AllowDuplicateFiles { get { return false; } }
        public uint ReadBufferSize { get { return 8 * 1024 * 1024; } }
        public uint WriteBufferSize { get { return 48 * 1024 * 1024; } }
    }

    public sealed class BuildSystemConfigPc : IBuildSystemConfig
    {
        public string Name =>  "TestGame";
        public EPlatform Platform => EPlatform.Win64;
        public bool LittleEndian => true;
        public bool EnumIsInt32 => true;
        public int SizeOfBool => 4;
    }

    public sealed class LocalizerConfigPc : ILocalizerConfig
    {
        public EPlatform Platform => EPlatform.Win64;
        public bool LittleEndian => true;
        public bool Unicode => false;
        public string SubDepFileExtension => ".sdep";
        public string MainDepFileExtension => ".dep";
        public string SubLocFileExtension => ".sloc";
        public string MainLocFileExtension => ".loc";
    }
}
