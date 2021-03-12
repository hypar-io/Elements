using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// Base class for all children of Cell
    /// </summary>
    public abstract class CellChild
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id;

        /// <summary>
        /// The CellComplex that this child belongs to
        /// </summary>
        [JsonIgnore]
        public CellComplex CellComplex { get; internal set; }

        /// <summary>
        /// Used for HashSets
        /// </summary>
        public override int GetHashCode()
        {
            return (int)this.Id;
        }

        /// <summary>
        /// Used for HashSets
        /// </summary>
        public override bool Equals(object obj)
        {
            CellChild other = obj as CellChild;
            if (other == null) return false;
            return this.Id == other.Id;
        }

        internal CellChild(long id, CellComplex cellComplex = null)
        {
            this.Id = id;
            this.CellComplex = cellComplex;
        }
    }
}