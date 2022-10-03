using System;
using System.Reflection;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public sealed class DataUnit : IDataUnit, IDynamicMember
    {
        public string mMemberName;
        public string mDataUnitPath;
        public EDataUnit mDataUnit;

        public DataUnit(string membername, string unitpath) : this(membername, unitpath, EDataUnit.External)
        {
        }

        public DataUnit(string membername, string unitpath, EDataUnit dataUnit)
        {
            mMemberName = membername;
            mDataUnitPath = unitpath;
            UnitType = dataUnit;
        }

        public EDataUnit UnitType { get; set; }

        public object extobject
        {
            get
            {
                return null;
            }
        }

        public string name { get { return mMemberName; } }
        public object value { get; set; }
    }
}

