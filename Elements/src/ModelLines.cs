using Elements.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements
{
    /// <summary>
    /// A collection of lines which are visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelLinesTests.cs?name=example)]
    /// </example>
    public class ModelLines : GeometricElement
    {
        /// <summary>
        /// The lines.
        /// </summary>
        public IList<Line> Lines { get; set; }

        private bool _isSelectable = false;

        /// <summary>
        /// Create a collection of lines. They share Material, Transformation and other parameters.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model lines transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the model lines.</param>
        /// <param name="name">The name of the model lines.</param>
        [JsonConstructor]
        public ModelLines(IList<Line> lines = null,
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
            this.Lines = lines != null ? lines : new List<Line>();
            this.Material = material != null ? material : BuiltInMaterials.Edges;
        }

        /// <summary>
        /// Set whether these model lines should be selectable in the web UI.
        /// Lines are not selectable by default.
        /// </summary>
        /// <param name="selectable"></param>
        public void SetSelectable(bool selectable)
        {
            _isSelectable = selectable;
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
            if (Lines.Count == 0)
            {
                return base.TryToGraphicsBuffers(out graphicsBuffers, out id, out mode);
            }
            id = _isSelectable ? $"{this.Id}_lines" : $"unselectable_{this.Id}_lines";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.LINES;
            graphicsBuffers = new List<GraphicsBuffers>();

            List<Vector3> points = new List<Vector3>();
            foreach (var line in Lines)
            {
                points.Add(line.Start);
                points.Add(line.End);
            }

            graphicsBuffers.Add(points.ToGraphicsBuffers());
            return true;
        }
    }
}