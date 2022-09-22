using System;
using GameCore;

namespace GameData
{
    public class RootObject : IDynamicMember
    {
        private readonly string mMemberName;

        public RootObject(string membername)
        {
            mMemberName = membername;
        }

        public string name { get { return mMemberName; } }
        public object value { get { return null; } }
    }
}
