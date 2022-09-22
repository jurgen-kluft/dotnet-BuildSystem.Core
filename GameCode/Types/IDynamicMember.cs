using System;

namespace GameData
{
    public interface IDynamicMember
    {
        string name { get; }
        object value { get; }
    }

    public class DynamicMember : IDynamicMember
    {
        private string mName = string.Empty;
        private object mValue = null;

        public DynamicMember(string name, object value)
        {
            mName = name;
            mValue = value;
        }

        public string name { get { return mName; } }
        public object value { get { return mValue; } }
    }

}
