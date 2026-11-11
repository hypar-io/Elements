using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Elements.Geometry;
using Newtonsoft.Json.Linq;

namespace Elements
{
    /// <summary>
    /// Resolves the world transform of an element whose stored Transform may be
    /// relative to its Parent.
    ///
    /// Hypar's web editor stores some elements' Transform relative to their
    /// Parent element: a conceptual mass inside a building, a wall on a mass, a
    /// door on that wall. The JSON does not say which ones, so this class
    /// ports the editor's rule (see <see cref="IsTransformRelativeToParent"/>)
    /// and walks the Parent chain, composing
    /// <c>local.Concatenated(parentWorld)</c> at each step.
    ///
    /// Not covered: anchor frames. The editor composes some children
    /// (components, CAD components, clearances, flexible components,
    /// annotation anchors) against a UVW anchor on the parent's rendered
    /// bounding box rather than the parent's world transform. That box is not
    /// in the JSON, so this resolver composes those children against the
    /// parent's world transform.
    ///
    /// <see cref="Model.Transform"/> is not applied.
    ///
    /// Build one resolver per model and reuse it: resolved transforms are cached.
    /// The cache assumes the model does not change while the resolver is in use.
    /// </summary>
    public class WorldTransformResolver
    {
        /// <summary>The discriminator of a conceptual mass, relative to its parent whenever it has one.</summary>
        public const string ConceptualMassDiscriminator = "Elements.ConceptualMass";

        // THE RULE TABLE. Source of truth: the Hypar web editor (pringle), origin/main:
        //   src/elements/registries/resolution/levelResolution.ts  transformIsRelativeToParent
        //   src/elements/registries/ElementPropertiesRegistry.ts    transformIsRelative defaults
        //   src/elements/elementTypes/Building.ts                   a ConceptualMass with a Parent is relative
        //   src/elements/packages/packageList.ts                    multi-level package kinds (stair, elevator)
        // A new relative type is one line here. An element is relative only when it
        // also has a Parent.

        /// <summary>Types whose Transform is relative to their Parent whenever they have one.</summary>
        private static readonly HashSet<string> RelativeWhenParented = new HashSet<string>
        {
            // ElementPropertiesRegistry.ts defaults.
            "Elements.Annotations.Anchor",
            "Elements.Clearance",
            "Elements.Circulation",
            "Elements.PlanningGrid",
            "Elements.CadComponent",
            "Elements.Component",
            "Elements.Door",
            "Elements.FlexibleComponent",
            "Elements.Window",
            "Elements.Floor",
            "Elements.Roof",
            // Multi-level package kinds.
            "Elements.Stair",
            "Elements.Elevator",
            // Building.ts: a mass's Transform is relative to the Elements.Building group it belongs to.
            ConceptualMassDiscriminator,
        };

        /// <summary>Types that are relative only from a version on, read from a string property as pringle's parseInt does.</summary>
        private static readonly Dictionary<string, (string Property, int MinimumVersion)> RelativeFromVersion =
            new Dictionary<string, (string, int)>
            {
                { "Elements.StandardWall", ("WallsVersion", 4) },
                { "Elements.GridLine", ("GridlinesVersion", 2) },
                { "Elements.Column", ("ColumnsVersion", 2) },
            };

        private const string DiscriminatorKey = "discriminator";
        private const string ParentKey = "Parent";
        private const string TransformKey = "Transform";

        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> TypedProperties =
            new ConcurrentDictionary<(Type, string), PropertyInfo>();

        private readonly Func<Guid, Element> _lookup;
        private readonly Dictionary<Guid, Transform> _worlds = new Dictionary<Guid, Transform>();

        /// <summary>
        /// Create a resolver over a model's elements.
        /// </summary>
        /// <param name="model">The model whose elements the Parent ids refer to.</param>
        public WorldTransformResolver(Model model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            _lookup = id => model.Elements.TryGetValue(id, out var element) ? element : null;
        }

