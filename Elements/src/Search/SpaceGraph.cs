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
        /// Create a space graph from room elements.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="alongTolerance"></param>
        public static SpaceGraph FromModel(Model model, double alongTolerance = 1.0)
        {
            // Create with relationships
            var graph = new SpaceGraph();
            var root = new RootNode();
            graph.Root = root;

            var wallPlanes = new Dictionary<StandardWall, Plane>();
            var walls = model.AllElementsOfType<StandardWall>();
            if (walls.Count() == 0)
            {
                return null;
            }

            foreach (var wall in walls)
            {
                wallPlanes.Add(wall, new Plane(wall.CenterLine.Start, wall.CenterLine.Direction().Cross(Vector3.ZAxis)));
            }

            var elements = model.AllElementsAssignableFromType<GeometricElement>().Where(e => !(e is StandardWall));

            // TODO: Multiple groupings of elements along the same wall.
            // Group objects according to their proximity to walls.
            var wallGroupedElements = elements.GroupBy(e =>
            {
                StandardWall closestWall = null;
                var minDistance = double.MaxValue;
                foreach (var wall in walls)
                {
                    var distance = Math.Abs(e.Transform.Origin.DistanceTo(wallPlanes[wall]));
                    if (distance < alongTolerance && distance < minDistance)
                    {
                        closestWall = wall;
                        minDistance = distance;
                    }
                }
                return closestWall;
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
                    // If there's only one element, add it as a leaf node.
                    alongNode.Children.Add(new ElementNode(wallGroup.First()));
                }
                else
                {
                    var groupNode = CreateGroupNode(wallGroup.ToList());
                    alongNode.Children.Add(groupNode);
                }

                // Calculate the average of all element's origins along the wall.
                var average = new BBox3(alongNode.GatherElements().Select(e => e.Transform.Origin).ToList()).Center();
                var parameter = average.ClosestPointOn(wall.CenterLine).DistanceTo(wall.CenterLine.Start) / wall.CenterLine.Length();
                alongNode.Parameter = parameter;

                root.Children.Add(alongNode);
            }

            foreach (var nonWallGroup in wallGroupedElements.Where(g => g.Key == null))
            {
                var groupNode = CreateGroupNode(nonWallGroup.ToList());
                root.Children.Add(groupNode);
            }
            return graph;
        }

        private static GroupNode CreateGroupNode(IEnumerable<GeometricElement> groupElements)
        {
            // If there are multiple elements, add them as a together node.
            var groupNode = new GroupNode();

            var supportingNodes = new Dictionary<GeometricElement, SupportNode>();
            var supportedElements = new List<GeometricElement>();

            // Starting from the elements, find the supporting elements.
            // If there's no supporting element, add a leaf node to the together node
            foreach (var element in groupElements)
            {
                foreach (var innerElement in groupElements)
                {
                    if (element == innerElement)
                    {
                        continue;
                    }

                    if (IsSupportedBy(innerElement, element))
                    {
                        // TODO: Check if the supporting element already has a support node
                        // support multiple levels of support.
                        if (!supportingNodes.ContainsKey(element))
                        {
                            supportingNodes.Add(element, new SupportNode(element, new List<GeometricElement> { innerElement }));
                        }
                        else
                        {
                            supportingNodes[element].Children.Add(new ElementNode(innerElement));
                        }
                        supportedElements.Add(innerElement);
                    }
                }

                if (!supportingNodes.ContainsKey(element))
                {
                    if (!supportedElements.Contains(element))
                    {
                        // TODO: This is where we'll look to create around nodes.
                        groupNode.Children.Add(new ElementNode(element));
                    }
                }
                else
                {
                    groupNode.Children.Add(supportingNodes[element]);
                }
            }
            return groupNode;
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
        public List<GeometricElement> GatherElements(List<GeometricElement> elements = null)
        {
            if (elements == null)
            {
                elements = new List<GeometricElement>();
            }

            foreach (var child in Children)
            {
                if (child is ElementNode elementNode)
                {
                    elements.Add(elementNode.Element);
                }
                else
                {
                    child.GatherElements(elements);
                }
            }
            return elements;
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
        public AroundNode(GeometricElement target, List<GeometricElement> elements)
        {
            Children.Add(new ElementNode(target));
            Children.AddRange(elements.Select(e => new ElementNode(e)));
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