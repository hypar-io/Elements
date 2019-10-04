using Elements.Geometry;
using Elements.GeoJSON;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elements.Serialization.IFC;
using Elements.ElementTypes;

namespace Elements
{
    /// <summary>
    /// A container for elements, element types, materials, and profiles.
    /// </summary>
    public partial class Model
    {
        /// <summary>
        /// The version of the assembly.
        /// </summary>
        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public Model()
        {
            this.Origin = new Position(0, 0);
            this.Elements = new Dictionary<Guid, Element>();
            this.Profiles = new Dictionary<Guid, Profile>();
            this.Materials = new Dictionary<Guid, Material>();
            this.ElementTypes = new Dictionary<Guid, ElementType>();

            AddMaterial(BuiltInMaterials.Edges);
        }

        /// <summary>
        /// Add an element to the model.
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        /// <exception cref="System.ArgumentException">Thrown when an element 
        /// with the same Id already exists in the model.</exception>
        public void AddElement(Element element)
        {
            if (element == null)
            {
                return;
            }

            if (!this.Elements.ContainsKey(element.Id))
            {
                this.Elements.Add(element.Id, element);
                GetRootLevelElementData(element);
            }
            else
            {
                throw new ArgumentException("An element with the same Id already exists in the Model.");
            }

            if (element is IAggregateElements)
            {
                var agg = (IAggregateElements)element;
                AddElements(agg.Elements);
            }
        }

        /// <summary>
        /// Update an element existing in the model.
        /// </summary>
        /// <param name="element">The element to update in the model.</param>
        /// <exception cref="System.ArgumentException">Thrown when no element 
        /// with the same Id exists in the model.</exception>
        public void UpdateElement(Element element)
        {
            if (element == null)
            {
                return;
            }

            if (this.Elements.ContainsKey(element.Id))
            {
                // remove the previous element
                this.Elements.Remove(element.Id);
                // Update the element itselft
                this.Elements.Add(element.Id, element);
                // Update the root elements
                GetRootLevelElementData(element);
            }
            else
            {
                throw new ArgumentException("No element with this Id exists in the Model.");
            }

            if (element is IAggregateElements)
            {
                var agg = (IAggregateElements)element;
                UpdateElements(agg.Elements);
            }
        }

        /// <summary>
        /// Add a collection of elements to the model.
        /// </summary>
        /// <param name="elements">The elements to add to the model.</param>
        public void AddElements(IEnumerable<Element> elements)
        {
            foreach (var e in elements)
            {
                AddElement(e);
            }
        }

        /// <summary>
        /// Update a collection of elements in the model.
        /// </summary>
        /// <param name="elements">The elements to be updated in the model.</param>
        public void UpdateElements(IEnumerable<Element> elements)
        {
            foreach (var e in elements)
            {
                UpdateElement(e);
            }
        }

        /// <summary>
        /// Get an element by id from the Model.
        /// </summary>
        /// <param name="id">The identifier of the element.</param>
        /// <returns>An element or null if no element can be found 
        /// with the provided id.</returns>
        public Element GetElementById(Guid id)
        {
            if (this.Elements.ContainsKey(id))
            {
                return this.Elements[id];
            }
            return null;
        }

        /// <summary>
        /// Get the first element with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>An element or null if no element can be found 
        /// with the provided name.</returns>
        public Element GetElementByName(string name)
        {
            var found = this.Elements.FirstOrDefault(e => e.Value.Name == name);
            if (found.Equals(new KeyValuePair<long, Element>()))
            {
                return null;
            }
            return found.Value;
        }

        /// <summary>
        /// Get a Material by name.
        /// </summary>
        /// <param name="name">The name of the Material.</param>
        /// <returns>A Material or null if no Material with the 
        /// specified id can be found.</returns>
        public Material GetMaterialByName(string name)
        {
            return this.Materials.Values.FirstOrDefault(m => m.Name == name);
        }

        /// <summary>
        /// Get an element type by name.
        /// </summary>
        /// <param name="name">The name of the element type.</param>
        /// <returns>An element type or null if no element type with 
        /// the specified name can be found.</returns>
        public ElementType GetElementTypeByName(string name)
        {
            return this.ElementTypes.Values.FirstOrDefault(et => et.Name == name);
        }

