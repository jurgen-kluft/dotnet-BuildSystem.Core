using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameCore
{
    [DebuggerDisplay("ID: {Id}")]
    public readonly struct StreamReference
    {
        #region Fields

        public static readonly StreamReference Empty = new StreamReference(0);

        #endregion
        #region Constructor

        private StreamReference(uint id)
        {
            Id = id;
        }

        #endregion
        #region Properties

        private static uint UniqueId { get; set; } = 0;
        public static StreamReference NewReference => new() { Id = ++UniqueId };

        public uint Id
        {
            get;
            private init;
        }

        #endregion
        #region Operators

        public static bool operator ==(StreamReference a, StreamReference b)
        {
            return a.Id == b.Id;
        }
        public static bool operator !=(StreamReference a, StreamReference b)
        {
            return a.Id != b.Id;
        }

        #endregion
        #region Comparer (IEqualityComparer)

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

        #endregion
        #region Methods

        public override bool Equals(object obj)
        {
            return obj != null && ((StreamReference)obj).Id == Id;
        }

        public override int GetHashCode()
        {
            return (int)Id;
        }

        #endregion
    }
}
