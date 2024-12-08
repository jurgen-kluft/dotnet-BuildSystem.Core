using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameCore
{
    [DebuggerDisplay("StreamReference: {Id}")]
    public readonly struct StreamReference
    {
        public static readonly StreamReference Empty = new StreamReference(0);

        private static uint UniqueId { get; set; } = 0;
        public static StreamReference NewReference => new() { Id = ++UniqueId };

        private StreamReference(uint id)
        {
            Id = id;
        }

        public uint Id
        {
            get;
            private init;
        }

        public bool IsEqual(StreamReference other)
        {
            return Id == other.Id;
        }

        public class Comparer : IEqualityComparer<StreamReference>
        {
            public bool Equals(StreamReference lhs, StreamReference rhs)
            {
                return lhs.Id == rhs.Id;
            }

            public int GetHashCode(StreamReference r)
            {
                return (int)r.Id;
            }
        }
    }
}
