using o = Octree;
using Elements.Geometry;
using System.Collections.Generic;

namespace Elements.Search
{
    /// <summary>
    /// A Dynamic Octree for storing any objects that can be described as a single point. This is a thin wrapper around the PointOctree class from NetOctree (https://github.com/mcserep/NetOctree).
    /// </summary>
    /// <remarks>
    /// Octree:	An octree is a tree data structure which divides 3D space into smaller partitions (nodes) 
    /// and places objects into the appropriate nodes. This allows fast access to objects
    /// in an area of interest without having to check every object.
    /// 
    /// Dynamic: The octree grows or shrinks as required when objects as added or removed.
    /// It also splits and merges nodes as appropriate. There is no maximum depth.
    /// </remarks>
    /// <typeparam name="T">The content of the octree can be anything, since the bounds data is supplied separately.</typeparam>
    public class PointOctree<T>
    {
        private readonly o.PointOctree<T> _octree;

        /// <summary>
		/// Constructor for the point octree.
		/// </summary>
		/// <param name="initialWorldSize">Size of the sides of the initial node. The octree will never shrink smaller than this.</param>
		/// <param name="initialWorldPos">Position of the center of the initial node.</param>
		/// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this.</param>
        public PointOctree(double initialWorldSize, Vector3 initialWorldPos, double minNodeSize)
        {
            _octree = new o.PointOctree<T>((float)initialWorldSize, initialWorldPos.ToOctreePoint(), (float)minNodeSize);
        }

        /// <summary>
        /// Returns all objects in the tree.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <returns>All objects.</returns>
        public ICollection<T> GetAll()
        {
            return _octree.GetAll();
        }

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objPos">Position of the object.</param>
        public void Add(T obj, Vector3 objPos)
        {
            _octree.Add(obj, objPos.ToOctreePoint());
        }

        /// <summary>
        /// Returns objects that are within <paramref name="maxDistance"/> of the specified ray.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <returns>Objects within range.</returns>
        public T[] GetNearby(Ray ray, double maxDistance)
        {
            return _octree.GetNearby(ray.ToOctreeRay(), (float)maxDistance);
        }

        /// <summary>
        /// Returns objects that are within <paramref name="maxDistance"/> of the specified position.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="position">The position. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the position to consider.</param>
        /// <returns>Objects within range.</returns>
        public T[] GetNearby(Vector3 position, double maxDistance)
        {
            return _octree.GetNearby(position.ToOctreePoint(), (float)maxDistance);
        }

        /// <summary>
        /// The total amount of objects currently in the tree
        /// </summary>
        public int Count
        {
            get
            {
                return _octree.Count;
            }
        }

        /// <summary>
        /// Gets the bounding box that represents the whole octree
        /// </summary>
        /// <value>The bounding box of the root node.</value>
        public BBox3 MaxBounds
        {
            get
            {
                return _octree.MaxBounds.ToBbox3();
            }
        }

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj)
        {
            return _octree.Remove(obj);
        }

        /// <summary>
        /// Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <param name="objPos">Position of the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj, Vector3 objPos)
        {
            return _octree.Remove(obj, objPos.ToOctreePoint());
        }
    }

    internal static class OctreeExtensions
    {
        internal static o.Ray ToOctreeRay(this Ray ray)
        {
            return new o.Ray(ray.Origin.ToOctreePoint(), ray.Direction.ToOctreePoint());
        }

        internal static o.Point ToOctreePoint(this Vector3 point)
        {
            return new o.Point((float)point.X, (float)point.Y, (float)point.Z);
        }

        internal static BBox3 ToBbox3(this o.BoundingBox bbox)
        {
            return new BBox3(bbox.Min.ToVector3(), bbox.Max.ToVector3());
        }

        internal static Vector3 ToVector3(this o.Point p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }
    }

}