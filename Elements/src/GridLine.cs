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
        public Line Line { get; set; }

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
            if (this.Line == null)
            {
                return base.TryToGraphicsBuffers(out graphicsBuffers, out id, out mode);
            }

            id = $"{this.Id}_gridline";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.LINES;
            graphicsBuffers = new List<GraphicsBuffers>();
            var dir = this.Line.Direction();
            var line = new Line(this.Line.Start - dir * ExtensionBeginning, this.Line.End + dir * ExtensionEnd);
            var center = this.Line.Start - dir * (ExtensionBeginning + Radius);
            var normal = Vector3.ZAxis;
            if (normal.Dot(dir) > 1 - Vector3.EPSILON)
            {
                normal = Vector3.XAxis;
            }
            var circle = new Circle(Radius);
            var circleVertexTransform = new Transform(center, dir, normal);
            var renderVertices = new List<Vector3>();
            renderVertices.AddRange(circle.RenderVertices().Select(v => circleVertexTransform.OfPoint(v)));
            renderVertices.AddRange(line.RenderVertices());
            graphicsBuffers.Add(renderVertices.ToGraphicsBuffers(true));
            return true;
        }
    }
}