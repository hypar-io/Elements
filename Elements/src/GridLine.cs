using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// An architectural or structural gridline.
    /// </summary>
    public class GridLine : GeometricElement
    {
        /// <summary>
        /// Line that runs from the start of the gridline to its end.
        /// </summary>
        /// <value></value>
        [Obsolete("We now use 'Curve' instead.")]
        public Line Line { get; set; }

        /// <summary>
        /// Curve that runs from the start of the gridline to its end.
        /// </summary>
        /// <value></value>
        public Curve Curve { get; set; }

        /// <summary>
        /// Radius of the gridline head.
        /// </summary>
        public double Radius = 1;

        /// <summary>
        /// How far to extend the gridline from the beginning to the start of the circle.
        /// </summary>
        public double ExtensionBeginning = 1;

        /// <summary>
        /// How far to extend the gridline past the end of the circle.
        /// </summary>
        public double ExtensionEnd = 1;

        internal override Boolean TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            if (this.Curve == null)
            {
                return base.TryToGraphicsBuffers(out graphicsBuffers, out id, out mode);
            }

            id = $"{this.Id}_gridline";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.LINES;
            graphicsBuffers = new List<GraphicsBuffers>();

            var renderVertices = new List<Vector3>();

            var start = GetPointAndDirectionAt(0);
            var end = GetPointAndDirectionAt(1);

            var circle = new Circle(Radius);
            var circleVertexTransform = GetCircleTransform();

            renderVertices.AddRange(circle.RenderVertices().Select(v => circleVertexTransform.OfPoint(v)));

            if (ExtensionBeginning > 0)
            {
                renderVertices.Add(start.point - start.dir * ExtensionBeginning);
            }

            renderVertices.AddRange(this.Curve.RenderVertices());

            if (ExtensionEnd > 0)
            {
                renderVertices.Add(end.point + end.dir * ExtensionEnd);
            }

            graphicsBuffers.Add(renderVertices.ToGraphicsBuffers(true));

            return true;
        }

        /// <summary>
        /// TODO: This function can be sidestepped altogether if curve.TransformAt consistently pointed from start to end or vice versa.
        /// </summary>
        private (Vector3 point, Vector3 dir) GetPointAndDirectionAt(double u)
        {
            var transform = this.Curve.TransformAt(u);
            var point = transform.Origin;
            var dir = transform.ZAxis;

            // TransformAt does not seem to consistently point either from start to end, or end to start,
            // so we normalize for that here in a very rough way.
            var segments = this.Curve.ToPolyline().Segments();
            var segment = u < 0.5 ? segments[0] : segments[segments.Length - 1];
            if (segment.Direction().PlaneAngleTo(dir) > Math.PI / 2)
            {
                dir = dir.Negate();
            }
            return (point, dir);
        }

        /// <summary>
        /// Get the transform of the circle created by the gridline.
        /// </summary>
        /// <returns></returns>
        public Transform GetCircleTransform()
        {
            var start = GetPointAndDirectionAt(0);
            var normal = Vector3.ZAxis;
            if (normal.Dot(start.dir) > 1 - Vector3.EPSILON)
            {
                normal = Vector3.XAxis;
            }
            var circleCenter = start.point - start.dir * (ExtensionBeginning + Radius);
            var circleVertexTransform = new Transform(circleCenter, start.dir, normal);
            return circleVertexTransform;
        }
    }
}