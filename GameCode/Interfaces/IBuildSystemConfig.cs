using GameCore;

namespace DataBuildSystem
{
    public interface IBuildSystemConfig
    {
        /// <summary>
        /// The name used for filenames
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The platform this configuration is for
        /// </summary>
        EPlatform Platform { get; }

        /// <summary>
        /// Write the BigfileToc and Resource data in which endian
        /// </summary>
        bool LittleEndian { get; }

        /// <summary>
        /// Treat every enum as a 32 bit integer
        /// </summary>
        bool EnumIsInt32 { get; }

        /// <summary>
        /// Treat every bool as a n byte value (1, 2 or 4)
        /// </summary>
        int SizeOfBool { get; }
    }

}
