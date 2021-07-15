
using Elements;
using Elements.Geometry;
using IxMilia.Dxf.Entities;

namespace Elements.Serialization.DXF
{
    /// <summary>
    /// The class defining a drawing range used during rendering
    /// </summary>
    public class DrawingRange
    {
        /// <summary>
        /// The transform representing the origin, and direction of the camera
        /// for this render.
        /// The origin is the camera location, and defines the cut plane.
        /// The camera direction is the -Z axis of this transform.
        /// The X-axis is considered to point to the right of the dxf "paperspace".
        /// </summary>
        public Transform Transform;
        /// <summary>
        /// How far in the -Z direction does the DrawingRange extend.
        /// </summary>
        public double Depth;
    }

    public class DxfRenderContext
    {
        /// <summary>
        /// The entire model that is being rendered.
        /// </summary>
        public Model Model;

        /// <summary>
        /// The drawing range that is currently being modeled.
        /// </summary>
        public DrawingRange DrawingRange;
    }

    /// <summary>
    /// Interface used during ModelToDxf rendering.
    /// </summary>
    public interface IRenderDxf
    {
        /// <summary>
        /// Create a netDxf Entity Object for a given Hypar Element.
        /// </summary>
        bool TryToCreateDxfEntity(Element element, DxfRenderContext context, out DxfEntity entity);
    }
}