using Xunit;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Geometry.Profiles;
using System;
using Newtonsoft.Json;
using Xunit.Abstractions;
using Elements.Serialization;
using System.Collections.Generic;

namespace Elements.Tests
{
    public class SolidTests
    {
        private readonly ITestOutputHelper output;

        public SolidTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SweptSolid()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var inner = Polygon.Ngon(n, 1.75).Reversed();

            var solid = new SweptSolid(outer, new[]{inner}, 5);
            foreach(var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            Assert.Equal(2 * n + 2, solid.Faces.Count);
            Assert.Equal(n * 6, solid.Edges.Count);
            Assert.Equal(n * 4, solid.Vertices.Count);
            solid.ToGlb("models/SweptSolid.glb");
        }

        [Fact]
        public void SweptSolidAngle()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var inner = Polygon.Ngon(n, 1.75).Reversed();

            var solid = new SweptSolid(outer, new[]{inner}, new Vector3(0.5,0.5,0.5), 5);
            foreach(var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            Assert.Equal(2 * n + 2, solid.Faces.Count);
            Assert.Equal(n * 6, solid.Edges.Count);
            Assert.Equal(n * 4, solid.Vertices.Count);
            solid.ToGlb("models/SweptSolidAngle.glb");
        }

        [Fact]
        public void SweptSolidTransformToStart()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var inner = Polygon.Ngon(n, 1.75).Reversed();

            var solid = new SweptSolid(outer, new[]{inner}, new Vector3(0.5,0.5,0.5), 5);
            foreach(var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            Assert.Equal(2 * n + 2, solid.Faces.Count);
            Assert.Equal(n * 6, solid.Edges.Count);
            Assert.Equal(n * 4, solid.Vertices.Count);
            solid.ToGlb("models/SweptSolidTransformToStart.glb");
        }

        [Fact]
        public void SweptSolidPolyline()
        {
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W44x335");
            var path = new Polyline(new []{new Vector3(0,0), new Vector3(0,2), new Vector3(0,3,1), new Vector3(0,5,1)});
            var solid = new SweptSolid(profile.Perimeter, null, path);
            foreach(var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            solid.ToGlb("models/SweptSolidPolyline.glb");
        }

        [Fact]
        public void SweptSolidPolygon()
        {
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W44x335");
            var path = Polygon.Ngon(12, 5);
            var solid = new SweptSolid(profile.Perimeter, null, path);
            foreach(var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            solid.ToGlb("models/SweptSolidPolygon.glb");
        }
    
        [Fact]
        public void Serialization()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var solid = new SweptSolid(outer, null, 2.0, BuiltInMaterials.Default);
            var materials = new Dictionary<long,Material>();
            var defMaterial = BuiltInMaterials.Default;
            materials.Add(defMaterial.Id, defMaterial);
            var json = JsonConvert.SerializeObject(solid, new JsonSerializerSettings(){
                Converters = new []{new SolidConverter(materials)},
                Formatting = Formatting.Indented
            });
            var newSolid = JsonConvert.DeserializeObject<Solid>(json, new JsonSerializerSettings(){
                Converters = new []{new SolidConverter(materials)}
            });
            Assert.Equal(8, newSolid.Vertices.Count);
            Assert.Equal(12, newSolid.Edges.Count);
            Assert.Equal(6, newSolid.Faces.Count);
            newSolid.ToGlb("models/SweptSolidDeserialized.glb");
        }
    }

}