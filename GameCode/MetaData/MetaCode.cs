using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using DataBuildSystem;
using Int8 = System.SByte;
using uint8 = System.Byte;
using GameCore;

namespace GameData
{
    namespace MetaCode
    {
        #region TypeInformation

        public interface ITypeInformation
        {
            bool IsNull(Type t);
            bool IsBool(Type t);
            bool IsInt8(Type t);
            bool IsUInt8(Type t);
            bool IsInt16(Type t);
            bool IsUInt16(Type t);
            bool IsInt32(Type t);
            bool IsUInt32(Type t);
            bool IsInt64(Type t);
            bool IsUInt64(Type t);
            bool IsFloat(Type t);
            bool IsDouble(Type t);
            bool IsString(Type t);
            bool IsEnum(Type t);
            bool IsArray(Type t);
            bool IsGenericList(Type t);
            bool IsGenericDictionary(Type t);
            bool IsClass(Type t);
            bool IsStruct(Type t);
            bool IsIStruct(Type t);
            bool IsFileId(Type t);
        }

        #endregion



        #region MetaType.TypeInfo

        public static class MetaType
        {
            public static bool TypeInfo(Type type, out string name, out int size, out int alignment)
            {
                name = type.Name;
                if (type == typeof(bool))
                {
                    alignment = 4;
                    size = 4;
                    name = "bool_t";
                }
                else if (type == typeof(byte))
                {
                    alignment = 1;
                    size = 1;
                    name = "u8";
                }
                else if (type == typeof(sbyte))
                {
                    alignment = 1;
                    size = 1;
                    name = "s8";
                }
                else if (type == typeof(ushort))
                {
                    alignment = 2;
                    size = 2;
                    name = "u16";
                }
                else if (type == typeof(short))
                {
                    alignment = 2;
                    size = 2;
                    name = "s16";
                }
                else if (type == typeof(uint))
                {
                    alignment = 4;
                    size = 4;
                    name = "u32";
                }
                else if (type == typeof(int))
                {
                    alignment = 4;
                    size = 4;
                    name = "s32";
                }
                else if (type == typeof(ulong))
                {
                    alignment = 8;
                    size = 8;
                    name = "u64";
                }
                else if (type == typeof(long))
                {
                    alignment = 8;
                    size = 8;
                    name = "s64";
                }
                else if (type == typeof(float))
                {
                    alignment = 4;
                    size = 4;
                    name = "f32";
                }
                else if (type == typeof(double))
                {
                    alignment = 8;
                    size = 8;
                    name = "f64";
                }
                else if (type == typeof(string))
                {
                    alignment = 4;
                    size = 0;
                    name = "string_t";
                }
                else if (type == typeof(Enum))
                {
                    alignment = 4;
                    size = 0;
                    name = "enum_t";
                }
                else
                {
                    alignment = 0;
                    size = 0;
                    name = string.Empty;
                    return false;
                }

                return true;
            }
        }

        #endregion
    }
}
