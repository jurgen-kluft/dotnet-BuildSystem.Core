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
        bool StructIsValueType { get; } // Is this struct a value type or a reference type (pointer)?
        int StructAlign { get; }
        int StructSize { get; }
        string StructName { get; }

        void StructWrite(GameCore.IBinaryWriter writer);
    }
}
