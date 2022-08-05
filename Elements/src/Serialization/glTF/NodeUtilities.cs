using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Collections.Generics;
using Elements.Geometry;
using glTFLoader.Schema;
using System.Numerics;
using Vector3 = Elements.Geometry.Vector3;

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

                var transNode = new Node
                {
                    Matrix = new[]{
                    (float)a.X, (float)a.Y, (float)a.Z, 0.0f,
                    (float)b.X, (float)b.Y, (float)b.Z, 0.0f,
                    (float)c.X, (float)c.Y, (float)c.Z, 0.0f,
                    (float)transform.Origin.X,(float)transform.Origin.Y,(float)transform.Origin.Z, 1.0f
                }
                };

                parentId = AddNode(nodes, transNode, 0);
            }

            return parentId;
        }

        internal static int CreateAndAddRTSNode(List<Node> nodes, Transform transform, int parentId)
        {
            if (transform != null)
            {
                var transNode = new Node();

                // HACK: Using the matrix class from System.Numerics
                // because it has support for matrix decomposition 
                // out of the box.
                var m = new Matrix4x4((float)transform.Matrix.m11,
                                          (float)transform.Matrix.m12,
                                          (float)transform.Matrix.m13,
                                          0.0f,
                                          (float)transform.Matrix.m21,
                                          (float)transform.Matrix.m22,
                                          (float)transform.Matrix.m23,
                                          0.0f,
                                          (float)transform.Matrix.m31,
                                          (float)transform.Matrix.m32,
                                          (float)transform.Matrix.m33,
                                          0.0f,
                                          (float)transform.Origin.X,
                                          (float)transform.Origin.Y,
                                          (float)transform.Origin.Z,
                                          1.0f);

                Matrix4x4.Decompose(m, out var scale, out System.Numerics.Quaternion rotation, out var translation);
                transNode.Scale = new float[] { scale.X, scale.Y, scale.Z };
                transNode.Translation = new float[] { translation.X, translation.Y, translation.Z };
                transNode.Rotation = new float[] { rotation.X, rotation.Y, rotation.Z, rotation.W };

                parentId = AddNode(nodes, transNode, 0);
            }

            return parentId;
        }

        internal static void AddInstanceAsCopyOfNode(
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
            nodes.Add(newNode);
            newNode.Children = new[] { nodes.Count };

            var rootTransform = new Transform();
            // glb has Y up. transform it to have Z up so we
            // can create instances of it in a Z up world. It will get switched
            // back to Y up further up in the node hierarchy. 
            rootTransform.Rotate(new Vector3(1, 0, 0), 90.0);
            float[] glbOrientationTransform = TransformToMatrix(rootTransform);
            var elementOrientationNode = new glTFLoader.Schema.Node
            {
                Matrix = glbOrientationTransform
            };
            nodes.Add(elementOrientationNode);
            elementOrientationNode.Children = new[] { nodes.Count };

            nodes[0].Children = (nodes[0].Children ?? Array.Empty<int>()).Concat(new[] { nodes.Count - 2 }).ToArray();

            RecursivelyCopyNode(nodes, nodeToCopy);
        }

        private static int RecursivelyCopyNode(List<Node> nodes, ProtoNode nodeToCopy)
        {
            var newNode = new Node
            {
                Matrix = nodeToCopy.Matrix
            };
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

        internal static int[] AddInstanceNode(
                                            List<glTFLoader.Schema.Node> nodes,
                                            List<int> meshIds,
                                            Transform transform)
        {
            float[] matrix = TransformToMatrix(transform);
            var newNodes = meshIds.Select(meshId => new Node() { Matrix = matrix, Mesh = meshId });
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

        internal static int CreateNodeForMesh(int meshId, List<glTFLoader.Schema.Node> nodes, Transform transform = null)
        {
            var parentId = 0;

            parentId = CreateAndAddRTSNode(nodes, transform, parentId);

            // Add mesh node to gltf nodes
            var node = new Node
            {
                Mesh = meshId,
            };

            var nodeId = AddNode(nodes, node, parentId);
            return nodeId;
        }

        internal static void CreateNodeFromNode(List<glTFLoader.Schema.Node> nodes, Transform transform)
        {
            CreateAndAddTransformNode(nodes, transform, 0);
        }
    }
}