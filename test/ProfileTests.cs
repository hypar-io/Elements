using System;
using System.Linq;
using Xunit;
using Elements.Serialization.glTF;
using System.Collections.Generic;

namespace Elements.Geometry.Tests {
    public class ProfileTests {
        [Fact]
        public void ProfileMultipleUnion()
        {
            // small grid of rough circles is unioned
            // 2x3 grid shoud produce 4 openings
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
            // uncomment the following to view the resulting union profile
            // var floorType = new FloorType("test", new List<MaterialLayer> { new MaterialLayer(new Material("green", Colors.Green, 0.0f, 0.0f), 0.1) });
            // var floor3 = new Floor(seed, floorType, 0);
            // var testMdl = new Model();
            // testMdl.AddElement(floor3);
            // testMdl.ToGlTF("../../../ProfileMultipleUnion.gltf", false);
            Assert.Equal(2, seed.Voids.Count());
        }
        [Fact]
        public void ProfileUnion() {
            var largeCirc = Polygon.Circle(3,4);
            var smallCirc = Polygon.Circle(1, 4);
            var firstProfile = new Profile(largeCirc, new Polygon[] {smallCirc});

            var transform = new Transform(new Vector3(4.5,0,0));
            var secondProfile = transform.OfProfile(firstProfile);

            var unionProfile = firstProfile.Union(secondProfile);

            // uncomment this for viewing the resulting profile
            // var testMdl = new Model();
            // var floorType = new FloorType("test", new List<MaterialLayer> { new MaterialLayer(new Material("green", Colors.Green, 0.0f, 0.0f), 0.1) });
            // var floorUnion = new Floor(unionProfile, floorType, 0);
            // testMdl.AddElement(floorUnion);
            // testMdl.ToGlTF("../../../ProfileUnion.gltf",false);
            Assert.Equal(2, unionProfile.Voids.Count());
        }
    }
}