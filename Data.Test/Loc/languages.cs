using System.Collections.Generic;

namespace BuildTools.Data
{
    public class Languages
    {
        public FileIdList FileId = new FileIdList(new LocalizationCompiler("Loc\\Localization.dat"));
    }
}