using System.Collections.Generic;
using Elements.Geometry;
using glTFLoader.Schema;

namespace Elements
{
    /// <summary>
    /// The element representation that represented like a curve.
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

        /// <summary>
        /// Get graphics buffers and other metadata required to modify a GLB.
        /// </summary>
        /// <param name="element">The element with this representation.</param>
        /// <param name="graphicsBuffers">The list of graphc buffers.</param>
        /// <param name="id">The buffer id. It will be used as a primitive name.</param>
        /// <param name="mode">The gltf primitive mode</param>
        /// <returns>
        /// True if there is graphicsbuffers data applicable to add, false otherwise.
        /// Out variables should be ignored if the return value is false.
        /// </returns>
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
    }
}