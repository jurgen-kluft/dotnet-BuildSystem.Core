using System;
using GameCore;

namespace GameData
{
    public interface IResourceIdProvider
    {
        /// <summary>
        /// The ResourceId is set by an outside process and this id is used to connect to an instance
        /// of this interface. With that we can thus connect 'ResourceId' to 'ResourceObject'.
        /// </summary>
        ushort ResourceSectionId { get; set; }
        ushort ResourceTypeId { get; set; }
        uint ResourceObjectId { get; set; }
    }
}
