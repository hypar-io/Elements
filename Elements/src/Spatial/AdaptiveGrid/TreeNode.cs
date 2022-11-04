using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Node that represents a vertex in a routed tree.
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// List of incoming nodes.
        /// </summary>
        public List<TreeNode> Leafs = new List<TreeNode>();

        /// <summary>
        /// Outgoing node.
        /// </summary>
        public TreeNode Trunk = null;

        /// <summary>
        /// Id of corresponding vertex in the gird.
        /// </summary>
        public ulong Id;

        /// <summary>
        /// Create new node from vertex id without any connections.
        /// </summary>
        /// <param name="id">Id of vertex.</param>
        public TreeNode(ulong id)
        {
            Id = id;
        }

        /// <summary>
        /// Remove all incoming and outgoing connections from the grid.
        /// Removed nodes will also be disconnected from this node.
        /// </summary>
        public void Disconnect()
        {
            foreach (var leaf in Leafs)
            {
                leaf.Trunk = null;
            }
            Leafs.Clear();

            if (Trunk != null)
            {
                Trunk.RemoveLeaf(this);
            }
        }

        /// <summary>
        /// Remove leaf connection and set its trunk to null.
        /// </summary>
        /// <param name="leaf"></param>
        public void RemoveLeaf(TreeNode leaf)
        {
            Leafs.Remove(leaf);
            leaf.Trunk = null;
        }

        /// <summary>
        /// Set trunk node and add this node as leaf to it.
        /// </summary>
        /// <param name="trunk"></param>
        public void SetTrunk(TreeNode trunk)
        {
            if (Trunk != null)
            {
                Trunk.Leafs.Remove(this);
            }

            Trunk = trunk;
            trunk.Leafs.Add(this);
        }
    }
}
