
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A geometry type representing an oriented cuboid in space.
    /// </summary>
    public class Box
    {
        /// <summary>
        /// The Transform indicating the local coordinate frame of this box. Do
        /// not modify this transform directly to modify the box, instead use TransformBox().
        /// </summary>
        public Transform Transform
        {
            get => transform;
            set
            {
                transform = value;
                _inverseTransform = transform.Inverted();
            }
        }

        /// <summary>
        /// A BBox3 representing the extents of this box, expressed in coordinates relative to the box's transform.
        /// </summary>
        /// <value></value>
        public BBox3 Bounds { get; set; }

        private Transform _inverseTransform;
        private Transform transform;

        /// <summary>
        /// Create a new box from a minimum and maximum point in world
        /// coordinates, and an optional Transform indicating the orientation
        /// frame of the box. 
        /// </summary>
        /// <param name="min">The minimum point of the box, in world coordinates.</param>
        /// <param name="max">The maximum point of the box, in world coordinates.</param>
        /// <param name="transform">If supplied, the transform indicating the box's origin and orientation.</param>
        public Box(Vector3 min, Vector3 max, Transform transform = null)
        {
            Transform = transform ?? new Transform();

            Bounds = new BBox3(_inverseTransform.OfPoint(min), _inverseTransform.OfPoint(max));
        }

        /// <summary>
        /// Create a new box from a bounding box.
        /// </summary>
        /// <param name="box">The world-oriented bounding box.</param>
        /// <param name="transform">If supplied, the transform indicating the box's local coordinate frame.</param>
        public Box(BBox3 box, Transform transform = null)
        {
            Transform = transform ?? new Transform();

            Bounds = box;
        }

        /// <summary>
        /// Make a new box as a copy of the supplied box.
        /// </summary>
        /// <param name="other">The box to copy.</param>
        public Box(Box other)
        {
            Transform = new Transform(other.Transform);
            Bounds = other.Bounds;
        }

        /// <summary>
        /// The volume of the box. Note that this value will be inaccurate if
        /// using a non-euclidean transform.
        /// </summary>
        [JsonIgnore]
        public double Volume => Bounds.Volume;

        /// <summary>
        /// Convert a Box to a set of model curves.
        /// </summary>
        /// <param name="material">An optional material to use for these curves.</param>
        public List<ModelCurve> ToModelCurves(Material material = null)
        {
            return new List<ModelCurve>(Bounds.ToModelCurves(Transform, material));
        }
        /// <summary>
        /// Automatically convert a Bbox3 to a Box.
        /// </summary>
        /// <param name="bbox">A bounding box.</param>
        public static implicit operator Box(BBox3 bbox)
        {
            return new Box(bbox);
        }

        /// <summary>
        /// Transform this box.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public void TransformBox(Transform transform)
        {
            Transform = Transform.Concatenated(transform);
        }

        /// <summary>
        /// Return a new box transformed by the supplied transform.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        /// <returns>A transformed copy of the box.</returns>
        public Box Transformed(Transform transform)
        {
            var boxCopy = new Box(this);
            boxCopy.TransformBox(transform);
            return boxCopy;
        }

    }
}