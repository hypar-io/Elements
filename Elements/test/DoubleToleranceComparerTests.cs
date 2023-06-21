using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;
using Elements.Search;
using Elements.Geometry;

namespace Elements.Tests
{
    public class DoubleToleranceComparerTests : ModelTest
    {
        [Fact]
        public void DoubleToleranceComparerTest()
        {
            List<double> differentNumbers = new List<double> { 1, 2, 3, 4, 5 };
            var groups = differentNumbers.GroupBy(n => n, new DoubleToleranceComparer(Vector3.EPSILON));
            Assert.Equal(5, groups.Count());
            Assert.True(groups.All(g => g.Count() == 1));

            List<double> sameNumbers = new List<double> { 1, 2, 3, 2, 3 };
            groups = sameNumbers.GroupBy(n => n, new DoubleToleranceComparer(Vector3.EPSILON));
            Assert.Equal(3, groups.Count());
            Assert.True(groups.FirstOrDefault(g => g.Key.ApproximatelyEquals(1)).Count() == 1);
            Assert.True(groups.FirstOrDefault(g => g.Key.ApproximatelyEquals(2)).Count() == 2);
            Assert.True(groups.FirstOrDefault(g => g.Key.ApproximatelyEquals(3)).Count() == 2);

            List<double> closeNumbers = new List<double> { 1, 2, 1.000001, 2.000002, 1.99999999 };
            groups = closeNumbers.GroupBy(n => n, new DoubleToleranceComparer(Vector3.EPSILON));
            Assert.Equal(2, groups.Count());
            Assert.True(groups.FirstOrDefault(g => g.Key.ApproximatelyEquals(1)).Count() == 2);
            Assert.True(groups.FirstOrDefault(g => g.Key.ApproximatelyEquals(2)).Count() == 3);
            
            //First "unique" item is set as a key. 
            List<double> firstIsKey = new List<double> { 1.000005, 1.00001, 1, 0.999995 };
            groups = firstIsKey.GroupBy(n => n, new DoubleToleranceComparer(Vector3.EPSILON));
            Assert.Equal(2, groups.Count());
            Assert.True(groups.FirstOrDefault(g => g.Key == 1.000005).Count() == 3);
            Assert.True(groups.FirstOrDefault(g => g.Key == 0.999995).Count() == 1);
        }
    }
}
