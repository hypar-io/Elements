using System;
using System.Collections.Generic;
using System.Text;
using Elements;
using Xunit;

namespace Elements.Tests
{
    public class PriorityQueueTests
    {
        [Fact]
        public void ConstructionFromCollection()
        {
            List<int> ids = new List<int>() { 10, 2, 4, 6, 3, 5 };
            PriorityQueue<int> pq = new PriorityQueue<int>(ids);
            //First in ids is set to 0, others to double.PositiveInfinity 
            Assert.Equal(10, pq.PopMin());
            pq.UpdatePriority(4, 10);
            pq.UpdatePriority(5, 7);
            pq.UpdatePriority(3, 20);
            Assert.Equal(5, pq.PopMin());
            Assert.Equal(4, pq.PopMin());
            Assert.False(pq.Empty());
            pq.UpdatePriority(6, 17);
            pq.UpdatePriority(2, 26);
            Assert.Equal(6, pq.PopMin());
            Assert.Equal(3, pq.PopMin());
            Assert.Equal(2, pq.PopMin());
            Assert.True(pq.Empty());
        }

        [Fact]
        public void ConstructingDynamically()
        {
            PriorityQueue<int> pq = new PriorityQueue<int>();
            pq.AddOrUpdate(1, 10);
            pq.AddOrUpdate(2, 7);
            pq.AddOrUpdate(3, 9);
            pq.AddOrUpdate(4, 13);
            Assert.Equal(2, pq.PopMin());
            Assert.Equal(3, pq.PopMin());
            Assert.False(pq.Empty());
            pq.AddOrUpdate(5, 10);
            pq.AddOrUpdate(6, 4);
            pq.AddOrUpdate(7, 14);
            Assert.Equal(6, pq.PopMin());
            //Order is not guaranteed if priority is the same
            var min = pq.PopMin();
            Assert.True(min == 5 || min == 1);
            min = pq.PopMin();
            Assert.True(min == 5 || min == 1);
            Assert.False(pq.Empty());
            Assert.Equal(4, pq.PopMin());
            Assert.Equal(7, pq.PopMin());
            Assert.True(pq.Empty());
        }

        [Fact]
        public void OverridesDuplicateIds()
        {
            PriorityQueue<int> pq = new PriorityQueue<int>();
            pq.AddOrUpdate(1, 10);
            pq.AddOrUpdate(2, 7);
            pq.AddOrUpdate(2, 12);
            pq.AddOrUpdate(3, 11);
            Assert.Equal(1, pq.PopMin());
            Assert.Equal(3, pq.PopMin());
            Assert.Equal(2, pq.PopMin());
            Assert.True(pq.Empty());
        }
    }
}