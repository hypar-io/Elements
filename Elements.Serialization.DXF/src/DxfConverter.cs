
using Elements;
using Elements.Geometry;
using Elements.Serialization.DXF.Extensions;
using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
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
        /// Add a DXF entity to a document
        /// </summary>
        void TryAddDxfEntity(DxfFile document, Element element, DxfRenderContext context);
    }

    public abstract class DxfConverter<T> : IRenderDxf where T : Element
    {
        public abstract void TryAddDxfEntity(DxfFile document, T element, DxfRenderContext context);

        public void TryAddDxfEntity(DxfFile document, Element element, DxfRenderContext context)
        {
            this.TryAddDxfEntity(document, element as T, context);
        }
    }

    public abstract class GeometricDxfConverter<T> : DxfConverter<T> where T : GeometricElement
    {
        public override void TryAddDxfEntity(DxfFile document, T element, DxfRenderContext context)
        {
            // TODO: handle context / drawing range / etc.
            if (element.Representation == null)
            {
                return;
            }
            var entities = element.GetEntitiesFromRepresentation();
            if (element.IsElementDefinition)
            {
                var block = new DxfBlock
                {
                    Name = element.GetBlockName(),
                    BasePoint = new DxfPoint(0, 0, 0)
                };
                foreach (var e in entities)
                {
                    block.Entities.Add(e);
                }
                document.Blocks.Add(block);
                document.BlockRecords.Add(new DxfBlockRecord(block.Name));
                return;
            }
            foreach (var e in entities)
            {
                document.Entities.Add(e);
            }
        }
    }
}