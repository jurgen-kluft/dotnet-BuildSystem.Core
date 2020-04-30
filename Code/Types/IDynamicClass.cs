using System.Collections.Generic;

namespace Game.Data
{
    public interface IDynamicClass
    {
        List<IDynamicMember> members { get; }
        void addMember(IDynamicMember member);
    }
}
