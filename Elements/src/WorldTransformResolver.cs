using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Elements.Geometry;
using Newtonsoft.Json.Linq;

namespace Elements
{
    /// <summary>
    /// <para>
    /// Resolves the world transform of an element whose stored Transform may be
    /// relative to its Parent.
    /// </para>
    /// <para>
    /// Hypar's web editor stores some elements' Transform relative to their
    /// Parent element: a conceptual mass inside a building, a wall on a mass.
    /// The JSON does not say which ones, so this class ports the editor's rule
    /// (see <see cref="IsTransformRelativeToParent"/>) and walks the Parent
    /// chain, composing <c>local.Concatenated(parentWorld)</c> at each step.
    /// </para>
    /// <para>
    /// Not covered, so the result is not where the editor draws the element:
    /// </para>
    /// <list type="bullet">
    /// <item>Anchor frames. An element with an Anchor (see <see cref="HasAnchor"/>),
    /// such as a door or window placed in a wall, a component or a clearance, is
    /// stored relative to a point on the parent's rendered bounding box, not to
    /// the parent's origin. That box is not in the JSON, so this resolver
    /// composes the element against the parent's world transform.</item>
    /// <item>A column placed on two gridlines, which is stored relative to their
    /// intersection and has no Parent, so it reads as world.</item>
    /// <item>A multi-level stair or elevator, whose world Z the editor replaces
    /// with its base level's elevation.</item>
    /// </list>
    /// <para>
    /// <see cref="Model.Transform"/> is not applied.
    /// </para>
    /// <para>
    /// Build one resolver per model and reuse it: resolved transforms are cached.
    /// The cache assumes the model does not change while the resolver is in use.
    /// </para>
    /// </summary>
    public class WorldTransformResolver
    {
        /// <summary>The discriminator of a conceptual mass, relative to its parent whenever it has one.</summary>
        public const string ConceptualMassDiscriminator = "Elements.ConceptualMass";

        // THE RULE TABLE, ported from the Hypar web editor's rule for which stored
        // Transforms are relative to the Parent. A new relative type is one line here.
        // An element is relative only when it also has a Parent.

        /// <summary>Types whose Transform is relative to their Parent whenever they have one.</summary>
        private static readonly HashSet<string> RelativeWhenParented = new HashSet<string>
        {
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
            // A mass's Transform is relative to the Elements.Building group it belongs to.
            ConceptualMassDiscriminator,
        };

        /// <summary>Types that are relative only from a version on, read from a string property as the editor's parseInt does.</summary>
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
        private const string AnchorKey = "Anchor";

        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> TypedProperties =
            new ConcurrentDictionary<(Type, string), PropertyInfo>();

        private readonly Func<Guid, Element> _lookup;
        private readonly Dictionary<Guid, Transform> _worlds = new Dictionary<Guid, Transform>();
        // Elements resolved to their own Transform, with nothing above them composed in.
        private readonly HashSet<Guid> _roots = new HashSet<Guid>();

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
        /// <returns>A new transform, or null when there is no such element or it stores no Transform
        /// (a typed GeometricElement always has one; see <see cref="GetWorldTransform(Element)"/>).</returns>
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
        /// Parent loop, each element on the loop resolves to its own Transform,
        /// whatever order elements are resolved in. The editor instead drops only
        /// the link that closes the loop, chosen by the order it loads elements,
        /// so the other members can differ from where it draws them.
        /// For an element with an Anchor (<see cref="HasAnchor"/>) the result is
        /// not where the editor draws it.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A new transform, or null when the element stores no Transform.
        /// A typed GeometricElement always has one: a Transform missing from its
        /// JSON reads as identity.</returns>
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
                        _roots.Add(chain[i].Id);
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
                if (i == chain.Count - 1 && !aboveKnown)
                {
                    // The top of the chain: not relative, or its parent is missing.
                    world = local;
                    _roots.Add(chain[i].Id);
                }
                else if (local == null)
                {
                    world = null;
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
        /// Get the Transform to store on an element so that its world transform
        /// becomes <paramref name="world"/>: <c>world.Concatenated(parentWorld.Inverted())</c>
        /// when the element resolves against its parent, otherwise the world
        /// transform itself. Use it to write back a moved element that keeps
        /// its Parent. Resolved transforms are cached, so after writing, build a
        /// new resolver before resolving the element or its children again.
        /// Wrong for an element with an Anchor (<see cref="HasAnchor"/>), which
        /// the editor stores against an anchor point instead.
        /// </summary>
        /// <param name="element">The element to store the transform on.</param>
        /// <param name="world">The world transform the element should have.</param>
        /// <returns>A new transform.</returns>
        public Transform ToLocalTransform(Element element, Transform world)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }
            GetWorldTransform(element);
            if (_roots.Contains(element.Id))
            {
                return new Transform(world);
            }
            var parentWorld = _worlds[GetParentId(element).Value] ?? new Transform();
            return world.Concatenated(parentWorld.Inverted());
        }