        /// <summary>
        /// Get a Profile by name.
        /// </summary>
        /// <param name="name">The name of the Profile.</param>
        /// <returns>A Profile or null if no Profile with the 
        /// specified name can be found.</returns>
        public Profile GetProfileByName(string name)
        {
            return this.Profiles.Values.FirstOrDefault(p => p.Name != null && p.Name == name);
        }

        /// <summary>
        /// Get all elements of the specified Type.
        /// </summary>
        /// <typeparam name="T">The Type of element to return.</typeparam>
        /// <returns>A collection of elements of the specified type.</returns>
        public IEnumerable<T> ElementsOfType<T>()
        {
            return this.Elements.Values.OfType<T>();
        }

        /// <summary>
        /// Create a model from IFC.
        /// </summary>
        /// <param name="path">The path to the IFC STEP file.</param>
        /// <param name="idsToConvert">An optional array of string identifiers 
        /// of IFC entities to convert.</param>
        /// <returns>A model.</returns>
        public static Model FromIFC(string path, string[] idsToConvert = null)
        {
            return IFCExtensions.FromIFC(path, idsToConvert);
        }

        internal Model(Dictionary<Guid, Element> elements, Dictionary<Guid,
            Material> materials, Dictionary<Guid, ElementType> elementTypes,
            Dictionary<Guid, Profile> profiles)
        {
            this.Elements = elements;
            this.Materials = materials;
            this.ElementTypes = elementTypes;
            this.Profiles = profiles;
            AddMaterial(BuiltInMaterials.Edges);
            AddMaterial(BuiltInMaterials.Void);
        }

        private void AddMaterial(Material material)
        {
            if (!this.Materials.ContainsKey(material.Id))
            {
                this.Materials.Add(material.Id, material);
            }
            else
            {
                this.Materials[material.Id] = material;
            }
        }

        private void GetRootLevelElementData(IElement element)
        {
            if (element is IMaterial)
            {
                var mat = (IMaterial)element;
                AddMaterial(mat.Material);
            }

            if (element is IProfile)
            {
                var ipp = (IProfile)element;
                if (ipp.Profile != null)
                {
                    AddProfile((Profile)ipp.Profile);
                }
            }

            if (element is IHasOpenings)
            {
                var ho = (IHasOpenings)element;
                if (ho.Openings != null)
                {
                    foreach (var o in ho.Openings)
                    {
                        AddProfile(o.Profile);
                    }
                }
            }

            if (element is IElementType<WallType>)
            {
                var wtp = (IElementType<WallType>)element;
                if (wtp.ElementType != null)
                {
                    AddElementType(wtp.ElementType);
                    foreach (var layer in wtp.ElementType.MaterialLayers)
                    {
                        AddMaterial(layer.Material);
                    }
                }
            }

            if (element is IElementType<FloorType>)
            {
                var ftp = (IElementType<FloorType>)element;
                if (ftp.ElementType != null)
                {
                    AddElementType(ftp.ElementType);
                    foreach (var layer in ftp.ElementType.MaterialLayers)
                    {
                        AddMaterial(layer.Material);
                    }
                }
            }

            if (element is IElementType<StructuralFramingType>)
            {
                var sft = (IElementType<StructuralFramingType>)element;
                if (sft.ElementType != null)
                {
                    AddElementType(sft.ElementType);
                    AddProfile(sft.ElementType.Profile);
                    AddMaterial(sft.ElementType.Material);
                }
            }

            if (element is IAggregateElements)
            {
                var ae = (IAggregateElements)element;
                if (ae.Elements.Count > 0)
                {
                    foreach (var esub in ae.Elements)
                    {
                        GetRootLevelElementData(esub);
                    }
                }
            }
        }

        private void AddElementType(ElementType elementType)
        {
            if (!this.ElementTypes.ContainsKey(elementType.Id))
            {
                this.ElementTypes.Add(elementType.Id, elementType);
            }
            else
            {
                this.ElementTypes[elementType.Id] = elementType;
            }
        }

        private void AddProfile(Profile profile)
        {
            if (!this.Profiles.ContainsKey(profile.Id))
            {
                this.Profiles.Add(profile.Id, profile);
            }
            else
            {
                this.Profiles[profile.Id] = profile;
            }
        }
    }
}