        /// <summary>
        /// Create a resolver over any element lookup.
        /// </summary>
        /// <param name="lookup">Returns the element with an id, or null when there is none.</param>
        public WorldTransformResolver(Func<Guid, Element> lookup)
        {
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
        }

        /// <summary>
        /// Get the world transform of the element with an id.
        /// </summary>
        /// <param name="id">The element's id.</param>
        /// <returns>A new transform, or null when there is no such element or it stores no Transform.</returns>
        public Transform GetWorldTransform(Guid id)
        {
            var element = _lookup(id);
            return element == null ? null : GetWorldTransform(element);
        }

        /// <summary>
        /// Get an element's world transform: its stored Transform composed with
        /// its Parent's world transform when the stored Transform is relative.
        /// A missing parent leaves the element's own Transform, as the editor
        /// does. A parent that stores no Transform composes as identity. In a
        /// Parent loop, each element on the loop resolves to its own Transform.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A new transform, or null when the element stores no Transform.</returns>
        public Transform GetWorldTransform(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            // Walk up the chain to the first element whose world is known or is
            // its own Transform, then fold back down.
            var chain = new List<Element>();
            var positions = new Dictionary<Guid, int>();
            var current = element;
            Transform above = null;
            var aboveKnown = false;
            while (true)
            {
                if (_worlds.TryGetValue(current.Id, out var cached))
                {
                    above = cached;
                    aboveKnown = true;
                    break;
                }
                if (positions.TryGetValue(current.Id, out var loopStart))
                {
                    // A Parent loop: its members resolve to their own Transform.
                    for (var i = loopStart; i < chain.Count; i++)
                    {
                        _worlds[chain[i].Id] = GetLocalTransform(chain[i]);
                    }
                    above = _worlds[chain[loopStart].Id];
                    aboveKnown = true;
                    chain.RemoveRange(loopStart, chain.Count - loopStart);
                    break;
                }
                positions[current.Id] = chain.Count;
                chain.Add(current);

                var parent = IsTransformRelativeToParent(current) ? FindParent(current) : null;
                if (parent == null)
                {
                    break;
                }
                current = parent;
            }

            for (var i = chain.Count - 1; i >= 0; i--)
            {
                var local = GetLocalTransform(chain[i]);
                Transform world;
                if (local == null || (i == chain.Count - 1 && !aboveKnown))
                {
                    // The top of the chain is a root, or the element stores no Transform.
                    world = local;
                }
                else
                {
                    world = local.Concatenated(above ?? new Transform());
                }
                _worlds[chain[i].Id] = world;
                above = world;
            }

            var result = _worlds[element.Id];
            return result == null ? null : new Transform(result);
        }

        /// <summary>
        /// Whether an element's stored Transform is relative to its Parent.
        /// True only when the element has a Parent and its type is one the
        /// editor stores relative (see the rule table in this file).
        /// </summary>
        /// <param name="element">The element.</param>
        public static bool IsTransformRelativeToParent(Element element)
        {
            if (element == null || !HasParent(element))
            {
                return false;
            }
            var discriminator = GetDiscriminator(element);
            if (discriminator == null)
            {
                return false;
            }
            if (RelativeFromVersion.TryGetValue(discriminator, out var versioned))
            {
                // Pringle reads the version only when it is a string, with parseInt.
                return TryGetProperty(element, versioned.Property, out var value)
                    && StringValue(value) is string version
                    && TryParseLeadingInt(version, out var number)
                    && number >= versioned.MinimumVersion;
            }
            return RelativeWhenParented.Contains(discriminator);
        }

        /// <summary>
        /// Get the id in an element's Parent, read from a typed Parent property
        /// or from AdditionalProperties.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The parent's id, or null when there is no Parent or it is not an id.</returns>
        public static Guid? GetParentId(Element element)
        {
            if (element == null || !TryGetProperty(element, ParentKey, out var value) || value == null)
            {
                return null;
            }
            switch (value)
            {
                case Element parent:
                    return parent.Id;
                case Guid id:
                    return id;
                case JValue jValue when jValue.Value is Guid jId:
                    return jId;
            }
            return Guid.TryParse(StringValue(value), out var parsed) ? parsed : (Guid?)null;
        }

