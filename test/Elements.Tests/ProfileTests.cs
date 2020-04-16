using System;
using System.Linq;
using Xunit;
using Elements.Serialization.glTF;
using System.Collections.Generic;
using Elements.Tests;

namespace Elements.Geometry.Tests
{
    public class ProfileTests : ModelTest
    {
        [Fact]
        public void ProfileMultipleUnion()
        {
            this.Name = "MultipleProfileUnion";
            // small grid of rough circles is unioned
            // 2x3 grid shoud produce 2 openings
            var circle = Polygon.Circle(3, 4);
            var seed = new Profile(circle);
            var originals = new List<Profile> { seed };
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Transform t = new Transform(5 * i, 5 * j, 0);
                    var newCircle = new Profile(new Polygon(circle.Vertices.Select(v => t.OfPoint(v)).ToArray()));
                    originals.Add(newCircle);

                    seed = seed.Union(newCircle);

                    if (seed == null) throw new Exception("Making union failed");
                }
            }
            var floor = new Floor(seed, 1);
            this.Model.AddElement(floor);

            Assert.Equal(2, seed.Voids.Count());
        }
        [Fact]
        public void ProfileUnion()
        {
            this.Name = "ProfileUnion";
            var largeCirc = Polygon.Circle(3, 4);
            var smallCirc = Polygon.Circle(1, 4);
            var firstProfile = new Profile(largeCirc, new List<Polygon> { smallCirc }, Guid.NewGuid(), "");

            var transform = new Transform(new Vector3(4.5, 0, 0));
            var secondProfile = transform.OfProfile(firstProfile);

            var unionProfile = firstProfile.Union(secondProfile);

            var floor = new Floor(unionProfile, 1);
            this.Model.AddElement(floor);

            Assert.Equal(2, unionProfile.Voids.Count());
        }
    }
}
