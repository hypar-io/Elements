using System.Collections.Generic;
using System.Linq;
using Elements.Collections.Generics;
using Elements.Geometry;
using glTFLoader.Schema;

namespace Elements.Serialization.glTF
{
    internal class NodeUtilities
    {
        internal static int[] AddNodes(List<Node> nodes, IEnumerable<Node> newNodes, int? parent)
        {
            var newIds = Enumerable.Range(nodes.Count, newNodes.Count()).ToArray(newNodes.Count());
            nodes.AddRange(newNodes);

            if (parent != null)
            {
                if (nodes[(int)parent].Children == null)
                {
                    nodes[(int)parent].Children = newIds.ToArray(newIds.Count());
                }
                else
                {
                    var originalChildren = nodes[(int)parent].Children;
                    var children = new int[originalChildren.Length + newNodes.Count()];
                    for (int i = 0; i < originalChildren.Length; i++)
                    {
                        children[i] = originalChildren[i];
                    }
                    for (int j = 0; j < newIds.Length; j++)
                    {
                        children[originalChildren.Length + j] = newIds[j];
                    }
                    nodes[(int)parent].Children = children;
                }
            }

            return newIds;
        }

        internal static int AddNode(List<Node> nodes, Node newNode, int? parentId)
        {
            return NodeUtilities.AddNodes(nodes, new[] { newNode }, parentId).First();
        }

        internal static int CreateAndAddTransformNode(List<Node> nodes, Transform transform, int parentId)
        {
            if (transform != null)
            {
                var a = transform.XAxis;
                var b = transform.YAxis;
                var c = transform.ZAxis;

                var transNode = new Node();

                transNode.Matrix = new[]{
                    (float)a.X, (float)a.Y, (float)a.Z, 0.0f,
                    (float)b.X, (float)b.Y, (float)b.Z, 0.0f,
                    (float)c.X, (float)c.Y, (float)c.Z, 0.0f,
                    (float)transform.Origin.X,(float)transform.Origin.Y,(float)transform.Origin.Z, 1.0f
                };

                parentId = AddNode(nodes, transNode, 0);
            }

            return parentId;
        }

        internal static int[] AddInstanceNode(
                                            List<glTFLoader.Schema.Node> nodes,
                                            List<int> meshIds,
                                            Transform transform)
        {
            var a = transform.XAxis;
            var b = transform.YAxis;
            var c = transform.ZAxis;

            var matrix = new[]{
                    (float)a.X, (float)a.Y, (float)a.Z, 0.0f,
                    (float)b.X, (float)b.Y, (float)b.Z, 0.0f,
                    (float)c.X, (float)c.Y, (float)c.Z, 0.0f,
                    (float)transform.Origin.X,(float)transform.Origin.Y,(float)transform.Origin.Z, 1.0f
                };
            var newNodes = meshIds.Select(meshId => new Node() { Matrix = matrix, Mesh = meshId });
            return AddNodes(nodes, newNodes, 0);
        }

        internal static int CreateNodeForMesh(int meshId, List<glTFLoader.Schema.Node> nodes, Transform transform = null)
        {
            var parentId = 0;

            parentId = NodeUtilities.CreateAndAddTransformNode(nodes, transform, parentId);

            // Add mesh node to gltf nodes
            var node = new Node();
            node.Mesh = meshId;
            var nodeId = AddNode(nodes, node, parentId);
            return nodeId;
        }
    }
}