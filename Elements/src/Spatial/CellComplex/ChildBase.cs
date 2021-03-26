using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// An abstract base for ChildBase that does not contain the geometry constraints.
    /// Do not inherit from this directly, always use the geometry constraints.
    /// </summary>
    public abstract class ChildBase
    {
        /// <summary>
        /// ID of this child.
        /// </summary>
        public ulong Id;

        /// <summary>
        /// The CellComplex that this child belongs to.
        /// </summary>
        [JsonIgnore]
        public CellComplex CellComplex { get; internal set; }

        /// <summary>
        /// Used to return a unique identifier for when we make HashSets of children of this type.
        /// </summary>
        public override int GetHashCode()
        {
            return (int)this.Id;
        }

        /// <summary>
        /// Base constructor for a CellComplex child.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cellComplex"></param>
        internal ChildBase(ulong id, CellComplex cellComplex = null)
        {
            this.Id = id;
            this.CellComplex = cellComplex;
        }

        /// <summary>
        /// Get the shortest distance from a point to the geometry representing this child.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public abstract double DistanceTo(Vector3 point);

        /// <summary>
        /// Used to handle comparisons for when we make HashSets of children of this type.
        /// </summary>
        public override bool Equals(object obj)
        {
            ChildBase other = obj as ChildBase;
            if (other == null) return false;
            return this.Id == other.Id;
        }

        /// <summary>
        /// Get the closest candidate from a list of candidates.
        /// </summary>
        /// <param name="candidates">List of available candidates.</param>
        /// <param name="point">Our target point to determine closest distance from.</param>
        /// <typeparam name="T">Return object type.</typeparam>
        /// <returns></returns>
        internal static T GetClosest<T>(List<T> candidates, Vector3 point) where T : ChildBase
        {
            if (candidates.Count == 0)
            {
                return null;
            }
            return candidates.OrderBy(c => c.DistanceTo(point)).ToList()[0];
        }
    }

    /// <summary>
    /// Base class for all children of Cell.
    /// </summary>
    public abstract class ChildBase<GeometryType> : ChildBase
    {
        internal ChildBase(ulong id, CellComplex cellComplex = null) : base(id, cellComplex) { }

        /// <summary>
        /// Get the associated geometry for this child.
        /// </summary>
        /// <returns></returns>
        public abstract GeometryType GetGeometry();
    }
}