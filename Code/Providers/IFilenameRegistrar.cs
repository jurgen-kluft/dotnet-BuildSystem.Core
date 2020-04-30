using System;
using Core;

namespace Game.Data
{
    public interface IFileRegistrar
    {
        FileId Add(Filename filename);
    }
}
