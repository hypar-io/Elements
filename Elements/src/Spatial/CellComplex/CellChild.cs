using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// Base class for all children of Cell
    /// </summary>
    public abstract class CellChild<GeometryType>
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
            CellChild<GeometryType> other = obj as CellChild<GeometryType>;
            if (other == null) return false;
            return this.Id == other.Id;
        }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cellComplex"></param>
        internal CellChild(long id, CellComplex cellComplex = null)
        {
            this.Id = id;
            this.CellComplex = cellComplex;
        }

        /// <summary>
        /// Get the associated geometry for this child
        /// </summary>
        /// <returns></returns>
        public abstract GeometryType GetGeometry();

        /// <summary>
        /// Get the distance from a point to the geometry representing this child
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public abstract double DistanceTo(Vector3 point);
    }
}