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

        new internal Boolean TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            if (this.Line == null)
            {
                return base.TryToGraphicsBuffers(out graphicsBuffers, out id, out mode);
            }

            var rad = 5;

            id = $"{this.Id}_gridline";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.LINES;
            graphicsBuffers = new List<GraphicsBuffers>();

            var line = new ModelCurve(this.Line);
            graphicsBuffers.Add(line.ToGraphicsBuffers(true));

            var dir = this.Line.Direction();
            var extension = new ModelCurve(new Line(this.Line.Start, this.Line.Start - dir * rad));
            graphicsBuffers.Add(extension.ToGraphicsBuffers(true));

            var circle = new Circle(rad);
            circle.Center = this.Line.Start - dir * rad * 2;
            graphicsBuffers.Add(new ModelCurve(circle).ToGraphicsBuffers(true));
            return true;
        }
    }
}