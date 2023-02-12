using Elements.Validators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// An axis-aligned bounding box.
    /// </summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public struct BBox3
    {
        /// <summary>The minimum extent of the bounding box.</summary>
        [JsonProperty("Min", Required = Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Vector3 Min { get; set; }

        /// <summary>The maximum extent of the bounding box.</summary>
        [JsonProperty("Max", Required = Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Vector3 Max { get; set; }

        /// <summary>
        /// The x dimension of the bounding box.
        /// </summary>
        [JsonIgnore]
        public double XSize => Max.X - Min.X;

        /// <summary>
        /// The y dimension of the bounding box.
        /// </summary>
        [JsonIgnore]
        public double YSize => Max.Y - Min.Y;

        /// <summary>
        /// The z dimension of the bounding box.
        /// </summary>
        [JsonIgnore]
        public double ZSize => Max.Z - Min.Z;

        /// <summary>
        /// A domain representing the x extents of the bounding box.
        /// </summary>
        [JsonIgnore]
        public Domain1d XDomain => new Domain1d(Min.X, Max.X);

        /// <summary>
        /// A domain representing the y extents of the bounding box.
        /// </summary>
        [JsonIgnore]
        public Domain1d YDomain => new Domain1d(Min.Y, Max.Y);

        /// <summary>
        /// A domain representing the z extents of the bounding box.
        /// </summary>
        [JsonIgnore]
        public Domain1d ZDomain => new Domain1d(Min.Z, Max.Z);

        /// <summary>
        /// Create a bounding box.
        /// </summary>
        /// <param name="min">The minimum point.</param>
        /// <param name="max">The maximum point.</param>
        [JsonConstructor]
        public BBox3(Vector3 @min, Vector3 @max)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (min.X == max.X || min.Y == max.Y)
                {
                    throw new System.ArgumentException("The bounding box will have zero volume, please ensure that the Min and Max don't have any identical vertex values.");
                }
            }

            this.Min = @min;
            this.Max = @max;
        }

        /// <summary>
        /// Construct a bounding box from an array of points.
        /// </summary>
        /// <param name="points">The points which are contained within the bounding box.</param>
        public BBox3(IEnumerable<Vector3> points)
        {
            this.Min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            this.Max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
            foreach (Vector3 v in points)
            {
                this.Extend(v);
            }
        }

        /// <summary>
        /// Extend this bounding box to contain the specified point.
        /// </summary>
        /// <param name="point">The point to include in the bounding box.</param>
        public void Extend(Vector3 point)
        {
            var newMin = new Vector3(Min.X, Min.Y, Min.Z);
            if (point.X < this.Min.X) newMin.X = point.X;
            if (point.Y < this.Min.Y) newMin.Y = point.Y;
            if (point.Z < this.Min.Z) newMin.Z = point.Z;
            this.Min = newMin;

            var newMax = new Vector3(Max.X, Max.Y, Max.Z);
            if (point.X > this.Max.X) newMax.X = point.X;
            if (point.Y > this.Max.Y) newMax.Y = point.Y;
            if (point.Z > this.Max.Z) newMax.Z = point.Z;
            this.Max = newMax;
        }

        /// <summary>
        /// Extend this bound box to contain the specified points.
        /// </summary>
        /// <param name="points">The points to include in the bounding box.</param>
        public void Extend(params Vector3[] points)
        {
            foreach (var pt in points)
            {
                Extend(pt);
            }
        }

        /// <summary>
        /// Create the BBox3 for a Profile.
        /// </summary>
        /// <param name="profile">The Profile.</param>
        public BBox3(Profile profile)
        {
            this.Min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            this.Max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
            for (var i = 0; i < profile.Perimeter.Vertices.Count; i++)
            {
                this.Extend(profile.Perimeter.Vertices[i]);
            }

            for (var i = 0; i < profile.Voids.Count; i++)
            {
                var v = profile.Voids[i];
                for (var j = 0; j < v.Vertices.Count; j++)
                {
                    this.Extend(v.Vertices[j]);
                }
            }
        }

        /// <summary>
        /// Create a bounding box for a collection of polygons.
        /// </summary>
        /// <param name="polygons"></param>
        public BBox3(IList<Polygon> polygons)
        {
            this.Min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            this.Max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
            foreach (var p in polygons)
            {
                foreach (var v in p.Vertices)
                {
                    this.Extend(v);
                }
            }
        }

        /// <summary>
        /// Create the BBox3 for an Element. Elements without any geometry will return invalid boxes. 
        /// Properties of the element that are themselves elements will not be considered.
        /// </summary>
        /// <param name="element">The element.</param>
        public BBox3(Element element)
        {
            this.Min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            this.Max = new Vector3(double.MinValue, double.MinValue, double.MinValue);

            var vertices = VerticesFromElement(element);
            foreach (var v in vertices)
            {
                Extend(v);
            }
        }

        private static List<Vector3> VerticesFromElement(Element element, bool includeElementTransform = true)
        {
            List<Vector3> vertices = new List<Vector3>();
            switch (element)
            {
                case ModelPoints modelPoints:
                    vertices.AddRange(modelPoints.Locations.Select(v => includeElementTransform ? modelPoints.Transform.OfPoint(v) : v));
                    break;
                case ModelCurve modelCurve:
                    vertices.AddRange(modelCurve.Curve.RenderVertices().Select(v => includeElementTransform ? modelCurve.Transform.OfPoint(v) : v));
                    break;
                case ContentElement contentElement:
                    vertices.AddRange(contentElement.BoundingBox.Corners().Select(v => includeElementTransform ? contentElement.Transform.OfPoint(v) : v));
                    break;
                case ElementInstance elementInstance:
                    // instances of content elements are transformed on top of their base definition's orientation; 
                    // other instances are not.
                    var baseDefVertices = VerticesFromElement(elementInstance.BaseDefinition, elementInstance.BaseDefinition is ContentElement);
                    vertices.AddRange(baseDefVertices.Select(elementInstance.Transform.OfPoint));
                    break;
                case MeshElement meshElement:
                    vertices.AddRange(meshElement.Mesh.Vertices.Select(v => meshElement.Transform.OfPoint(v.Position)));
                    break;
                case GeometricElement geometricElement:
                    vertices.AddRange(VerticesFromRepresentation(geometricElement.Representation, includeElementTransform ? geometricElement.Transform : null));
                    break;
                case Profile profile:
                    vertices.AddRange(profile.Perimeter.Vertices);
                    break;
                default:
                    break;
            }
            return vertices;
        }

        private static List<Vector3> VerticesFromRepresentation(Representation representation, Transform transform = null)
        {
            List<Vector3> vertices = new List<Vector3>();
            if (transform == null)
            {
                transform = new Transform();
            }
            if (representation != null && representation.SolidOperations != null && representation.SolidOperations.Count > 0)
            {
                foreach (var solidOp in representation.SolidOperations)
                {
                    var solidTransform = (solidOp.LocalTransform ?? new Transform()).Concatenated(transform);
                    vertices.AddRange(solidOp.Solid.Vertices.Select(v => solidTransform.OfPoint(v.Value.Point)));
                }
            }
            return vertices;
        }

        /// <summary>
        /// Get a translated copy of the bounding box.
        /// </summary>
        /// <param name="translation">The translation to apply.</param>
        public BBox3 Translated(Vector3 translation)
        {
            return new BBox3(this.Min + translation, this.Max + translation);
        }

        /// <summary>
        /// Get the center of the bounding box.
        /// </summary>
        /// <returns>The center of the bounding box.</returns>
        public Vector3 Center()
        {
            return this.Max.Average(this.Min);
        }

        /// <summary>
        /// Get all 8 corners of this bounding box. 
        /// Ordering is CCW bottom, then CCW top, each starting from minimum (X,Y). 
        /// For a unit cube this would be:
        /// (0,0,0),(1,0,0),(1,1,0),(0,1,0),(0,0,1),(1,0,1),(1,1,1),(0,1,1)
        /// </summary>
        /// <returns>The corners of the bounding box.</returns>
        public List<Vector3> Corners()
        {
            return new List<Vector3> {
                Min,
                new Vector3(Max.X,Min.Y,Min.Z),
                new Vector3(Max.X,Max.Y,Min.Z),
                new Vector3(Min.X,Max.Y,Min.Z),
                new Vector3(Min.X,Min.Y,Max.Z),
                new Vector3(Max.X,Min.Y,Max.Z),
                Max,
                new Vector3(Min.X,Max.Y,Max.Z),
            };
        }

        /// <summary>
        /// Get a point from this bounding box by supplying normalized parameters from 0 to 1.
        /// A point at (0,0,0) will be the minimum point of the bounding box, a
        /// point at (1,1,1) will be the maximum point, and a point at
        /// (0.5,0.5,0.5) will be the center. 
        /// </summary>
        /// <param name="u">The u parameter at which to evaluate the box.</param>
        /// <param name="v">The v parameter at which to evaluate the box.</param>
        /// <param name="w">The w parameter at which to evaluate the box.</param>
        /// <returns>A point in world coordinates.</returns>
        public Vector3 PointAt(double u, double v, double w)
        {
            return new Vector3(u.MapToDomain(XDomain), v.MapToDomain(YDomain), w.MapToDomain(ZDomain));
        }

        /// <summary>
        /// Get a point from this bounding box by supplying a vector specifying normalized parameters from 0 to 1.
        /// A point at (0,0,0) will be the minimum point of the bounding box, a
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
            return new Transform(PointAt(u, v, w));
        }

        /// <summary>
        /// For a point in world coordinates, get the corresponding vector
        /// in the box's parametric UVW coordinate space.
        /// </summary>
        /// <param name="point">A point in world coordinates.</param>
        /// <returns>A Vector3 representing the corresponding U,V,W coordinates in the box's coordinate space.</returns>
        public Vector3 UVWAtPoint(Vector3 point)
        {
            return new Vector3(point.X.MapFromDomain(XDomain), point.Y.MapFromDomain(YDomain), point.Z.MapFromDomain(ZDomain));
        }

        /// <summary>
        /// Offset a box in each coordinate by a given amount. 
        /// </summary>
        /// <param name="amount">Offset distance.</param>
        /// <returns></returns>
        public BBox3 Offset(double amount)
        {
            return new BBox3(new Vector3(Min.X - amount, Min.Y - amount, Min.Z - amount),
                             new Vector3(Max.X + amount, Max.Y + amount, Max.Z + amount));
        }

        /// <summary>
        /// The volume of the bounding box.
        /// </summary>
        [JsonIgnore]
        public double Volume => XSize * YSize * ZSize;

        /// <summary>
        /// Is the provided object a bounding box? If so, is it
        /// equal to this bounding box within Epsilon?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BBox3))
            {
                return false;
            }
            var a = (BBox3)obj;
            return this.Min.IsAlmostEqualTo(a.Min) && this.Max.IsAlmostEqualTo(a.Max);
        }

        /// <summary>
        /// Get the hash code for the bounding box.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// The string representation of the bounding box.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Min:{this.Min}, Max:{this.Max}";
        }

        /// <summary>
        /// Are the two bounding boxes equal within Epsilon?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator ==(BBox3 a, BBox3 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Are the two bounding boxes not equal within Epsilon?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator !=(BBox3 a, BBox3 b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Does this bounding box have a valid set value?
        /// </summary>
        public bool IsValid()
        {
            return
                Min.X != double.MaxValue &&
                Min.Y != double.MaxValue &&
                Min.Z != double.MaxValue &&
                Max.X != double.MinValue &&
                Max.Y != double.MinValue &&
                Max.Z != double.MinValue;
        }

        /// <summary>
        /// Does this bounding box have a dimension of 0 along any axis?
        /// </summary>
        public bool IsDegenerate()
        {
            return
                Min.X.ApproximatelyEquals(Max.X) ||
                Min.Y.ApproximatelyEquals(Max.Y) ||
                Min.Z.ApproximatelyEquals(Max.Z);
        }

        /// <summary>
        /// Does this bounding box contain the provided point?
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if the bounding box contains the point, otherwise false.</returns>
        public bool Contains(Vector3 point)
        {
            if (point <= Max && point >= Min)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Does this bounding box intersect the other bounding box?
        /// </summary>
        /// <param name="other">The bounding box to test.</param>
        /// <returns>True if an intersection occurs, otherwise false.</returns>
        public bool Intersects(BBox3 other)
        {
            return !(other.Min.X > Max.X
                     || other.Max.X < Min.X
                     || other.Min.Y > Max.Y
                     || other.Max.Y < Min.Y
                     || other.Min.Z > Max.Z
                     || other.Max.Z < Min.Z);
        }

        /// <summary>
        /// Does the bounding box intersect the provided plane?
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <param name="transform">An optional transform of the bounding box to apply before intersection.</param>
        /// <param name="relationToPlane">The relation of the bounding box to the plane.</param>
        /// <returns>True if the bounding box intersects, otherwise false.</returns>
        public bool Intersects(Plane plane, out RelationToPlane relationToPlane, Transform transform = null)
        {
            var transformedPlane = new Plane(plane.Origin, plane.Normal);
            if (transform != null)
            {
                // Transform the plane into the space of the bounding box.
                var inverse = transform.Inverted();
                transformedPlane.Origin = inverse.OfPoint(transformedPlane.Origin);
                transformedPlane.Normal = inverse.OfVector(transformedPlane.Normal);
            }

            // https://gdbooks.gitbooks.io/3dcollisions/content/Chapter2/static_aabb_plane.html
            // Convert AABB to center-extents representation
            var c = (Max + Min) * 0.5; // Compute AABB center
            var e = Max - c; // Compute positive extents

            // Compute the projection interval radius of b onto L(t) = b.c + t * p.n
            var r = e.X * Math.Abs(transformedPlane.Normal.X) + e.Y * Math.Abs(transformedPlane.Normal.Y) + e.Z * Math.Abs(transformedPlane.Normal.Z);

            // Compute distance of box center from plane
            var s = c.DistanceTo(transformedPlane);

            // Intersection occurs when distance s falls within [-r,+r] interval
            if (Math.Abs(s) <= r)
            {
                relationToPlane = RelationToPlane.Intersects;
                return true;
            }

            var corners = Corners();
            if (corners.All(co => co.DistanceTo(transformedPlane) < 0))
            {
                relationToPlane = RelationToPlane.Below;
                return false;
            }
            else if (corners.All(co => co.DistanceTo(transformedPlane) > 0))
            {
                relationToPlane = RelationToPlane.Above;
                return false;
            }

            relationToPlane = RelationToPlane.None;
            return false;
        }
    }
}