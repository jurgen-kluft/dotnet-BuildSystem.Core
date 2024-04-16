using System;
using System.Collections.Generic;

namespace GameData
{
    using GameCore;
    using MetaCode;

    /// <summary>
    /// StdStream representing a standard object data tree
    /// </summary>
    public sealed class StdDataStream
    {
    }

    public sealed class GenericTypeInformation : ITypeInformation
    {
        #region Private Methods

        public bool IsNull(Type t)
        {
            return (t == null);
        }

        public bool IsBool(Type t)
        {
            return t == typeof(bool);
        }

        public bool IsInt8(Type t)
        {
            return t == typeof(SByte);
        }

        public bool IsUInt8(Type t)
        {
            return t == typeof(Byte);
        }

        public bool IsInt16(Type t)
        {
            return t == typeof(Int16);
        }

        public bool IsUInt16(Type t)
        {
            return t == typeof(UInt16);
        }

        public bool IsInt32(Type t)
        {
            return t == typeof(Int32);
        }

        public bool IsUInt32(Type t)
        {
            return t == typeof(UInt32);
        }

        public bool IsInt64(Type t)
        {
            return t == typeof(Int64);
        }

        public bool IsUInt64(Type t)
        {
            return t == typeof(UInt64);
        }

        public bool IsFloat(Type t)
        {
            return t == typeof(float);
        }

        public bool IsDouble(Type t)
        {
            return t == typeof(double);
        }

        public bool IsString(Type t)
        {
            return t == typeof(string);
        }

        public bool IsEnum(Type t)
        {
            return t.IsEnum;
        }

        public bool IsArray(Type t)
        {
            return (!t.IsPrimitive && t.IsArray);
        }

        public bool IsGenericList(Type t)
        {
            return (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>)));
        }

        public bool IsGenericDictionary(Type t)
        {
            return (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Dictionary<,>)));
        }

        public bool IsClass(Type t)
        {
            return (t.IsClass || (t.IsValueType && !t.IsPrimitive && !t.IsEnum)) && (!t.IsArray && !IsString(t));
        }

        public bool IsStruct(Type t)
        {
            return (t.IsValueType && !t.IsPrimitive && !t.IsEnum) && (!t.IsArray && !IsString(t) && (t != typeof(decimal)) && (t != typeof(DateTime)));
        }

        public bool IsFileId(Type t)
        {
            return !t.IsPrimitive && HasGenericInterface(t, typeof(GameData.IFileId));
        }

        public bool IsIStruct(Type t)
        {
            return HasGenericInterface(t, typeof(GameData.IStruct));
        }

        #endregion


        #region Static Methods

        private static bool HasGenericInterface(Type objectType, Type interfaceType)
        {
            var baseTypes = objectType.GetInterfaces();
            foreach (var t in baseTypes)
                if (t == interfaceType)
                    return true;
            return false;
        }

        private static bool HasOrIsGenericInterface(Type objectType, Type interfaceType)
        {
            if (objectType == interfaceType)
                return true;

            var baseTypes = objectType.GetInterfaces();
            foreach (var t in baseTypes)
                if (t == interfaceType)
                    return true;
            return false;
        }

        #endregion
    }
}
