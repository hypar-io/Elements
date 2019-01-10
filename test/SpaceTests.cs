using Elements;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    public class SpaceTests : ModelTest
    {
        [Fact]
        public void SpaceExtrude()
        {
            this.Name = "SpaceExtrude";
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polygon(new[]{a,b,c,d});
            var space = new Space(profile, 10);
            this.Model.AddElement(space);
        }

        [Fact]
        public void SpaceBRep()
        {
            this.Name = "SpaceBRep";
            var faces = Extrusions.Extrude(new Profile(Polygon.Rectangle()), 1.0);
            var brep = new FacetedBRep(faces);
            var space = new Space(brep, new Transform(new Vector3(0,0,5)));
            this.Model.AddElement(space);
            var json = this.Model.ToJson();
            var newModel = Model.FromJson(json);
            var newSpace = newModel.ElementsOfType<Space>().ToArray()[0];
            Assert.Equal(space.Transform.Origin.Z, newSpace.Transform.Origin.Z);
        }

        [Fact]
        public void NegativeHeight_ThrowsException()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polygon(new[]{a,b,c,d});
            Assert.Throws<ArgumentOutOfRangeException>(() => new Space(profile, -10));
        }

        [Fact]
        public void Transform()
        {
            var p = Polygon.Rectangle();
            var space = new Space(p, 1.0);
            var t = new Vector3(5,5,5);
            space.Transform.Move(t);
            var p1 = space.Profile.Perimeter;
            for(var i=0; i<p.Vertices.Length; i++)
            {
                Assert.Equal(p.Vertices[i] + t, p1.Vertices[i] + t);
            }
        }
    }
}