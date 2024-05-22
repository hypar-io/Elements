using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using glTFLoader.Schema;

namespace Elements
{
    /// <summary>
    /// An element representation displayed as an open or closed continuous curve.
    /// </summary>
    public class CurveRepresentation : ElementRepresentation
    {
        private BoundedCurve _curve;
        private bool _isSelectable = true;

        /// <summary>
        /// Initializes a new instance of CurveRepresentation.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="isSelectable">If curve is selectable.</param>
        public CurveRepresentation(BoundedCurve curve, bool isSelectable)
        {
            _curve = curve;
            _isSelectable = isSelectable;
        }

        /// <summary>
        /// The curve.
        /// </summary>
        public BoundedCurve Curve => _curve;

        /// <summary>
        /// Indicates if curve is selectable.
        /// </summary>
        public bool IsSelectable => _isSelectable;

        /// <inheritdoc/>
        public override bool TryToGraphicsBuffers(GeometricElement element, out List<GraphicsBuffers> graphicsBuffers, out string id, out MeshPrimitive.ModeEnum? mode)
        {
            graphicsBuffers = new List<GraphicsBuffers>
            {
                _curve.ToGraphicsBuffers()
            };

            id = _isSelectable ? $"{element.Id}_curve" : $"unselectable_{element.Id}_curve";
            mode = _curve.IsClosedForRendering ? glTFLoader.Schema.MeshPrimitive.ModeEnum.LINE_LOOP : glTFLoader.Schema.MeshPrimitive.ModeEnum.LINE_STRIP;
            return true;
        }

        /// <inheritdoc/>
        protected override List<SnappingPoints> CreateSnappingPoints(GeometricElement element)
        {
            var snappingPoints = new List<SnappingPoints>();
            var curvePoints = _curve.RenderVertices();
            snappingPoints.Add(new SnappingPoints(curvePoints));
            return snappingPoints;
        }
    }
}