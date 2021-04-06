using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Components
{
    /// <summary>
    /// All component placement rules should implement this interface.
    /// </summary>
    public interface IComponentPlacementRule
    {
        /// <summary>
        /// The name of the rule.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Create elements from this rule based on a component definition.
        /// </summary>
        /// <param name="definition">The definition to instantiate.</param>
        List<Element> Instantiate(ComponentDefinition definition);
    }

    /// <summary>
    /// Rules that distort a polyline or boundary polygon should implement this interface
    /// </summary>
    public interface ICurveBasedComponentPlacementRule : IComponentPlacementRule
    {
        /// <summary>
        /// The abstract curve being deformed by this rule.
        /// </summary>
        Polyline Curve { get; set; }

        /// <summary>
        /// The indices matching curve vertices to anchors.
        /// </summary>

        IList<int> AnchorIndices { get; set; }

        /// <summary>
        /// The displacement vectors for each curve vertex from its anchor.
        /// </summary>

        IList<Vector3> AnchorDisplacements { get; set; }
    }
}