using System;
using GameCore;

namespace GameData
{
    public interface IFileRegistrar
    {
        FileId Add(string filename);
    }
}
