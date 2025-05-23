#pragma warning disable CS1591
#pragma warning disable CS1570

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;
using Elements.Serialization.JSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.GeoJSON;
using System.IO;

namespace Elements
{
    /// <summary>
    /// A container for elements.
    /// </summary>
    public class Model
    {
        private class GatherSubElementsResult
        {
            /// <summary>
            /// List of elements collected from the object.
            /// </summary>
            public List<Element> Elements { get; } = new List<Element>();
            /// <summary>
            /// List of shared objects collected from the object.
            /// </summary>
            public List<SharedObject> SharedObjects { get; } = new List<SharedObject>();
            /// <summary>
            /// List of elements collected from the shared object's properties.
            ///
            /// If shared object is marked as JsonIgnore (e.g. RepresentationInstance), it will not be
            /// serialized to JSON, but its properties will be collected here so they can be used
            /// during gltf serialization.
            /// </summary>
            public List<Element> ElementsFromSharedObjectProperties { get; } = new List<Element>();

            public void MergeSubResult(GatherSubElementsResult gatherResult, bool hasJsonIgnore, bool isTypeRelatedToSharedObjects)
            {
                if (isTypeRelatedToSharedObjects)
                {
                    ElementsFromSharedObjectProperties.AddRange(gatherResult.Elements);
                }
                else
                {
                    Elements.AddRange(gatherResult.Elements);
                }
                // do not save shared objects marked with JsonIgnore
                if (!hasJsonIgnore)
                {
                    SharedObjects.AddRange(gatherResult.SharedObjects);
                    Elements.AddRange(gatherResult.ElementsFromSharedObjectProperties);
                }
                ElementsFromSharedObjectProperties.AddRange(gatherResult.ElementsFromSharedObjectProperties);
            }
        }

