using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Collections.Generics;
using Elements.Geometry;
using glTFLoader.Schema;

namespace Elements.Serialization.glTF
{
    internal static class NodeUtilities
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

        internal static int[] AddNodes(List<Node> nodes, List<int> meshIds, int? parentId)
        {
            var newNodes = meshIds.Select(meshId =>
                        {
                            return new Node() { Mesh = meshId };
                        });

            return AddNodes(nodes, newNodes, parentId);
        }

        internal static int AddNode(List<Node> nodes, Node newNode, int? parentId)
        {
            return NodeUtilities.AddNodes(nodes, new[] { newNode }, parentId).First();
        }

        internal static int CreateAndAddTransformNode(List<Node> nodes, Transform transform, int parentId, Guid? elementId = null)
        {
            if (transform != null)
            {
                var a = transform.XAxis;
                var b = transform.YAxis;
                var c = transform.ZAxis;

                var transNode = new Node();
                if (elementId.HasValue)
                {
                    transNode.SetElementInfo(elementId.Value);
                }

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

        internal static Node AddInstanceAsCopyOfNode(
                                            List<glTFLoader.Schema.Node> nodes,
                                            ProtoNode nodeToCopy,
                                            Transform transform,
                                            System.Guid instanceElementId)
        {
            // Two new nodes are created: a top-level node, which has the
            // element's Transform, and one just below that, which handles
            // flipping the orientation of the glb to have Z up. That node has
            // the node to copy as its only child. 
            // We use the node to copy exactly as is, with an unmodified
            // transform.
            // We need the outermost node to be "purely" the element's
            // transform, so that the transform can be modified in explore at
            // runtime (e.g. by a transform override) and have the expected effect.
            float[] elementTransform = TransformToMatrix(transform);
            var newNode = new glTFLoader.Schema.Node
            {
                Name = $"{instanceElementId}",
                Matrix = elementTransform
            };
            newNode.SetElementInfo(instanceElementId);
            nodes.Add(newNode);
            newNode.Children = new[] { nodes.Count };

            var rootTransform = new Transform();
            // glb has Y up. transform it to have Z up so we
            // can create instances of it in a Z up world. It will get switched
            // back to Y up further up in the node hierarchy. 
            rootTransform.Rotate(new Vector3(1, 0, 0), 90.0);
            float[] glbOrientationTransform = TransformToMatrix(rootTransform);
            var elementOrientationNode = new glTFLoader.Schema.Node();
            elementOrientationNode.Matrix = glbOrientationTransform;
            nodes.Add(elementOrientationNode);
            elementOrientationNode.Children = new[] { nodes.Count };

            nodes[0].Children = (nodes[0].Children ?? Array.Empty<int>()).Concat(new[] { nodes.Count - 2 }).ToArray();

            RecursivelyCopyNode(nodes, nodeToCopy);
            return newNode;
        }

        private static int RecursivelyCopyNode(List<Node> nodes, ProtoNode nodeToCopy)
        {
            var newNode = new Node();
            newNode.Matrix = nodeToCopy.Matrix;
            if (nodeToCopy.Mesh != null)
            {
                newNode.Mesh = nodeToCopy.Mesh;
            }
            nodes.Add(newNode);
            var nodeIndex = nodes.Count - 1;

            var childIndices = new List<int>();

            foreach (var child in nodeToCopy.Children)
            {
                childIndices.Add(RecursivelyCopyNode(nodes, child));
            }
            if (childIndices.Count > 0)
            {
                newNode.Children = childIndices.ToArray();
            }

            return nodeIndex;
        }

        internal static int AddInstanceNode(List<glTFLoader.Schema.Node> nodes, Transform transform, Guid elementId)
        {
            float[] matrix = TransformToMatrix(transform);
            var newNode = new Node() { Matrix = matrix, Name = elementId.ToString() };
            newNode.SetElementInfo(elementId);
            return AddNode(nodes, newNode, 0);
        }

        internal static int AddEmptyNode(List<glTFLoader.Schema.Node> nodes, int parentId)
        {
            return AddNode(nodes, new Node(), parentId);
        }

        internal static int[] AddInstanceNode(
                                            List<glTFLoader.Schema.Node> nodes,
                                            List<int> meshIds,
                                            Transform transform,
                                            Guid elementId)
        {
            float[] matrix = TransformToMatrix(transform);
            var newNodes = meshIds.Select(meshId =>
            {
                var node = new Node() { Matrix = matrix, Mesh = meshId };
                node.SetElementInfo(elementId);
                return node;
            });
            return AddNodes(nodes, newNodes, 0);
        }

        private static float[] TransformToMatrix(Transform transform)
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
            return matrix;
        }

        internal static int CreateNodeForMesh(int meshId, List<glTFLoader.Schema.Node> nodes, Guid? elementId = null, Transform transform = null)
        {
            var parentId = 0;

            parentId = NodeUtilities.CreateAndAddTransformNode(nodes, transform, parentId, elementId);

            // Add mesh node to gltf nodes
            var node = new Node
            {
                Mesh = meshId
            };

            var nodeId = AddNode(nodes, node, parentId);
            return nodeId;
        }

        public static void SetElementInfo(this Node node, Guid elementId, bool? selectable = null)
        {
            if (node.Extensions == null)
            {
                node.Extensions = new Dictionary<string, object>();
            }

            var extensionDict = new Dictionary<string, object>
                {
                    {"id", elementId},
                };

            if (selectable.HasValue)
            {
                extensionDict["selectable"] = selectable.Value;
            }
            node.Extensions["HYPAR_info"] = extensionDict;
        }

        public static void SetRepresentationInfo(this Node node, RepresentationInstance representationInstance)
        {
            if (node.Extensions == null)
            {
                node.Extensions = new Dictionary<string, object>();
            }

            var extensionDict = new Dictionary<string, object>
                {
                    {"isDefault", representationInstance.IsDefault},
                    {"representationType", representationInstance.RepresentationTypes}
                };

            node.Extensions["HYPAR_representation_info"] = extensionDict;
        }
    }
}