using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// The interface for all models.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// The transform of the model.
        /// </summary>
        Transform Transform { get; set; }

        /// <summary>
        /// Add an element to the model.
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        /// <param name="gatherSubElements">Should elements be searched recursively for child elements?</param>
        void AddElement(Element element, bool gatherSubElements = true);

        /// <summary>
        /// Add elements to the model.
        /// </summary>
        /// <param name="elements">A collection of elements to add to the model.</param>
        /// <param name="gatherSubElements">Should elements be searched recursively for child elements?</param>
        void AddElements(IEnumerable<Element> elements, bool gatherSubElements = true);

        /// <summary>
        /// Add elements to the model.
        /// </summary>
        /// <param name="elements">Elements to add to the model.</param>
        void AddElements(params Element[] elements);

        /// <summary>
        /// Get an element by id from the model.
        /// </summary>
        /// <param name="id">The id of the element.</param>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <returns>An element of type T.</returns>
        T GetElementOfType<T>(Guid id) where T : Element;

        /// <summary>
        /// Get an element by name from the model.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <returns>An element of type T.</returns>
        T GetElementByName<T>(string name) where T : Element;

        /// <summary>
        /// Get all elements of type T from the model.
        /// </summary>
        /// <typeparam name="T">The type of elements to return.</typeparam>
        /// <returns>A collection of elements of type T.</returns>
        IEnumerable<T> AllElementsOfType<T>();
    }
}