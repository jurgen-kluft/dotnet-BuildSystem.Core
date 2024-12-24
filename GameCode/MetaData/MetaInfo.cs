namespace GameData
{
    namespace MetaCode
    {
        public readonly struct MetaInfo
        {
            public static readonly MetaInfo s_unknown = new() { Index = 0 };
            public static readonly MetaInfo s_bool = new() { Index = 1 };
            public static readonly MetaInfo s_bitset = new() { Index = 2 };
            public static readonly MetaInfo s_int8 = new() { Index = 3 };
            public static readonly MetaInfo s_uint8 = new() { Index = 4 };
            public static readonly MetaInfo s_int16 = new() { Index = 5 };
            public static readonly MetaInfo s_uint16 = new() { Index = 6 };
            public static readonly MetaInfo s_int32 = new() { Index = 7 };
            public static readonly MetaInfo s_uint32 = new() { Index = 8 };
            public static readonly MetaInfo s_int64 = new() { Index = 9 };
            public static readonly MetaInfo s_uint64 = new() { Index = 10 };
            public static readonly MetaInfo s_float = new() { Index = 11 };
            public static readonly MetaInfo s_double = new() { Index = 12 };
            public static readonly MetaInfo s_string = new() { Index = 13 };
            public static readonly MetaInfo s_enum = new() { Index = 14 };
            public static readonly MetaInfo s_struct = new() { Index = 15 };
            public static readonly MetaInfo s_class = new() { Index = 16 };
            public static readonly MetaInfo s_array = new() { Index = 17 };
            public static readonly MetaInfo s_dictionary = new() { Index = 18 };
            public static readonly MetaInfo s_dataUnit = new() { Index = 19 };

            public const int Count = 20;

            public bool IsBool => Index == 1;
            public bool IsEnum => Index == 14;
            public bool IsStruct => Index == 15;
            public bool IsClass => Index == 16;
            public bool IsArray => Index == 17;
            public bool IsDictionary => Index == 18;
            public bool IsDataUnit => Index == 19;

            private readonly struct Details
            {
                internal byte SizeInBits { get; init; }
                internal byte AlignmentInBytes { get; init; }
                internal bool IsSignedType { get; init; }
                internal string TypeName { get; init; }
            };

            private static readonly Details[] s_details = new Details[Count]
            {
                new() { SizeInBits = 0, AlignmentInBytes = 1, IsSignedType = false, TypeName = "unknown" }, // Unknown
                new() { SizeInBits = 1, AlignmentInBytes = 1, IsSignedType = false, TypeName = "bool" }, // Bool
                new() { SizeInBits = 8, AlignmentInBytes = 1, IsSignedType = false, TypeName = "u8" }, // BitSet
                new() { SizeInBits = 8, AlignmentInBytes = 1, IsSignedType = true, TypeName = "s8" }, // Int8
                new() { SizeInBits = 8, AlignmentInBytes = 1, IsSignedType = false, TypeName = "u8" }, // UInt8
                new() { SizeInBits = 16, AlignmentInBytes = 2, IsSignedType = true, TypeName = "s16" }, // Int16
                new() { SizeInBits = 16, AlignmentInBytes = 2, IsSignedType = false, TypeName = "u16" }, // UInt16
                new() { SizeInBits = 32, AlignmentInBytes = 4, IsSignedType = true, TypeName = "s32" }, // Int32
                new() { SizeInBits = 32, AlignmentInBytes = 4, IsSignedType = false, TypeName = "u32" }, // UInt32
                new() { SizeInBits = 64, AlignmentInBytes = 8, IsSignedType = true, TypeName = "s64" }, // Int64
                new() { SizeInBits = 64, AlignmentInBytes = 8, IsSignedType = false, TypeName = "u64" }, // UInt64
                new() { SizeInBits = 32, AlignmentInBytes = 4, IsSignedType = true, TypeName = "f32" }, // Float
                new() { SizeInBits = 64, AlignmentInBytes = 8, IsSignedType = true, TypeName = "f64" }, // Double
                new() { SizeInBits = 64 + 32 + 32, AlignmentInBytes = 16, IsSignedType = false, TypeName = "string_t" }, // String
                new() { SizeInBits = 0, AlignmentInBytes = 0, IsSignedType = false, TypeName = "enum_t" }, // Enum
                new() { SizeInBits = 0, AlignmentInBytes = 0, IsSignedType = false, TypeName = "struct" }, // Struct
                new() { SizeInBits = 64, AlignmentInBytes = 8, IsSignedType = false, TypeName = "class" }, // Class
                new() { SizeInBits = 64 + 32 + 32, AlignmentInBytes = 16, IsSignedType = false, TypeName = "array_t" }, // Array
                new() { SizeInBits = 64 + 32 + 32, AlignmentInBytes = 16, IsSignedType = false, TypeName = "dict_t" }, // Dictionary
                new() { SizeInBits = 32 + 32, AlignmentInBytes = 4, IsSignedType = false, TypeName = "dataunit_t" }, // DataUnit
            };

            public byte Index { get; private init; }

            public int SizeInBits => s_details[Index].SizeInBits;
            public int SizeInBytes => (s_details[Index].SizeInBits + 7) >> 3;
            public bool IsSigned => s_details[Index].IsSignedType;
            public int Alignment => s_details[Index].AlignmentInBytes;
            public string NameOfType => s_details[Index].TypeName;
        }
    }
}
