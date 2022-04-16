
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Elements.Geometry
{
    /// <summary>
    /// A geometry type representing an oriented cuboid in space.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/BoxTests.cs?name=example)]
    /// </example>
    public class Box
    {
        /// <summary>
        /// The Transform indicating the local coordinate frame of this box. Do
        /// not modify this transform directly to modify the box, instead use TransformBox().
        /// </summary>
        public Transform Transform
        {
            get => _transform;
            set
            {
                _transform = value;
                _inverseTransform = _transform.Inverted();
            }
        }

        /// <summary>
        /// A BBox3 representing the extents of this box, expressed in coordinates relative to the box's transform.
        /// </summary>
        /// <value></value>
        public BBox3 Bounds { get; set; }

        /// <summary>
        /// The (0,0,0) corner of the box, in world coordinates
        /// </summary>
        public Vector3 Min => PointAt(0, 0, 0);

        /// <summary>
        /// The (0,0,0) corner of the box, in world coordinates
        /// </summary>
        public Vector3 Max => PointAt(1, 1, 1);

        private Transform _inverseTransform;
        private Transform _transform;

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
        [JsonConstructor]
        public Box(BBox3 box = default, Transform transform = null)
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
        /// Construct a box from a collection of points and a transform to
        /// specify the box's orientation.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Box(IEnumerable<Vector3> points, Transform transform)
        {
            Transform = transform;
            Bounds = new BBox3(points.Select(p => _inverseTransform.OfPoint(p)));
        }

        /// <summary>
        /// The volume of the box. Note that this value will be inaccurate if
        /// using a non-euclidean transform.
        /// </summary>
        [JsonIgnore]
        public double Volume => Bounds.Volume;

        /// <summary>
        /// Get a point from this bounding box by supplying normalized parameters from 0 to 1.
        /// A point at (0,0,0) will be the minimum point of the box, a
        /// point at (1,1,1) will be the maximum point, and a point at
        /// (0.5,0.5,0.5) will be the center. 
        /// </summary>
        /// <param name="u">The u parameter at which to evaluate the box.</param>
        /// <param name="v">The v parameter at which to evaluate the box.</param>
        /// <param name="w">The w parameter at which to evaluate the box.</param>
        /// <returns>A point in world coordinates.</returns>
        public Vector3 PointAt(double u, double v, double w)
        {
            return Transform.OfPoint(Bounds.PointAt(u, v, w));
        }

        /// <summary>
        /// Get a point from this box by supplying a vector specifying normalized parameters from 0 to 1.
        /// A point at (0,0,0) will be the minimum point of the box, a
        /// point at (1,1,1) will be the maximum point, and a point at
        /// (0.5,0.5,0.5) will be the center. 
        /// </summary>
        /// <param name="uvw">The vector in the box's parametric UVW coordinate space.</param>
        /// <returns>A point in world coordinates.</returns>
        public Vector3 PointAt(Vector3 uvw)
        {
            return PointAt(uvw.X, uvw.Y, uvw.Z);
        }

        /// <summary>
        /// Get a transform from this bounding box by supplying normalized parameters from 0 to 1.
        /// A point at (0,0,0) will be a transform at the minimum point of the bounding box, a
        /// point at (1,1,1) will be at the maximum point, and a point at
        /// (0.5,0.5,0.5) will be at the center. 
        /// </summary>
        /// <param name="u">The u parameter at which to evaluate the box.</param>
        /// <param name="v">The v parameter at which to evaluate the box.</param>
        /// <param name="w">The w parameter at which to evaluate the box.</param>
        /// <returns></returns>
        public Transform TransformAt(double u, double v, double w)
        {
            return new Transform(PointAt(u, v, w)).Concatenated(Transform);
        }

        /// <summary>
        /// For a point in world coordinates, get the corresponding vector
        /// in the box's parametric UVW coordinate space.
        /// </summary>
        /// <param name="point">A point in world coordinates.</param>
        /// <returns>A Vector3 representing the corresponding U,V,W coordinates in the box's coordinate space.</returns>
        public Vector3 UVWAtPoint(Vector3 point)
        {
            return Bounds.UVWAtPoint(_inverseTransform.OfPoint(point));
        }

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

        /// <summary>
        /// Get the transform that maps geometry from one box to another.
        /// </summary>
        public static Transform TransformBetween(Box from, Box to)
        {
            return from.BoxToUVW().Concatenated(to.UVWToBox());
        }
        /// <summary>
        /// Get the transform that maps geometry from this box to a normalized,
        /// world-oriented unit cube at the origin (the UVW coordinate space of
        /// the box).
        /// </summary>
        public Transform BoxToUVW()
        {
            return UVWToBox().Inverted();
        }

        /// <summary>
        /// Get the transform that maps geometry from a normalized,
        /// world-oriented unit cube at the origin (the UVW coordinate space of
        /// the box) to this box.
        /// </summary>
        public Transform UVWToBox()
        {
            var scaleTransform = new Transform().Scaled(new Vector3(Bounds.XSize, Bounds.YSize, Bounds.ZSize));
            var positionTransform = new Transform(Min, Transform.XAxis, Transform.ZAxis);
            return scaleTransform.Concatenated(positionTransform);
        }

        /// <summary>
        /// Check if this box has a valid transform and bounds.
        /// </summary>
        public bool IsValid()
        {
            return Bounds.IsValid() && Transform != null;
        }
    }
}