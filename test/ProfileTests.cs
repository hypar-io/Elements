using System;
using System.Linq;
using Xunit;

namespace Elements.Geometry.Tests {
    public class ProfileTests {
        [Fact]
        public void ProfileUnion()
        {
            var circle = Polygon.Circle(3,4);
            var seed = new Profile( circle );
            for(int i=0; i<3; i++) {
                for(int j=0; j<2; j++) {
                    Transform t = new Transform(5*i, 5*j, 0);
                    var newCircle = new Profile( new Polygon(circle.Vertices.Select(v => t.OfPoint(v)).ToArray()) );
                    seed = seed.Union(newCircle);
                    if(seed == null) throw new Exception("Making union failed");
                }
            }
            Assert.Equal(2, seed.Voids.Count());
        }
    }
}