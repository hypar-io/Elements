using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A collection of arrows which are visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelArrowsTests.cs?name=example)]
    /// </example>
    public class ModelArrows : GeometricElement, IVisualizeCurves3d
    {
        /// <summary>
        /// A collection of tuples specifying the origin, magnitude, and color
        /// of the arrows.
        /// </summary>
        public IList<(Vector3 origin, Vector3 direction, double magnitude, Color? color)> Vectors { get; set; }

        /// <summary>
        /// Should an arrow head be drawn at the start?
        /// </summary>
        public bool ArrowAtStart { get; set; }

        /// <summary>
        /// Should an arrow head be drawn at the end?
        /// </summary>
        public bool ArrowAtEnd { get; set; }

        /// <summary>
        /// The angle of the arrow head.
        /// </summary>
        public double ArrowAngle { get; set; }

        /// <summary>
        /// Create a collection of points.
        /// </summary>
        /// <param name="vectors">A collection of tuples specifying the 
        /// origin, direction, and the magnitude of the arrows.</param>
        /// <param name="arrowAtStart">Should an arrow head be drawn at the start?</param>
        /// <param name="arrowAtEnd">Should an arrow head be drawn at the end?</param>
        /// <param name="arrowAngle">The angle of the arrow head.</param>
        /// <param name="transform">The model arrows' transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the model arrows.</param>
        /// <param name="name">The name of the model arrows.</param>
        [JsonConstructor]
        public ModelArrows(IList<(Vector3 location, Vector3 direction, double magnitude, Color? color)> vectors = null,
                           bool arrowAtStart = false,
                           bool arrowAtEnd = true,
                           double arrowAngle = 60.0,
                           Transform transform = null,
                           bool isElementDefinition = false,
                           Guid id = default(Guid),
                           string name = null) : base(transform != null ? transform : new Transform(),
                                                     BuiltInMaterials.Default,
                                                     null,
                                                     isElementDefinition,
                                                     id != default(Guid) ? id : Guid.NewGuid(),
                                                     name)
        {
            this.Vectors = vectors != null ? vectors : new List<(Vector3, Vector3, double, Color?)>();
            this.ArrowAtEnd = arrowAtEnd;
            this.ArrowAtStart = arrowAtStart;
            this.ArrowAngle = arrowAngle;
        }

        /// <summary>
        /// Visualize model arrows in 3d.
        /// </summary>
        public GraphicsBuffers VisualizeCurves3d(bool lineLoop)
        {
            if (this.Vectors.Count == 0)
            {
                return null;
            }

            var x = 0.1 * Math.Cos(Units.DegreesToRadians(-this.ArrowAngle));
            var y = 0.1 * Math.Sin(Units.DegreesToRadians(-this.ArrowAngle));

            var gb = new GraphicsBuffers();

            for (var i = 0; i < this.Vectors.Count; i++)
            {
                var v = this.Vectors[i];
                var start = v.origin;
                var end = v.origin + v.direction * v.magnitude;
                var up = v.direction.IsParallelTo(Vector3.ZAxis) ? Vector3.YAxis : Vector3.ZAxis;
                var tx = v.direction.Cross(up);
                var ty = v.direction;
                var tz = ty.Cross(tx);
                var tr = new Transform(Vector3.Origin, tx, ty, tz);

                var pts = new List<Vector3>() { v.origin, end };

                if (this.ArrowAtStart)
                {
                    var arrow1 = tr.OfPoint(new Vector3(x, -y));
                    var arrow2 = tr.OfPoint(new Vector3(-x, -y));

                    pts.Add(start);
                    pts.Add(start + arrow1);
                    pts.Add(start);
                    pts.Add(start + arrow2);
                }
                if (this.ArrowAtEnd)
                {
                    var arrow1 = tr.OfPoint(new Vector3(x, y));
                    var arrow2 = tr.OfPoint(new Vector3(-x, y));
                    pts.Add(end);
                    pts.Add(end + arrow1);
                    pts.Add(end);
                    pts.Add(end + arrow2);
                }

                for (var j = 0; j < pts.Count; j++)
                {
                    var pt = pts[j];
                    gb.AddVertex(pt, default(Vector3), default(UV), v.color ?? Colors.Red);
                    gb.AddIndex((ushort)(i * pts.Count + j));
                }
            }

            return gb;
        }
    }
}