using System;

namespace GameData
{
    /// These pre-defined struct(s) already exist in the target code-base.
    /// So we only need to know some binary properties and a function to
    /// write it out to a binary stream.
    /// As for C++ code, we only need to know the name of the struct/type.
    public interface IStruct
    {
        int StructSize { get; }
        int StructAlign { get; }
        string StructName { get; }
        void StructWrite(IBinaryWriter writer);
    }
}