        /// <summary>
        /// Get an element's stored Transform, as written, from a typed Transform
        /// property or from AdditionalProperties.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The stored transform, or null when the element stores none.</returns>
        public static Transform GetLocalTransform(Element element)
        {
            if (element == null || !TryGetProperty(element, TransformKey, out var value))
            {
                return null;
            }
            switch (value)
            {
                case Transform transform:
                    return transform;
                case JObject jObject:
                    try
                    {
                        return jObject.ToObject<Transform>();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
            }
            return null;
        }

        /// <summary>
        /// Get an element's discriminator: the one it was read with, kept in
        /// AdditionalProperties when its type was not recognized, or else its
        /// type's full name.
        /// </summary>
        /// <param name="element">The element.</param>
        public static string GetDiscriminator(Element element)
        {
            if (element == null)
            {
                return null;
            }
            if (element.AdditionalProperties != null
                && element.AdditionalProperties.TryGetValue(DiscriminatorKey, out var value)
                && StringValue(value) is string discriminator)
            {
                return discriminator;
            }
            return element.GetType().FullName;
        }

        private Element FindParent(Element element)
        {
            var parentId = GetParentId(element);
            return parentId.HasValue ? _lookup(parentId.Value) : null;
        }

        private static bool HasParent(Element element)
        {
            // Pringle tests `!element.Parent`: null and the empty string are no parent.
            if (!TryGetProperty(element, ParentKey, out var value) || value == null)
            {
                return false;
            }
            if (value is JValue jValue)
            {
                value = jValue.Value;
            }
            return value != null && !(value is string text && text.Length == 0);
        }

        /// <summary>A typed public property wins; otherwise AdditionalProperties.</summary>
        private static bool TryGetProperty(Element element, string name, out object value)
        {
            var property = TypedProperties.GetOrAdd((element.GetType(), name), key =>
            {
                try
                {
                    return key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance);
                }
                catch (AmbiguousMatchException)
                {
                    // A derived type hides the property with `new`; take the most derived one.
                    return key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }
            });
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                value = property.GetValue(element);
                return true;
            }
            if (element.AdditionalProperties != null && element.AdditionalProperties.TryGetValue(name, out value))
            {
                return true;
            }
            value = null;
            return false;
        }

        private static string StringValue(object value)
        {
            if (value is string text)
            {
                return text;
            }
            if (value is JValue jValue && jValue.Type == JTokenType.String)
            {
                return (string)jValue.Value;
            }
            return null;
        }

        /// <summary>JavaScript's parseInt(text, 10): optional whitespace and sign, then leading digits.</summary>
        private static bool TryParseLeadingInt(string text, out int number)
        {
            number = 0;
            var i = 0;
            while (i < text.Length && char.IsWhiteSpace(text[i]))
            {
                i++;
            }
            var sign = 1;
            if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            {
                sign = text[i] == '-' ? -1 : 1;
                i++;
            }
            var start = i;
            long accumulated = 0;
            while (i < text.Length && text[i] >= '0' && text[i] <= '9')
            {
                accumulated = Math.Min(accumulated * 10 + (text[i] - '0'), int.MaxValue);
                i++;
            }
            if (i == start)
            {
                return false;
            }
            number = (int)(sign * accumulated);
            return true;
        }
    }

    /// <summary>
    /// World transform conveniences on a model.
    /// </summary>
    public static class ModelWorldTransformExtensions
    {
        /// <summary>
        /// Get an element's world transform, composing Parent-relative
        /// transforms as <see cref="WorldTransformResolver"/> does. To resolve
        /// many elements, build one <see cref="WorldTransformResolver"/> and reuse it.
        /// </summary>
        /// <param name="model">The model holding the element's parents.</param>
        /// <param name="element">The element.</param>
        /// <returns>A new transform, or null when the element stores no Transform.</returns>
        public static Transform GetWorldTransform(this Model model, Element element)
        {
            return new WorldTransformResolver(model).GetWorldTransform(element);
        }
    }
}
