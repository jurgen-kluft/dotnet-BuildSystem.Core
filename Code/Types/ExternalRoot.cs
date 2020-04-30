using System;

namespace Game.Data
{
    public class ExternalRoot : IDynamicMember
    {
        private ExternalRootNodeCompiler mProvider;
        public ExternalRoot(string name, string foldername)
        {
            mProvider = new ExternalRootNodeCompiler(name, foldername);
        }
        public string name { get { return mProvider.name; } }
        public object value { get { return new FileIdList(mProvider.fileIds); } }
    }
}
