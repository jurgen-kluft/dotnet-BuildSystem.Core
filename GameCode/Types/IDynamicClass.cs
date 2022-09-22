using System.Collections.Generic;

namespace GameData
{
    public interface IDynamicClass
    {
        List<IDynamicMember> members { get; }
        void addMember(IDynamicMember member);
    }
}
