using System.Collections.Generic;
using Elements.Flow;
using Xunit;

namespace Elements.MEP.Tests
{
    public partial class Tests
    {

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(6)]
        public void Cloning(int nodeIndex)
        {
            var original = FittingsTests.GetSampleTreeWithTrunkBelow();
            var clone = original.Clone() as Tree;
            Assert.Equal(original.Connections.Count, clone.Connections.Count);
            Assert.Equal(original.InternalNodes.Count, clone.InternalNodes.Count);

            var originalNode = original.InternalNodes[nodeIndex];
            Assert.False(clone.InternalNodes.Contains(originalNode));
            Assert.Null(clone.GetOutgoingConnection(originalNode));
            Assert.Equal(clone.InternalNodes[nodeIndex].Position, originalNode.Position);
            Assert.NotEqual(clone.InternalNodes[nodeIndex].Id, originalNode.Id);
        }
    }
}