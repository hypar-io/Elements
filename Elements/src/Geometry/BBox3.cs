using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// An axis-aligned bounding box.
    /// </summary>
    public partial struct BBox3
    {
        /// <summary>
        /// Construct a bounding box from an array of points.
        /// </summary>
        /// <param name="points">The points which are contained within the bounding box.</param>
        public BBox3(IList<Vector3> points)
        {
            this.Min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            this.Max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
            for (var i = 0; i < points.Count; i++)
            {
                this.Extend(points[i]);
            }
        }

        internal void Extend(Vector3 v)
        {
            var newMin = new Vector3(Min.X, Min.Y, Min.Z);
            if (v.X < this.Min.X) newMin.X = v.X;
            if (v.Y < this.Min.Y) newMin.Y = v.Y;
            if (v.Z < this.Min.Z) newMin.Z = v.Z;
            this.Min = newMin;

            var newMax = new Vector3(Max.X, Max.Y, Max.Z);
            if (v.X > this.Max.X) newMax.X = v.X;
            if (v.Y > this.Max.Y) newMax.Y = v.Y;
            if (v.Z > this.Max.Z) newMax.Z = v.Z;
            this.Max = newMax;
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
            return $"Min:{this.Min.ToString()}, Max:{this.Max.ToString()}";
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
    }
}