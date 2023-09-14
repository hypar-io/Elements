using System.Collections.Generic;
using Elements;
using Elements.Geometry;
using glTFLoader.Schema;
using System;

/// <summary>
/// The element's representation
/// </summary>
public abstract class ElementRepresentation : SharedObject
{
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
    public abstract bool TryToGraphicsBuffers(GeometricElement element, out List<GraphicsBuffers> graphicsBuffers,
        out string id, out MeshPrimitive.ModeEnum? mode);
}