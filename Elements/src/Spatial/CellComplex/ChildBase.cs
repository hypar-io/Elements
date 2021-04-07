using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;
using System;

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

        /// <summary>
        /// A utility to traverse the neighbors of a traversable child.
        /// </summary>
        /// <param name="startChild">Starting child.</param>
        /// <param name="maxCount">The number of traversals after which this will abort. This is a safety measure against infinite loops.</param>
        /// <param name="target">Target to traverse toward.</param>
        /// <param name="completedRadius">If provided, ends the traversal when the neighbor is within this distance to the target point.</param>
        /// <param name="getNextNeighbor">Provide the method by which we will grab the next neighbor in the traversal series.</param>
        /// <typeparam name="T">The derived child class.</typeparam>
        /// <returns>A collection of traversed children, including the starting child.</returns>
        internal static List<T> TraverseNeighbors<T>(T startChild, int maxCount, Vector3 target, double completedRadius, Func<T, T> getNextNeighbor) where T : ChildBase
        {
            var count = 0;
            var neighbors = new List<T>();
            var curNeighbor = startChild;
            while (curNeighbor != null && count <= maxCount)
            {
                neighbors.Add(curNeighbor);
                if (curNeighbor.DistanceTo(target) <= completedRadius)
                {
                    break;
                }
                curNeighbor = getNextNeighbor(curNeighbor);
                count += 1;
            }
            return neighbors;
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