        /// <summary>
        /// Whether an element's stored Transform is relative to its Parent.
        /// True only when the element has a Parent and its discriminator is one the
        /// editor stores relative:
        /// <list type="bullet">
        /// <item>Always: <c>Elements.ConceptualMass</c>, <c>Elements.Annotations.Anchor</c>,
        /// <c>Elements.Clearance</c>, <c>Elements.Circulation</c>, <c>Elements.PlanningGrid</c>,
        /// <c>Elements.CadComponent</c>, <c>Elements.Component</c>, <c>Elements.FlexibleComponent</c>,
        /// <c>Elements.Door</c>, <c>Elements.Window</c>, <c>Elements.Floor</c>, <c>Elements.Roof</c>,
        /// <c>Elements.Stair</c> and <c>Elements.Elevator</c>.</item>
        /// <item>From a version on: <c>Elements.StandardWall</c> when <c>WallsVersion</c> is 4 or more,
        /// <c>Elements.GridLine</c> when <c>GridlinesVersion</c> is 2 or more, and
        /// <c>Elements.Column</c> when <c>ColumnsVersion</c> is 2 or more. The version is read
        /// only from a string, by its leading digits.</item>
        /// </list>
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
                // The editor's rule reads the version only when it is a string, with parseInt.
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
        /// <returns>The stored transform, or null when the element stores none. A
        /// typed GeometricElement always has one: a Transform missing from its JSON
        /// reads as identity.</returns>
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

        /// <summary>
        /// Whether an element has an Anchor, read from a typed Anchor property or
        /// from AdditionalProperties. The editor stores such an element relative
        /// to a point on its parent's rendered bounding box, which this resolver
        /// cannot see, so its resolved transform is not where the editor draws it.
        /// </summary>
        /// <param name="element">The element.</param>
        public static bool HasAnchor(Element element)
        {
            return element != null && HasValue(element, AnchorKey);
        }

        private static bool HasParent(Element element)
        {
            return HasValue(element, ParentKey);
        }

        private static bool HasValue(Element element, string name)
        {
            // As the editor's `!element.Parent`: null and the empty string are no value.
            if (!TryGetProperty(element, name, out var value) || value == null)
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
            var property = TypedProperties.GetOrAdd((element.GetType(), name), key => MostDerivedProperty(key.Item1, key.Item2));
            if (property != null)
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

        /// <summary>
        /// The public instance property nearest the most derived type. A function
        /// that declares its own type, for example a ConceptualMass whose
        /// `new Transform Transform` hides GeometricElement.Transform, keeps the
        /// stored value in the hiding property, and Type.GetProperty can throw
        /// AmbiguousMatchException on it.
        /// </summary>
        private static PropertyInfo MostDerivedProperty(Type type, string name)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                var property = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null && property.GetGetMethod() != null && property.GetIndexParameters().Length == 0)
                {
                    return property;
                }
            }
            return null;
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
