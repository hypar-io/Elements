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
    public abstract class ChildBase< GeometryType>
    {
        /// <summary>
        /// ID
        /// </summary>
        public ulong Id;

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
            ChildBase<GeometryType> other = obj as ChildBase<GeometryType>;
            if (other == null) return false;
            return this.Id == other.Id;
        }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cellComplex"></param>
        internal ChildBase(ulong id, CellComplex cellComplex = null)
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

        /// <summary>
        /// Get the closest candidate from a list of candidates
        /// </summary>
        /// <param name="candidates">List of available candidates</param>
        /// <param name="point">Point to get the closest to</param>
        /// <typeparam name="T">Return object type</typeparam>
        /// <returns></returns>
        internal static T GetClosest<T>(List<T> candidates, Vector3 point) where T: ChildBase<GeometryType>
        {
            if (candidates.Count == 0)
            {
                return null;
            }
            return candidates.OrderBy(c => c.DistanceTo(point)).ToList()[0];
        }
    }
}