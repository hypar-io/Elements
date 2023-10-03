using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Flow
{
    public partial class Tree
    {
        public static Section AdjustPath(Tree tree, string sectionKey, List<Vector3> path)
        {
            var section = tree.GetSectionFromKey(sectionKey);
            if (TreeSectionPathAdjustment.AdjustPath(tree, section, path))
            {
                tree.UpdateSections();
            }
            return section;
        }
    }

    internal class TreeSectionPathAdjustment
    {
        public static bool AdjustPath(Tree tree, Section section, List<Vector3> path)
        {
            TreeSectionPathAdjustment adjusted = new TreeSectionPathAdjustment(tree, section, path);
            adjusted.ApplyPath();

            section.HintPath = new Polyline(path);
            return true;
        }

        private readonly Tree tree;
        private readonly Section section;
        private List<Vector3> path;

        private TreeSectionPathAdjustment(Tree tree, Section section, List<Vector3> path)
        {
            this.tree = tree;
            this.section = section;
            this.path = path;
        }

        private void ApplyPath()
        {
            Node start = section.Start;
            Node end = section.End;

            var connections = tree.GetOutgoingConnections(start);
            var connection = connections.FirstOrDefault(c => tree.GetSectionFromConnection(c).SectionKey.Equals(section.SectionKey));
            var head = connection.End;
            if (connection.End != end)
            {
                tree.ShiftConnectionToNode(connection, end);
            }

            while (head != end)
            {
                var connectionToRemove = tree.GetOutgoingConnection(head);
                head = connectionToRemove.End;
                tree.Disconnect(connectionToRemove);
                tree.InternalNodes.Remove(connectionToRemove.Start);
            }

            var referenceConnection = tree.GetIncomingConnections(start).FirstOrDefault();
            var referenceVector = referenceConnection == null ? Vector3.ZAxis : referenceConnection.Direction();
            path = new Polyline(path).ForceAngleCompliance(new List<double> { 90, 45 }, referenceVector, NormalizationType.Middle).Vertices.ToList();

            if (!path.First().IsAlmostEqualTo(start.Position))
            {
                start.Position = path.First();
            }

            var nodes = new List<Node>();
            var currentConnection = connection;
            for (int i = 1; i < path.Count - 1; i++)
            {
                var node = tree.SplitConnectionThroughPoint(currentConnection, path[i], out var createdConnections);
                nodes.Add(node);
                currentConnection = createdConnections[1];
            }

            if (!path.Last().IsAlmostEqualTo(end.Position))
            {
                end.Position = path.Last();
            }

            foreach (var node in nodes)
            {
                var income = tree.GetIncomingConnections(node);
                var outgoing = tree.GetOutgoingConnection(node);
                if (income.Count == 1 && outgoing != null &&
                    income[0].Direction().IsParallelTo(outgoing.Direction()))
                {
                    var newEnd = outgoing.End;
                    tree.Disconnect(outgoing);
                    tree.ShiftConnectionToNode(income[0], newEnd);
                }
            }
        }
    }
}