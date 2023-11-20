
using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Components;
using Elements.Geometry;

namespace Elements.Components
{
    /// <summary>
    /// A keyed collection of named content configurations
    /// </summary>
    public class SpaceConfiguration : Dictionary<string, ContentConfiguration>
    {
        internal static Dictionary<string, ContentElement> contentDict = new Dictionary<string, ContentElement>();
    }

    /// <summary>
    /// A collection of content items with a specified boundary.
    /// </summary>
    public class ContentConfiguration
    {

        /// <summary>
        /// A rectangular boundary around this content configuration.
        /// </summary>
        public class BoundaryDefinition
        {
            /// <summary>
            /// The minimum point of this boundary definition.
            /// </summary>
            public Vector3 Min { get; set; }

            /// <summary>
            /// The maximum point of this boundary definition.
            /// </summary>
            public Vector3 Max { get; set; }

            /// <summary>
            /// The calculated width of this boundary definition.
            /// </summary>
            public double Width => Max.X - Min.X;

            /// <summary>
            /// The calculated depth of this boundary definition.
            /// </summary>
            public double Depth => Max.Y - Min.Y;
        }

        /// <summary>
        /// A reference .
        /// </summary>
        public class ContentItem
        {
            /// <summary>
            /// The url to the content GLB.
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// The name of this content item.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The transform of this content item, relative to its anchor.
            /// </summary>
            public Transform Transform { get; set; }

            /// <summary>
            /// The reference position this content item should move with.
            /// </summary>

            public Vector3 Anchor { get; set; }

            /// <summary>
            /// Fetch the matching content element for this item from a catalog.
            /// </summary>
            private ContentElement MakeContentElement()
            {
                var matchingContentElement = ContentCatalogRetrieval.GetCatalog().Content.FirstOrDefault(s =>
                {
                    if (Name != null)
                    {
                        return s.Name == Name && s.GltfLocation == Url;
                    }
                    return s.GltfLocation == Url;
                });
                if (matchingContentElement == null)
                {
                    matchingContentElement = new ContentElement(Url, new BBox3(new Vector3(0, 0, 0), new Vector3(1, 1, 1)), 1, new Vector3(0, 1, 0), new Transform(), defaultMaterial, null, true, Guid.NewGuid(), Name, "{}");
                }
                matchingContentElement.Material = defaultMaterial;
                matchingContentElement.Transform = new Transform();
                return matchingContentElement;
            }

            /// <summary>
            /// The content element corresponding to this item.
            /// </summary>
            [Newtonsoft.Json.JsonIgnore]
            public ContentElement ContentElement
            {
                get
                {
                    if (!SpaceConfiguration.contentDict.ContainsKey(this.Url))
                    {
                        SpaceConfiguration.contentDict[this.Url] = MakeContentElement();
                    }
                    return SpaceConfiguration.contentDict[this.Url];
                }
            }
        }

        /// <summary>
        /// The definition of this configuration's boundary.
        /// </summary>
        public BoundaryDefinition CellBoundary { get; set; }

        /// <summary>
        /// The individual content itmems in this configuration.
        /// </summary>

        public List<ContentItem> ContentItems { get; set; }

        /// <summary>
        /// The width of the configuration boundary.
        /// </summary>
        public double Width => this.CellBoundary.Width;

        /// <summary>
        /// The depth of the configuration boundary.
        /// </summary>
        public double Depth => this.CellBoundary.Depth;

        /// <summary>
        /// Allow rotation of the configuration
        /// </summary>
        public bool AllowRotation { get; set; }

        /// <summary>
        /// Create a set of element instances from this configuration.
        /// </summary>
        /// <param name="t">The transform to apply to the configuration.</param>
        public List<ElementInstance> Instantiate(Transform t)
        {
            var instances = new List<ElementInstance>();
            for (int i = 0; i < ContentItems.Count; i++)
            {
                var definition = ContentItems[i].ContentElement;
                var instance = definition.CreateInstance(ContentItems[i].Transform.Concatenated(t), null);
                instances.Add(instance);
            }
            return instances;
        }

        /// <summary>
        /// Generate a consistent set of anchors from a rectangular polygon — corners, midpoints, and center.
        /// </summary>
        /// <param name="polygon">The polygon from which to generate anchors.</param>
        public static List<Vector3> AnchorsFromRect(Polygon polygon)
        {
            var anchors = new List<Vector3>();
            anchors.AddRange(polygon.Vertices);
            anchors.AddRange(polygon.Segments().Select(s => s.Mid()));
            anchors.Add(polygon.Centroid());
            return anchors;
        }

        /// <summary>
        /// The anchors for this configuration's boundary.
        /// </summary>
        public List<Vector3> Anchors()
        {
            var baseRect = Polygon.Rectangle(this.CellBoundary.Min, this.CellBoundary.Max);
            var baseAnchors = AnchorsFromRect(baseRect);
            return baseAnchors;
        }

        /// <summary>
        /// Generate a set of Component Placement Rules for placing instances of the items in this configuration.
        /// </summary>
        public List<IComponentPlacementRule> Rules()
        {
            var baseAnchors = Anchors();
            var rules = new List<IComponentPlacementRule>();
            foreach (var contentItem in this.ContentItems)
            {
                var anchor = contentItem.Anchor;
                var anchorIndex = Enumerable.Range(0, baseAnchors.Count).OrderBy(a => baseAnchors[a].DistanceTo(anchor)).First();
                var closestAnchor = baseAnchors[anchorIndex];
                var element = contentItem.ContentElement;
                var offsetXform = contentItem.Transform.Concatenated(new Transform(anchor.Negate())).Concatenated(new Transform(anchor - closestAnchor));
                rules.Add(new PositionPlacementRule(contentItem.Name ?? contentItem.Url, anchorIndex, element, offsetXform));
            }
            return rules;
        }

        /// <summary>
        /// Default material for content display.
        /// </summary>
        public static Material defaultMaterial = new Material("Default Material", new Color(0.9, 0.9, 0.9, 1.0));

    }
}