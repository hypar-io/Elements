using System.Collections.Generic;
using Elements;
using Elements.Geometry;
using glTFLoader.Schema;
using Elements.Serialization.glTF;

/// <summary>
/// The element's representation
/// </summary>
public abstract class ElementRepresentation : SharedObject
{
    protected List<SnappingPoints> _snapPoints;

    /// <summary>
    /// Get graphics buffers and other metadata required to modify a GLB.
    /// </summary>
    /// <param name="element">The element with this representation.</param>
    /// <param name="graphicsBuffers">The list of graphic buffers.</param>
    /// <param name="id">The buffer id. It will be used as a primitive name.</param>
    /// <param name="mode">The gltf primitive mode</param>
    /// <returns>
    /// True if there is graphics buffers data applicable to add, false otherwise.
    /// Out variables should be ignored if the return value is false.
    /// </returns>
    public abstract bool TryToGraphicsBuffers(GeometricElement element, out List<GraphicsBuffers> graphicsBuffers,
        out string id, out MeshPrimitive.ModeEnum? mode);

    internal virtual List<NodeExtension> GetNodeExtensions(GeometricElement element)
    {
        return new List<NodeExtension>();
    }

    /// <summary>
    /// Set the snapping points for this representation.
    /// </summary>
    public void SetSnappingPoints(List<SnappingPoints> snapPoints)
    {
        _snapPoints = snapPoints;
    }

    /// <summary>
    /// Get the snapping points for this representation. Uses CreateSnappingPoints if _snapPoints is null.
    /// </summary>
    public List<SnappingPoints> GetSnappingPoints(GeometricElement element)
    {
        if (_snapPoints == null) return CreateSnappingPoints(element);
        return _snapPoints;
    }


    /// <summary>
    ///Creates the set of snapping points
    /// </summary>
    /// <param name="element">The element with this representation.</param>
    /// <returns></returns>
    protected virtual List<SnappingPoints> CreateSnappingPoints(GeometricElement element)
    {
        return new List<SnappingPoints>();
    }
}