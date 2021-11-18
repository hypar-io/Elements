using System;
using System.Collections.Generic;
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

        new internal Boolean TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            if (this.Line == null)
            {
                return base.TryToGraphicsBuffers(out graphicsBuffers, out id, out mode);
            }

            id = $"{this.Id}_gridline";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.LINES;
            graphicsBuffers = new List<GraphicsBuffers>();

            var line = new ModelCurve(this.Line);
            graphicsBuffers.Add(line.ToGraphicsBuffers(true));

            var dir = this.Line.Direction();

            if (ExtensionBeginning > 0)
            {
                var extensionBeginning = new ModelCurve(new Line(this.Line.Start, this.Line.Start - dir * ExtensionBeginning));
                graphicsBuffers.Add(extensionBeginning.ToGraphicsBuffers(true));
            }

            if (ExtensionEnd > 0)
            {
                var extensionEnd = new ModelCurve(new Line(this.Line.End, this.Line.End + dir * ExtensionEnd));
                graphicsBuffers.Add(extensionEnd.ToGraphicsBuffers(true));
            }

            var circle = new Circle(Radius);
            circle.Center = this.Line.Start - dir * (ExtensionBeginning + Radius);
            graphicsBuffers.Add(new ModelCurve(circle).ToGraphicsBuffers(true));
            return true;
        }
    }
}