
namespace GameData
{
    using GameCore;
    using MetaCode;

    public interface ITypeInfo2
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
        bool IsDataUnit(Type t);
    }

    public sealed class TypeInfo2 : ITypeInfo2
    {
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
            return t == typeof(sbyte);
        }

        public bool IsUInt8(Type t)
        {
            return t == typeof(byte);
        }

        public bool IsInt16(Type t)
        {
            return t == typeof(short);
        }

        public bool IsUInt16(Type t)
        {
            return t == typeof(ushort);
        }

        public bool IsInt32(Type t)
        {
            return t == typeof(int);
        }

        public bool IsUInt32(Type t)
        {
            return t == typeof(uint);
        }

        public bool IsInt64(Type t)
        {
            return t == typeof(long);
        }

        public bool IsUInt64(Type t)
        {
            return t == typeof(ulong);
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

        public bool IsIStruct(Type t)
        {
            return HasGenericInterface(t, typeof(GameData.IStruct));
        }

        public bool IsDataUnit(Type t)
        {
            return !t.IsPrimitive && HasGenericInterface(t, typeof(GameData.IDataUnit));
        }

        private static bool HasGenericInterface(Type objectType, Type interfaceType)
        {
            var baseTypes = objectType.GetInterfaces();
            foreach (var t in baseTypes)
                if (t == interfaceType)
                    return true;
            return false;
        }
    }
}
