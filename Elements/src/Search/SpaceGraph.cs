using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// A space graph.
    /// </summary>
    public class SpaceGraph
    {
        /// <summary>
        /// The root node of the graph.
        /// </summary>
        public BaseNode Root { get; set; }

        /// <summary>
        /// Create a space graph from a model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="alongTolerance"></param>
        public static SpaceGraph FromModel(Model model, double alongTolerance = 1.0)
        {
            var graph = new SpaceGraph();
            var root = new RootNode();
            graph.Root = root;

            // We work from the leaves backwards, merging nodes as we go.
            // Each merge stage removes elements from the list of elements to merge.

            var elements = model.AllElementsAssignableFromType<GeometricElement>().Where(e => !(e is StandardWall)).ToList();

            var freeSupportNodes = MergeSupportingNodes(elements);
            var freeAroundNodes = MergeAroundNodes(freeSupportNodes, elements);
            MergeAlongNodes(root, model, elements, freeSupportNodes, freeAroundNodes);

            return graph;
        }

        private static void MergeAlongNodes(RootNode root, Model model,
                                                                   List<GeometricElement> freeElements,
                                                                   Dictionary<GeometricElement, SupportNode> freeSupportNodes,
                                                                   Dictionary<GeometricElement, AroundNode> freeAroundNodes,
                                                                   double alongTolerance = 1.0)
        {
            var wallPlanes = new Dictionary<StandardWall, Plane>();
            var walls = model.AllElementsOfType<StandardWall>();
            if (walls.Count() == 0)
            {
                return;
            }

            foreach (var wall in walls)
            {
                wallPlanes.Add(wall, new Plane(wall.CenterLine.Start, wall.CenterLine.Direction().Cross(Vector3.ZAxis)));
            }

            // TODO: Multiple groupings of elements along the same wall.

            var elements = freeElements.Concat(freeSupportNodes.Keys).Concat(freeAroundNodes.Keys).Distinct().ToList();

            // Group objects according to their proximity to walls.
            var wallGroupedElements = elements.GroupBy(e =>
            {
                // StandardWall closestWall = null;
                // var minDistance = double.MaxValue;
                // foreach (var wall in walls)
                // {
                //     var distance = Math.Abs(e.Transform.Origin.DistanceTo(wallPlanes[wall]));
                //     if (distance < alongTolerance && distance < minDistance)
                //     {
                //         closestWall = wall;
                //         minDistance = distance;
                //     }
                // }
                // return closestWall;
                return walls.OrderBy(w => Math.Abs(e.Transform.Origin.DistanceTo(wallPlanes[w]))).First();
            });

            // Create leaf along relationships for leaf elements.
            foreach (var wall in walls)
            {
                var wallGroup = wallGroupedElements.FirstOrDefault(g => g.Key == wall);
                if (wallGroup == null)
                {
                    root.Children.Add(new ElementNode(wall));
                    continue;
                }

                if (wallGroup.Count() == 0)
                {
                    // For walls with no close elements, add a leaf node.
                    root.Children.Add(new ElementNode(wallGroup.Key));
                    continue;
                }

                // Create an along node for the wall.
                var alongNode = new AlongNode(wallGroup.Key, new List<BaseNode>());

                if (wallGroup.Count() == 1)
                {
                    alongNode.Children.Add(FindNodeOrCreateLeafForElement(wallGroup.First(), freeSupportNodes, freeAroundNodes, freeElements));
                }
                else
                {
                    var groupNode = new GroupNode(wallGroup.Select(o => FindNodeOrCreateLeafForElement(o, freeSupportNodes, freeAroundNodes, freeElements)).ToList());
                    alongNode.Children.Add(groupNode);
                }

                // Calculate the average of all element's origins along the wall.
                var average = new BBox3(alongNode.GatherChildElements().Select(e => e.Transform.Origin).ToList()).Center();
                var parameter = average.ClosestPointOn(wall.CenterLine).DistanceTo(wall.CenterLine.Start) / wall.CenterLine.Length();
                alongNode.Parameter = parameter;

                root.Children.Add(alongNode);
            }
        }

        private static BaseNode FindNodeOrCreateLeafForElement(GeometricElement element,
                                                Dictionary<GeometricElement, SupportNode> freeSupportNodes,
                                                Dictionary<GeometricElement, AroundNode> freeAroundNodes,
                                                List<GeometricElement> freeElements)
        {
            if (freeSupportNodes.ContainsKey(element))
            {
                return freeSupportNodes[element];
            }
            else if (freeAroundNodes.ContainsKey(element))
            {
                return freeAroundNodes[element];
            }
            else
            {
                freeElements.Remove(element);
                return new ElementNode(element);
            }
        }

        private static Dictionary<GeometricElement, AroundNode> MergeAroundNodes(Dictionary<GeometricElement, SupportNode> freeSupportingNodes,
                                                                                   List<GeometricElement> elements, double tolerance = 2.0)
        {
            var aroundNodes = new Dictionary<GeometricElement, AroundNode>();

            // Supporting elements can be a
            var targetElements = elements.Concat(freeSupportingNodes.Keys).ToList();

            for (var i = targetElements.Count - 1; i >= 0; i--)
            {
                var element = targetElements[i];

                // Get all elements that are within the tolerance of the target element,
                // and are not already part of an around node, excluding the target element.
                var otherElements = targetElements.Where(e => e != element
                && e.Transform.Origin.DistanceTo(element.Transform.Origin) < tolerance
                && !aroundNodes.Any(a => a.Value.GatherChildElements().Contains(e))).ToList();

                if (otherElements.Count == 0)
                {
                    continue;
                }

                // Group the elements by name. These are the groupings
                // of elements that are around the target element.
                var nameGroups = otherElements.GroupBy(e => e.Name);
                foreach (var nameGroup in nameGroups)
                {
                    if (nameGroup.Count() < 2)
                    {
                        continue;
                    }

                    var childNodes = new List<BaseNode>();
                    foreach (var childElement in nameGroup)
                    {
                        if (freeSupportingNodes.ContainsKey(childElement))
                        {
                            childNodes.Add(freeSupportingNodes[childElement]);

                            // Remove the supporting node from the list of free
                            // supporting nodes so that it can't be considered again.
                            freeSupportingNodes.Remove(childElement);
                        }
                        else
                        {
                            childNodes.Add(new ElementNode(childElement));
                            elements.Remove(childElement);
                        }
                    }
                    var aroundNode = new AroundNode(element, childNodes);
                    aroundNodes.Add(element, aroundNode);

                    // Remove the element from the list of target elements
                    // so that it can't be considered as a target element.
                    elements.Remove(element);
                }
            }
            return aroundNodes;
        }

        private static Dictionary<GeometricElement, SupportNode> MergeSupportingNodes(List<GeometricElement> elements)
        {
            var supportingNodes = new Dictionary<GeometricElement, SupportNode>();

            for (var i = elements.Count - 1; i >= 0; i--)
            {
                var element = elements[i];

                for (var j = elements.Count - 1; j >= 0; j--)
                {
                    var innerElement = elements[j];
                    if (element == innerElement)
                    {
                        continue;
                    }

                    if (IsSupportedBy(innerElement, element))
                    {
                        if (supportingNodes.ContainsKey(element))
                        {
                            supportingNodes[element].Children.Add(new ElementNode(innerElement));
                        }
                        else
                        {
                            var supportNode = new SupportNode(element, new List<GeometricElement> { innerElement });
                            supportingNodes.Add(element, supportNode);
                        }

                        // Remove the inner element from the list of elements.
                        // so that it can't be considered supported by another element.
                        elements.Remove(innerElement);
                    }
                }

                if (supportingNodes.ContainsKey(element))
                {
                    // The element is now a supporting element so it
                    // shouldn't be considered as a an unsupported element.
                    elements.Remove(element);
                }
            }
            return supportingNodes;
        }

        /// <summary>
        /// Print the graph.
        /// </summary>
        public override string ToString()
        {
            return Root.ToString();
        }

        /// <summary>
        /// Write the graph to a dot file.
        /// </summary>
        public void ToDot(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph {");
            var ids = new Dictionary<Guid, string>();
            Root.ToDot(sb, ids);
            sb.AppendLine("}");
            File.WriteAllText(path, sb.ToString());
        }

        private static bool IsSupportedBy(GeometricElement element, GeometricElement supportingElement)
        {
            var bounds1 = element.Bounds;
            var boundsT = new Transform(new Vector3(element.Transform.Origin.X, element.Transform.Origin.Y), element.Transform.XAxis, element.Transform.ZAxis);
            var boundsPoly = Polygon.Rectangle(new Vector3(bounds1.Min.X, bounds1.Min.Y), new Vector3(bounds1.Max.X, bounds1.Max.Y)).TransformedPolygon(boundsT);

            var bounds2 = supportingElement.Bounds;
            var boundsT2 = new Transform(new Vector3(supportingElement.Transform.Origin.X, supportingElement.Transform.Origin.Y), supportingElement.Transform.XAxis, supportingElement.Transform.ZAxis);
            var boundsPoly2 = Polygon.Rectangle(new Vector3(bounds2.Min.X, bounds2.Min.Y), new Vector3(bounds2.Max.X, bounds2.Max.Y)).TransformedPolygon(boundsT2);

            // The bottom and top surfaces of the bounding boxes are coplanar
            // and either the supported element's polygon is contained in the support element's polygon
            // or the polygons intersect.
            var supportedMinZ = element.Transform.OfPoint(bounds1.Min).Z;
            var supportingMaxZ = supportingElement.Transform.OfPoint(bounds2.Max).Z;
            return supportedMinZ.ApproximatelyEquals(supportingMaxZ) && (boundsPoly2.Contains(boundsPoly) || boundsPoly2.Intersects(boundsPoly));
        }
    }

    /// <summary>
    /// A node that represents the root of a space graph.
    /// </summary>
    public abstract class BaseNode
    {
        /// <inheritdoc/>
        public List<BaseNode> Children { get; set; } = new List<BaseNode>();

        /// <summary>
        /// The name of the node.
        /// </summary>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// The node's id.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Construct a base node.
        /// </summary>
        public BaseNode()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Print the node.
        /// </summary>
        public virtual string ToString(int indent)
        {
            var sb = new StringBuilder();
            sb.AppendLine(new string(' ', indent) + GetType().Name);
            foreach (var child in Children)
            {
                sb.AppendLine(child.ToString(indent + 2));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Write the node to a dot file.
        /// </summary>
        public void ToDot(StringBuilder sb, Dictionary<Guid, string> ids, BaseNode parent = null)
        {
            if (!ids.ContainsKey(Id))
            {
                ids.Add(Id, Name);
                sb.AppendLine($"\t\"{Id}\" [label=\"{Name}\"]");
            }

            if (parent != null)
            {
                sb.AppendLine($"\t\"{parent.Id}\" -> \"{Id}\"");
            }

            foreach (var child in Children)
            {
                child.ToDot(sb, ids, this);
            }
        }

        /// <summary>
        /// Print the node.
        /// </summary>
        public override string ToString()
        {
            return ToString(0);
        }

        /// <summary>
        /// Gather all elements that are children of this node.
        /// </summary>
        public List<GeometricElement> GatherChildElements(List<GeometricElement> elements = null)
        {
            if (elements == null)
            {
                elements = new List<GeometricElement>();
            }

            if (this is ElementNode elementNode)
            {
                elements.Add(elementNode.Element);
            }

            foreach (var child in Children)
            {
                child.GatherChildElements(elements);
            }
            return elements;
        }

        /// <summary>
        /// Gather all nodes that are children of this node.
        /// </summary>
        public List<BaseNode> GatherChildNodes(List<BaseNode> nodes = null)
        {
            if (nodes == null)
            {
                nodes = new List<BaseNode>();
            }

            foreach (var child in Children)
            {
                nodes.Add(child);
                child.GatherChildNodes(nodes);
            }
            return nodes;
        }
    }

    /// <summary>
    /// The root node.
    /// </summary>
    public class RootNode : BaseNode
    {
        /// <inheritdoc/>
        public override string Name => "Root";
    }

    /// <summary>
    /// A node that represents a supporting relationship.
    /// The first child is the supporting object.
    /// The additional children are the supported objects.
    /// </summary>
    public class SupportNode : BaseNode
    {
        /// <summary>
        /// Construct a support node.
        /// </summary>
        public SupportNode(GeometricElement supportingElement, List<GeometricElement> supportedElements)
        {
            Children.Add(new ElementNode(supportingElement));
            Children.AddRange(supportedElements.Select(e => new ElementNode(e)));
        }
    }

    /// <summary>
    /// A node that represents an object or object grouping
    /// along a wall. The first child is the wall. The additional children
    /// are elements or groups of elements.
    /// </summary>
    public class AlongNode : BaseNode
    {
        /// <summary>
        /// The normalized parameter along the wall.
        /// </summary>
        public double Parameter { get; set; }

        /// <summary>
        /// Construct an along node.
        /// </summary>
        public AlongNode(StandardWall wall, List<BaseNode> elements)
        {
            Children.Add(new ElementNode(wall));
            Children.AddRange(elements);
        }
    }

    /// <summary>
    /// A node that represents a group of elements.
    /// </summary>
    public class GroupNode : BaseNode
    {
        /// <summary>
        /// Construct a group node.
        /// </summary>
        public GroupNode()
        {

        }

        /// <summary>
        /// Construct a group node.
        /// </summary>
        public GroupNode(List<BaseNode> nodes)
        {
            // Order the nodes by their volume.
            Children.AddRange(nodes.OrderBy(n => n.GatherChildElements().Max(e => e.Bounds.Volume)));
        }
    }

    /// <summary>
    /// A node that represents elements of a similar 
    /// type that are arrayed around another element
    /// like chairs around a table. The first child is the 
    /// target. The rest of the children are the elements
    /// arrayed around the target.
    /// </summary>
    public class AroundNode : BaseNode
    {
        /// <summary>
        /// Construct an around node.
        /// </summary>
        public AroundNode(GeometricElement target, List<BaseNode> nodes)
        {
            Children.Add(new ElementNode(target));
            Children.AddRange(nodes);
        }
    }

    /// <summary>
    /// A node that represents a leaf element.
    /// </summary>
    public class ElementNode : BaseNode
    {
        /// <inheritdoc/>
        public override string Name => Element.Name ?? Element.GetType().Name;

        /// <summary>
        /// The leaf element.
        /// </summary>
        public GeometricElement Element { get; set; }

        /// <summary>
        /// Construct an element node.
        /// </summary>
        public ElementNode(GeometricElement element)
        {
            Element = element;
        }

        /// <summary>
        /// Print the node.
        /// </summary>
        public override string ToString(int indent)
        {
            return new string(' ', indent) + Element.Name;
        }
    }
}