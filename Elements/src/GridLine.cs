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

            var curve = new ModelCurve(this.Curve);
            graphicsBuffers.Add(curve.ToGraphicsBuffers(true));

            var start = this.Curve.PointAt(0);
            var end = this.Curve.PointAt(1);

            var dirStart = this.Curve.TransformAt(0).ZAxis;
            var dirEnd = this.Curve.TransformAt(1).ZAxis;

            // TransformAt does not seem to consistently point either from start to end, or end to start,
            // so we normalize for that here.
            // TODO: fix TransformAt for all curves so that they point in a consistent direction?

            var segments = this.Curve.ToPolyline().Segments();
            var firstSegment = segments[0];
            if (firstSegment.Direction().PlaneAngleTo(dirStart) > Math.PI / 2)
            {
                dirStart = dirStart.Negate();
            }
            var lastSegment = segments[segments.Length - 1];
            if (lastSegment.Direction().PlaneAngleTo(dirEnd) > Math.PI / 2)
            {
                dirEnd = dirEnd.Negate();
            }

            if (ExtensionBeginning > 0)
            {
                var extensionBeginning = new ModelCurve(new Line(start, start - dirStart * ExtensionBeginning));
                graphicsBuffers.Add(extensionBeginning.ToGraphicsBuffers(true));
            }

            if (ExtensionEnd > 0)
            {
                var extensionEnd = new ModelCurve(new Line(end, end + dirEnd * ExtensionEnd));
                graphicsBuffers.Add(extensionEnd.ToGraphicsBuffers(true));
            }

            var circle = new Circle(Radius);
            circle.Center = start - dirStart * (ExtensionBeginning + Radius);
            graphicsBuffers.Add(new ModelCurve(circle).ToGraphicsBuffers(true));
            return true;
        }
    }
}