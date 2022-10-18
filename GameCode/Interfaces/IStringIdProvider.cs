using System;
using GameCore;

namespace GameData
{
    public interface ILStringIdProvider
    {
        /// <summary>
        /// The LString Id is set by and outside process and this id is used to connect to a localization
        /// string that exists in the Localization Database.
        /// </summary>
        string LStringText { get; }
        Int64 LStringId { set; }
    }
}
