using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Hypar;
using Hypar.Elements;
using Hypar.Geometry;
using Xunit.Abstractions;

namespace Hypar.Tests
{
    public class ModelTests
    {
        [Fact]
        public void Default_Construct_NoParameters_Success()
        {
            var model = new Model();
            Assert.NotNull(model);
        }

        [Fact]
        public void Default_Construct_NoParameters_ModelHasBuiltinMaterials()
        {
            var model = new Model();
            Assert.Equal(model.Materials.Count, 4);
            Assert.NotNull(model.Materials[BuiltInMaterials.DEFAULT]);
            Assert.NotNull(model.Materials[BuiltInMaterials.CONCRETE]);
            Assert.NotNull(model.Materials[BuiltInMaterials.GLASS]);
            Assert.NotNull(model.Materials[BuiltInMaterials.STEEL]);
        }

        [Fact]
        public void Default_SaveToGltf_Success()
        {
            var model = QuadPanelModel();
            model.SaveGltf("saveToGltf.gltf");
            Assert.True(File.Exists("saveToGltf.gltf"));
        }

        [Fact]
        public void Default_SaveToGlb_Success()
        {
            var model = QuadPanelModel();
            model.SaveGlb("saveToGlb.glb");
            Assert.True(File.Exists("saveToGlb.glb"));
        }


        [Fact]
        public void Frame()
        {
            var sw = new Stopwatch();
            sw.Start();

            var model = new Model();
            var perimeter = Profiles.Square(new Vector2(), 20, 20);
            var lines = perimeter.Explode();

            var colA = new Line(new Vector3(-10, -10), new Vector3(-10, -10, 20));
            var colB = new Line(new Vector3(10,-10), new Vector3(10, -10, 20));
            var colC = new Line(new Vector3(10, 10), new Vector3(10, 10, 20));
            var colD = new Line(new Vector3(-10,10), new Vector3(-10, 10, 20));
            var d = 2.0;
            var profile = new WideFlangeProfile(1.0, d, 0.1, 0.1);

            model.AddMaterial(new Material("orange", 1.0f, 1.0f, 0.0f, 1.0f, 0.1f, 0.0f));
            var material = model.Materials["orange"];

            var col1 = new Beam(colA, profile, material);
            var col2 = new Beam(colB, profile, material);
            var col3 = new Beam(colC, profile, material);
            var col4 = new Beam(colD, profile, material);

            model.AddElement(col1);
            model.AddElement(col2);
            model.AddElement(col3);
            model.AddElement(col4);

            foreach(var l in lines)
            {
                var a = l.Start;
                var b = l.End;
                var newLine = new Line(new Vector3(a.X, a.Y, 10), new Vector3(b.X, b.Y, 10));
                var newLine2 = new Line(new Vector3(a.X, a.Y, 20), new Vector3(b.X, b.Y, 20));
                var b1 = new Beam(newLine, profile, material);
                var b2 = new Beam(newLine2, profile, material);
                model.AddElement(b1);
                model.AddElement(b2);
            }

            var slab1 = new Slab(perimeter, new Polygon2[]{}, 10.0 + d/2, 0.3, model.Materials[BuiltInMaterials.CONCRETE]);
            model.AddElement(slab1);
            var slab2 = new Slab(perimeter, new Polygon2[]{}, 20.0 + d/2, 0.3, model.Materials[BuiltInMaterials.CONCRETE]);
            model.AddElement(slab2);

            var wf = new WideFlangeProfile(0.5, 1.0, 0.1, 0.1);
            var sys1 = new BeamSystem(slab1, 5, wf, material);
            foreach(var b in sys1.Beams)
            {
                model.AddElement(b);
            }

            var sys2 = new BeamSystem(slab2, 5, wf, material);
            foreach(var b in sys2.Beams)
            {
                model.AddElement(b);
            }

            model.SaveGlb("frame.glb");
            Assert.True(File.Exists("frame.glb"));
            Assert.Equal(24, model.Elements.Count);

            Console.WriteLine($"{sw.Elapsed} for creating frame.");
        }

        private Model QuadPanelModel()
        {
            var model = new Model();
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            var panel = new Panel(new Polygon3(new[]{a,b,c,d}),model.Materials[BuiltInMaterials.GLASS]);
            model.AddElement(panel);
            return model;
        }
    }
}
