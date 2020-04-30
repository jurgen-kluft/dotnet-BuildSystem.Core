using System;
using Core;

namespace Game.Data
{
    public class RootObject : IDynamicMember
    {
        private readonly string mMemberName;
        private IExternalObjectProvider mProvider = null;

        public RootObject(string membername, string filename)
        {
            mMemberName = membername;
            Filename assemblyFilename = new Filename(filename);
            assemblyFilename.ChangeExtension(".dll");
            mProvider = new RootObjectNodeCompiler(assemblyFilename, new Filename(filename));
        }

        public string name { get { return mMemberName; } }
        public object value { get { return mProvider.extobject; } }
    }
}
