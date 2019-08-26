using System;
using System.Linq;
using Xunit;
using Elements.Serialization.glTF;
using System.Collections.Generic;
using Elements.Tests;

namespace Elements.Geometry.Tests {
    public class ProfileTests :ModelTest{
        [Fact]
        public void ProfileMultipleUnion()
        {
            this.Name = "MultipleProfileUnion";
            // small grid of rough circles is unioned
            // 2x3 grid shoud produce 4 openings
            var circle = Polygon.Circle(3,4);
            var seed = new Profile( circle );
            var originals = new List<Profile>{seed};
            for(int i=0; i<3; i++) {
                for(int j=0; j<2; j++) {
                    Transform t = new Transform(5*i, 5*j, 0);
                    var newCircle = new Profile( new Polygon(circle.Vertices.Select(v => t.OfPoint(v)).ToArray()) );
                    originals.Add(newCircle);

                    seed = seed.Union(newCircle);
                    
                    if(seed == null) throw new Exception("Making union failed");
                }
            }
            SaveFloorsOfProfiles(new[] {seed}, "./models/ProfileMultipleUnion.gltf", originals);

            Assert.Equal(2, seed.Voids.Count());
        }
        [Fact]
        public void ProfileUnion() {
            this.Name = "ProfileUnion";
            var largeCirc = Polygon.Circle(3,4);
            var smallCirc = Polygon.Circle(1, 4);
            var firstProfile = new Profile(largeCirc, new Polygon[] {smallCirc});

            var transform = new Transform(new Vector3(4.5,0,0));
            var secondProfile = transform.OfProfile(firstProfile);

            var unionProfile = firstProfile.Union(secondProfile);

            SaveFloorsOfProfiles(new[] {unionProfile}, "./models/ProfileUnion.gltf");
            Assert.Equal(2, unionProfile.Voids.Count());
        }

        private void SaveFloorsOfProfiles(IEnumerable<Profile> profiles, string path, IEnumerable<Profile> originals = null) {
            foreach(var profile in profiles) {
                var floorType = new FloorType("profile", new List<MaterialLayer> { new MaterialLayer(new Material("green", Colors.Green, 0.0f, 0.0f), 0.1) });
                var floor = new Floor(profile, floorType, 0);
                this.Model.AddElement(floor);
            }
            if(originals != null) {
                foreach(var original in originals) {
                    var floorType = new FloorType("original", new List<MaterialLayer> { new MaterialLayer(new Material("red", Colors.Red, 0.0f, 0.0f), 0.1) });
                    var floor = new Floor(original, floorType, -1);
                    this.Model.AddElement(floor);
                }
            }
        }
    }
}