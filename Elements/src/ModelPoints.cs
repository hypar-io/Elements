using System;
using System.Collections.Generic;
using Elements.Geometry;
using System.Text.Json.Serialization;

namespace Elements
{
    /// <summary>
    /// A collection of points which are visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelPointsTests.cs?name=example)]
    /// </example>
    public class ModelPoints : GeometricElement
    {
        /// <summary>
        /// The locations of the points.
        /// </summary>
        public IList<Vector3> Locations { get; set; }

        /// <summary>
        /// Create a collection of points.
        /// </summary>
        /// <param name="locations">The locations of the points.</param>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model points transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the model points.</param>
        /// <param name="name">The name of the model points.</param>
        [JsonConstructor]
        public ModelPoints(IList<Vector3> locations = null,
                          Material material = null,
                          Transform transform = null,
                          bool isElementDefinition = false,
                          Guid id = default(Guid),
                          string name = null) : base(transform != null ? transform : new Transform(),
                                                     material != null ? material : BuiltInMaterials.Points,
                                                     null,
                                                     isElementDefinition,
                                                     id != default(Guid) ? id : Guid.NewGuid(),
                                                     name)
        {
            this.Locations = locations != null ? locations : new List<Vector3>();
        }

        internal GraphicsBuffers ToGraphicsBuffers()
        {
            var gb = new GraphicsBuffers();

            for (var i = 0; i < this.Locations.Count; i++)
            {
                var l = this.Locations[i];
                gb.AddVertex(l, default(Vector3), default(UV), null);
                gb.AddIndex((ushort)(i));
            }

            return gb;
        }

        /// <summary>
        /// Get graphics buffers and other metadata required to modify a GLB.
        /// </summary>
        /// <returns>
        /// True if there is graphicsbuffers data applicable to add, false otherwise.
        /// Out variables should be ignored if the return value is false.
        /// </returns>
        public override Boolean TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            if (this.Locations.Count == 0)
            {
                return base.TryToGraphicsBuffers(out graphicsBuffers, out id, out mode);
            }
            id = $"{this.Id}_point";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.POINTS;
            graphicsBuffers = new List<GraphicsBuffers>() { this.ToGraphicsBuffers() };
            return true;
        }
    }
}