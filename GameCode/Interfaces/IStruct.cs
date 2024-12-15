using System;

namespace GameData
{
    /// These pre-defined struct(s) already exist in the target code-base.
    /// So we only need to know some binary properties and a function to
    /// write it out to a binary stream.
    /// As for C++ code, we only need to know the name of the struct/type and this is taken
    /// from the declared struct and can be overriden by using an attribute.
    public interface IStruct
    {
        bool StructIsTemplate { get; } // This is true if the struct is a template
        string StructTemplateType { get; } // This is the templated type of the struct

        int StructAlign { get; } // This is the required memory alignment of the struct
        int StructSize { get; } // This is the memory size of the struct
        string StructName { get; } // This is the name of the struct in the target code-base

        void StructWrite(IGameDataWriter writer);
    }
}