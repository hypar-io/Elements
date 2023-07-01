using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// An element in a building, such as a floor, wall, or window.
    /// </summary>
    public class BuildingElement : GeometricElement
    {
        /// <summary>
        /// A collection of openings.
        /// </summary>
        public List<Opening> Openings { get; } = new List<Opening>();

        /// <summary>
        /// Construct a building element.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="material"></param>
        /// <param name="representation"></param>
        /// <param name="isElementDefinition"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public BuildingElement(Transform transform = null,
                               Material material = null,
                               Representation representation = null,
                               bool isElementDefinition = false,
                               Guid id = default,
                               string name = null) : base(transform, material, representation, isElementDefinition, id, name) { }
    }
}