        /// <summary>The origin of the model.</summary>
        [JsonProperty("Origin", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Obsolete("Use Transform instead.")]
        public Position Origin { get; set; }

        /// <summary>The transform of the model.</summary>
        [JsonProperty("Transform")]
        public Transform Transform { get; set; }

        /// <summary>A collection of Elements keyed by their identifiers.</summary>
        [JsonProperty("Elements", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public System.Collections.Generic.IDictionary<Guid, Element> Elements { get; set; } = new System.Collections.Generic.Dictionary<Guid, Element>();

        /// <summary>A collection of SharedObjects keyed by their identifiers.</summary>
        [JsonProperty("SharedObjects", Required = Required.Default)]
        public System.Collections.Generic.IDictionary<Guid, SharedObject> SharedObjects { get; set; } = new System.Collections.Generic.Dictionary<Guid, SharedObject>();

        /// <summary>
        /// Collection of subelements from shared objects or RepresentationInstances (e.g. SolidRepresentation.Profile or RepresentationInstance.Material).
        /// We do not serialize shared objects to json, but we do include them in other formats like gltf.
        /// This collection contains all elements referenced directly by RepresentationInstances, such as Materials and Profiles.
        /// These objects affect representation appearance and may be used at glTF creation time.
        /// </summary>
        [JsonIgnore]
        public System.Collections.Generic.IDictionary<Guid, Element> SubElementsFromSharedObjects { get; set; } = new System.Collections.Generic.Dictionary<Guid, Element>();

        /// <summary>
        /// Construct a model.
        /// </summary>
        /// <param name="origin">The origin of the model.</param>
        /// <param name="transform">The transform of the model.</param>
        /// <param name="elements">A collection of elements.</param>
        [JsonConstructor]
        public Model(Position @origin, Transform @transform, System.Collections.Generic.IDictionary<Guid, Element> @elements)
        {

#pragma warning disable CS0618
            this.Origin = @origin;
#pragma warning restore CS0618
            this.Transform = @transform;
            this.Elements = @elements;
        }

        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public Model()
        {
            this.Transform = new Transform();
        }

        /// <summary>
        /// Construct a model.
        /// </summary>
        /// <param name="transform">The model's transform.</param>
        /// <param name="elements">The model's elements.</param>
        public Model(Transform @transform, System.Collections.Generic.IDictionary<Guid, Element> @elements)
        {
            this.Transform = @transform;
            this.Elements = @elements;
        }

        /// <summary>
        /// Construct a model with the given elements.
        /// </summary>
        /// <param name="elements">The model's elements.</param>
        /// <param name="transform">The models' transform.</param>
        public Model(System.Collections.Generic.IEnumerable<Element> @elements, Transform transform = null)
        {
            this.Transform = transform ?? new Transform();
            this.Elements = @elements.ToDictionary(e => e.Id, e => e);
        }

        /// <summary>
        /// Add an element to the model.
        /// This operation recursively searches the element's properties
        /// for element sub-properties and adds those elements to the elements
        /// dictionary before adding the element itself.
        /// Properties of the following types are be supported for introspection:
        /// Element, IList<Element>, IDictionary<Element>, Representation, IList<SolidOperation>
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        /// <param name="gatherSubElements">Should sub-elements in properties be
        /// added to the model's elements collection?</param>
        /// <param name="updateElementsRepresentations">Indicates whether UpdateRepresentation should be called for the element.</param>
        public void AddElement(Element element, bool gatherSubElements = true, bool updateElementRepresentations = true)
        {
            if (element == null || this.Elements.ContainsKey(element.Id))
            {
                return;
            }

            // Function wrapper code no longer calls UpdateRepresentations, so we need to do it here.
            if (updateElementRepresentations && element is GeometricElement geo)
            {
                geo.UpdateRepresentations();
            }

            if (gatherSubElements)
            {
                // Look at all public properties of the element.
                // For all properties which inherit from element, add those
                // to the elements dictionary first. This will ensure that
                // those elements will be read out and be available before
                // an attempt is made to deserialize the element itself.
                var gatherSubElementsResult = RecursiveGatherSubElements(element);
                foreach (var e in gatherSubElementsResult.Elements)
                {
                    if (!this.Elements.ContainsKey(e.Id))
                    {
                        // Because function wrapper code doesn't call UpdateRepresentations any more
                        // we need to call it here for all nested elements while they are added.
                        if (updateElementRepresentations && e is GeometricElement geoE)
                        {
                            geoE.UpdateRepresentations();
                        }
                        this.Elements.Add(e.Id, e);
                    }
                }

                foreach (var sharedObject in gatherSubElementsResult.SharedObjects)
                {
                    if (!SharedObjects.ContainsKey(sharedObject.Id))
                    {
                        SharedObjects.Add(sharedObject.Id, sharedObject);
                    }
                }

                foreach (var e in gatherSubElementsResult.ElementsFromSharedObjectProperties)
                {
                    if (!SubElementsFromSharedObjects.ContainsKey(e.Id))
                    {
                        SubElementsFromSharedObjects.Add(e.Id, e);
                    }
                }
            }
            else
            {
                if (!this.Elements.ContainsKey(element.Id))
                {
                    this.Elements.Add(element.Id, element);
                }
            }
        }

        /// <summary>
        /// Add a collection of elements to the model.
        /// </summary>
        /// <param name="elements">The elements to add to the model.</param>
        /// <param name="gatherSubElements">Should sub-elements in properties be
        /// added to the model's elements collection?</param>
        public void AddElements(IEnumerable<Element> elements, bool gatherSubElements = true)
        {
            foreach (var e in elements)
            {
                AddElement(e, gatherSubElements);
            }
        }

        /// <summary>
        /// Add elements to the model.
        /// </summary>
        /// <param name="elements">The elements to add to the model.</param>
        public void AddElements(params Element[] elements)
        {
            foreach (var e in elements)
            {
                AddElement(e);
            }
        }

        /// <summary>
        /// Get an entity by id from the Model.
        /// </summary>
        /// <param name="id">The identifier of the element.</param>
        /// <returns>An entity or null if no entity can be found
        /// with the provided id.</returns>
        public T GetElementOfType<T>(Guid id) where T : Element
        {
            if (this.Elements.ContainsKey(id))
            {
                return (T)this.Elements[id];
            }
            return null;
        }

        /// <summary>
        /// Get the first entity with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>An entity or null if no entity can be found
        /// with the provided name.</returns>
        public T GetElementByName<T>(string name) where T : Element
        {
            var found = this.Elements.FirstOrDefault(e => e.Value.Name == name);
            if (found.Equals(new KeyValuePair<long, Element>()))
            {
                return null;
            }
            return (T)found.Value;
        }

        /// <summary>
        /// Get all elements of the type T.
        /// </summary>
        /// <typeparam name="T">The type of element to return.</typeparam>
        /// <returns>A collection of elements of the specified type.</returns>
        public IEnumerable<T> AllElementsOfType<T>() where T : Element
        {
            return Elements.Values.OfType<T>();
        }

        /// <summary>
        /// Get all elements assignable from type T. This will include
        /// types which derive from T and types which implement T if T
        /// is an interface.
        /// </summary>
        /// <typeparam name="T">The type of the element from which returned elements derive.</typeparam>
        /// <returns>A collection of elements derived from the specified type.</returns>1
        public IEnumerable<T> AllElementsAssignableFromType<T>() where T : Element
        {
            return Elements.Values.Where(e => typeof(T).IsAssignableFrom(e.GetType())).Cast<T>();
        }

        /// <summary>
        /// Serialize the model to JSON.
        /// </summary>
        /// <param name="indent">Should the JSON be indented?</param>
        /// <param name="gatherSubElements">Should sub-elements of elements be processed?</param>
        /// <param name="updateElementsRepresentations">Indicates whether UpdateRepresentation should be called for all elements.</param>
        public string ToJson(bool indent = false, bool gatherSubElements = true, bool updateElementsRepresentations = true)
        {
            var exportModel = CreateExportModel(gatherSubElements, updateElementsRepresentations);

            return JsonConvert.SerializeObject(exportModel, indent ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Serialize the model to JSON and write to a stream.
        /// </summary>
        /// <param name="stream">The stream into which the JSON will be written.</param>
        /// <param name="indent">Should the JSON be indented?</param>
        /// <param name="gatherSubElements">Should sub-elements of elements be processed?</param>
        /// <param name="updateElementsRepresentations">Indicates whether UpdateRepresentation should be called for all elements.</param>
        public void ToJson(MemoryStream stream, bool indent = false, bool gatherSubElements = true, bool updateElementsRepresentations = true)
        {
            var exportModel = CreateExportModel(gatherSubElements, updateElementsRepresentations);

            var json = JsonConvert.SerializeObject(exportModel, indent ? Formatting.Indented : Formatting.None);
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(json);
            }
        }

        /// <summary>
        /// Serialize the model to JSON using default arguments.
        /// </summary>
        public string ToJson()
        {
            // By default we don't want to update representations because the UpdateRepresentation
            // method is called during function adding.  Setting this to false makes the behavior
            // match our function wrapping code behavior.
            return ToJson(false, true, false);
        }

        /// <summary>
        /// Serialize the model to JSON to match default arguments.
        /// TODO this method can be removed after Hypar.Functions release 0.9.11 occurs.
        /// </summary>
        public string ToJson(bool indent = false)
        {
            return ToJson(indent, true);
        }

        /// <summary>
        /// Serialize the model to a JSON file.
        /// </summary>
        /// <param name="path">The path of the file on disk.</param>
        /// <param name="gatherSubElements"></param>
        /// <param name="updateElementsRepresentations">Indicates whether UpdateRepresentation should be called for all elements.</param>
        public void ToJson(string path, bool gatherSubElements = true, bool updateElementsRepresentations = true)
        {
            var exportModel = CreateExportModel(gatherSubElements, updateElementsRepresentations);

            // Json.net recommends writing to a stream for anything over 85k to avoid a string on the large object heap.
            // https://www.newtonsoft.com/json/help/html/Performance.htm
            using (FileStream s = File.Create(path))
            using (StreamWriter writer = new StreamWriter(s))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, exportModel);
                jsonWriter.Flush();
            }
        }

        /// <summary>
        /// Intersect the model with the provided plane.
        /// </summary>
        /// <param name="plane">The intersection plane.</param>
        /// <param name="intersectionPolygons">A collection of polygons resulting from the
        /// intersection of the plane with all elements in the model.</param>
        /// <param name="beyondPolygons">A collection of polygons resulting from intersection
        /// of the beyond plane with all elements in the model.</param>
        /// <param name="lines">A collection of line segments resulting from intersection of the plane
        /// with all elements in the model.</param>
        public void Intersect(Plane plane,
                              out Dictionary<Guid, List<Geometry.Polygon>> intersectionPolygons,
                              out Dictionary<Guid, List<Geometry.Polygon>> beyondPolygons,
                              out Dictionary<Guid, List<Geometry.Line>> lines)
        {
            UpdateBoundsAndComputedSolids();

            var geos = Elements.Values.Where(e => e is GeometricElement geo && geo.IsElementDefinition == false).Cast<GeometricElement>();
            var intersectingElements = geos.Where(geo => geo.Bounds.Intersects(plane, out _)).ToList();

            var geoInstances = Elements.Values.Where(e => e is ElementInstance instance).Cast<ElementInstance>();
            var intersectingInstances = geoInstances.Where(geo => geo.BaseDefinition.Bounds.Intersects(plane, out _, geo.Transform)).ToList();

            var allIntersectingElements = new List<Element>();
            allIntersectingElements.AddRange(intersectingInstances);
            allIntersectingElements.AddRange(intersectingElements);

            IntersectElementsWithPlane(plane, allIntersectingElements, out intersectionPolygons, out beyondPolygons, out lines);
        }

        /// <summary>
        /// Update the representations of all geometric elements in the model.
        /// </summary>
        public void UpdateRepresentations()
        {
            foreach (var geo in Elements.Values.Where(e => e is GeometricElement).Cast<GeometricElement>())
            {
                geo.UpdateRepresentations();
            }
        }

        /// <summary>
        /// Update the bounds and computed solids of all geometric elements in the model.
        /// </summary>
        public void UpdateBoundsAndComputedSolids()
        {
            foreach (GeometricElement geo in Elements.Values.Where(e => e is GeometricElement).Cast<GeometricElement>())
            {
                geo.UpdateBoundsAndComputeSolid();
            }
        }

        private void IntersectElementsWithPlane(Plane plane,
                                       List<Element> intersecting,
                                       out Dictionary<Guid, List<Geometry.Polygon>> intersectionPolygons,
                                       out Dictionary<Guid, List<Geometry.Polygon>> beyondPolygons,
                                       out Dictionary<Guid, List<Geometry.Line>> lines)
        {
            intersectionPolygons = new Dictionary<Guid, List<Geometry.Polygon>>();
            beyondPolygons = new Dictionary<Guid, List<Geometry.Polygon>>();
            lines = new Dictionary<Guid, List<Geometry.Line>>();

            foreach (var element in intersecting)
            {
                GeometricElement geo = null;
                if (element is GeometricElement)
                {
                    geo = element as GeometricElement;
                }
                else if (element is ElementInstance instance)
                {
                    geo = instance.BaseDefinition;
                }


                if (geo.Intersects(plane, out var xsectP, out var beyondP, out var xsectL))
                {
                    foreach (var kvp in xsectP)
                    {
                        intersectionPolygons.Add(kvp.Key, kvp.Value);
                    }
                    foreach (var kvp in beyondP)
                    {
                        beyondPolygons.Add(kvp.Key, kvp.Value);
                    }
                    foreach (var kvp in xsectL)
                    {
                        lines.Add(kvp.Key, kvp.Value);
                    }
                }

            }
        }

        internal Model CreateExportModel(bool gatherSubElements, bool updateElementsRepresentations)
        {
            // Recursively add elements and sub elements in the correct
            // order for serialization. We do this here because element properties
            // may have been null when originally added, and we need to ensure
            // that they have a value if they've been set since.
            var exportModel = new Model();
            foreach (var kvp in this.Elements)
            {
                exportModel.AddElement(kvp.Value, gatherSubElements, updateElementsRepresentations);
            }
            exportModel.Transform = this.Transform;
            return exportModel;
        }

        /// <summary>
        /// Deserialize a model from JSON.
        /// </summary>
        /// <param name="json">The JSON representing the model.</param>
        /// <param name="errors">A collection of deserialization errors.</param>
        /// <param name="forceTypeReload">Option to force reloading the internal type cache. Use if you add types dynamically in your code.</param>
        public static Model FromJson(string json, out List<string> errors, bool forceTypeReload = false)
        {
            // When user elements have been loaded into the app domain, they haven't always been
            // loaded into the InheritanceConverter's Cache.  This does have some overhead,
            // but is useful here, at the Model level, to ensure user types are available.
            var deserializationErrors = new List<string>();
            if (forceTypeReload)
            {
                JsonInheritanceConverter.RefreshAppDomainTypeCache(out var typeLoadErrors);
                deserializationErrors.AddRange(typeLoadErrors);
            }

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Model>(json, new JsonSerializerSettings()
            {
                Error = (sender, args) =>
                {
                    deserializationErrors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            });
            deserializationErrors.AddRange(JsonInheritanceConverter.GetAndClearDeserializationWarnings());
            errors = deserializationErrors;
            JsonInheritanceConverter.Elements.Clear();
            return model;
        }

        public static Model FromJson(string json, bool forceTypeReload = false)
        {
            return FromJson(json, out _, forceTypeReload);
        }

        private GatherSubElementsResult RecursiveGatherSubElements(object obj)
        {
            // A dictionary created for the purpose of caching properties
            // that we need to recurse, for types that we've seen before.
            var props = new Dictionary<Type, List<PropertyInfo>>();
            return RecursiveGatherSubElementsInternal(obj, props, false);
        }

        private GatherSubElementsResult RecursiveGatherSubElementsInternal(object obj, Dictionary<Type, List<PropertyInfo>> properties, bool parentHasJsonIgnore)
        {
            GatherSubElementsResult result = new GatherSubElementsResult();

            if (obj == null)
            {
                return result;
            }

            var e = obj as Element;
            if (e != null && Elements.ContainsKey(e.Id))
            {
                // Do nothing. The Element has already
                // been added. This assumes that that the sub-elements
                // have been added as well and we don't need to continue.
                return result;
            }

            // This explicit loop is because we have mappings marked as internal so it's elements won't be automatically serialized.
            if (e != null)
            {
                foreach (var map in e.Mappings ?? new Dictionary<string, MappingBase>())
                {
                    if (!Elements.ContainsKey(map.Value.Id))
                    { result.Elements.Add(map.Value); }
                }
            }

            var sharedObject = obj as SharedObject;
            // if this shared object is already in the list, we don't need to process and add it again
            if (sharedObject != null)
            {
                if (SharedObjects.ContainsKey(sharedObject.Id))
                {
                    return result;
                }
            }

            var t = obj.GetType();

            // Ignore value types and strings
            // as they won't have properties that
            // could be elements.
            if (!t.IsClass || t == typeof(string))
            {
                return result;
            }

            List<PropertyInfo> constrainedProps;
            if (properties.ContainsKey(t))
            {
                constrainedProps = properties[t];
            }
            else
            {
                // This query had a nice little speed boost when we filtered for
                // valid types first then filtered for custom attributes.
                constrainedProps = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => IsValidForRecursiveAddition(p.PropertyType) &&
                    (p.GetCustomAttribute<JsonIgnoreAttribute>() == null || IsPropertyTypeRelatedToSharedObjects(p.PropertyType))).ToList();
                properties.Add(t, constrainedProps);
            }

            bool isTypeRelatedToSharedObjects = IsTypeRelatedToSharedObjects(t);
            foreach (var p in constrainedProps)
            {
                try
                {
                    var pValue = p.GetValue(obj, null);
                    if (pValue == null)
                    {
                        continue;
                    }

                    // Do not save shared object to the model if it is marked with JsonIgnore (e.g. ElementRepresentation)
                    bool hasJsonIgnore = parentHasJsonIgnore || p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Any();

                    if (pValue is IList elems)
                    {
                        foreach (var item in elems)
                        {
                            var subElements = RecursiveGatherSubElementsInternal(item, properties, hasJsonIgnore);
                            result.MergeSubResult(subElements, hasJsonIgnore, isTypeRelatedToSharedObjects);
                        }
                        continue;
                    }

                    // Get the properties dictionaries.
                    if (pValue is IDictionary dict)
                    {
                        foreach (var value in dict.Values)
                        {
                            var subElements = RecursiveGatherSubElementsInternal(value, properties, hasJsonIgnore);
                            result.MergeSubResult(subElements, hasJsonIgnore, isTypeRelatedToSharedObjects);
                        }
                        continue;
                    }

                    var gatheredSubElements = RecursiveGatherSubElementsInternal(pValue, properties, hasJsonIgnore);
                    result.MergeSubResult(gatheredSubElements, hasJsonIgnore, isTypeRelatedToSharedObjects);
                }
                catch (Exception ex)
                {
                    throw new Exception($"The {p.Name} property or one of its children was not valid for introspection. Check the inner exception for details.", ex);
                }
            }

            if (e != null)
            {
                result.Elements.Add(e);
            }

            if (sharedObject != null)
            {
                result.SharedObjects.Add(sharedObject);
            }

            return result;
        }

        /// <summary>
        /// Check whether a type is valid for introspection.
        /// TODO: When representations become elements, we should
        /// remove the inclusion for Representation, but keep that
        /// for SolidOperation.
        /// </summary>
        /// <param name="t">The type to check.</param>
        /// <returns>Return true if a type is valid for introspection, otherwise false.</returns>
        internal static bool IsValidForRecursiveAddition(Type t)
        {
            if (t.IsGenericType)
            {
                var genT = t.GetGenericArguments();
                if (genT.Length == 1)
                {
                    if (typeof(IList<>).MakeGenericType(genT[0]).IsAssignableFrom(t))
                    {
                        if (!IsValidListType(genT[0]))
                        {
                            return false;
                        }

                        return true;
                    }
                }
                else if (genT.Length == 2)
                {
                    if (typeof(IDictionary<,>).MakeGenericType(genT).IsAssignableFrom(t))
                    {
                        if (typeof(Element).IsAssignableFrom(genT[1]))
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }

            if (t.IsArray)
            {
                return typeof(Element).IsAssignableFrom(t.GetElementType());
            }

            return typeof(Element).IsAssignableFrom(t)
                   || typeof(Representation).IsAssignableFrom(t)
                   || typeof(SolidOperation).IsAssignableFrom(t)
                   || typeof(SharedObject).IsAssignableFrom(t)
               || typeof(RepresentationInstance).IsAssignableFrom(t);
        }

        private static bool IsTypeRelatedToSharedObjects(Type t)
        {
            return typeof(SharedObject).IsAssignableFrom(t)
                || typeof(RepresentationInstance).IsAssignableFrom(t);
        }

        private static bool IsPropertyTypeRelatedToSharedObjects(Type t)
        {
            if (t.IsGenericType)
            {
                var genT = t.GetGenericArguments();
                if (genT.Length == 1)
                {
                    if (typeof(IList<>).MakeGenericType(genT[0]).IsAssignableFrom(t))
                    {
                        if (!IsTypeRelatedToSharedObjects(genT[0]))
                        {
                            return false;
                        }

                        return true;
                    }
                }
                else if (genT.Length == 2)
                {
                    if (typeof(IDictionary<,>).MakeGenericType(genT).IsAssignableFrom(t))
                    {
                        if (IsTypeRelatedToSharedObjects(genT[1]))
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }

            if (t.IsArray)
            {
                return IsTypeRelatedToSharedObjects(t.GetElementType());
            }

            return IsTypeRelatedToSharedObjects(t);
        }

        private static bool IsValidListType(Type t)
        {
            return typeof(Element).IsAssignableFrom(t)
                   || typeof(SolidOperation).IsAssignableFrom(t)
                   || typeof(SharedObject).IsAssignableFrom(t)
               || typeof(RepresentationInstance).IsAssignableFrom(t);
        }
    }

    public static class ModelExtensions
    {
        /// <summary>
        /// Get all elements of a certain type from a specific model name in a dictionary of models.
        /// </summary>
        /// <param name="models">Dictionary of models keyed by string.</param>
        /// <param name="modelName">The name of the model.</param>
        /// <typeparam name="T">The type of element we want to retrieve.</typeparam>
        /// <returns></returns>
        public static List<T> AllElementsOfType<T>(this Dictionary<string, Model> models, string modelName) where T : Element
        {
            var elements = new List<T>();
            models.TryGetValue(modelName, out var model);
            if (model != null)
            {
                elements.AddRange(model.AllElementsOfType<T>());
            }
            return elements;
        }

        /// <summary>
        /// Get all proxies of a certain type from a specific model name in a dictionary of models.
        /// </summary>
        /// <param name="models">Dictionary of models keyed by string</param>
        /// <param name="modelName">The name of the model</param>
        /// <typeparam name="T">The type of element we want to retrieve</typeparam>
        /// <returns></returns>
        public static List<ElementProxy<T>> AllProxiesOfType<T>(this Dictionary<string, Model> models, string modelName) where T : Element
        {
            return models.AllElementsOfType<T>(modelName).Proxies(modelName).ToList();
        }
    }
}