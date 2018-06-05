using System;
using System.Diagnostics;
using Hypar.Elements;
using Hypar.Geometry;

namespace examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            var model = new Model();
            var perimeter = Profiles.Square(new Vector3(), 20, 20);
            var lines = perimeter.Explode();

            var colA = Line.FromStart(new Vector3(-10, -10)).ToEnd(new Vector3(-10, -10, 20));
            var colB = Line.FromStart(new Vector3(10,-10)).ToEnd(new Vector3(10, -10, 20));
            var colC = Line.FromStart(new Vector3(10, 10)).ToEnd(new Vector3(10, 10, 20));
            var colD = Line.FromStart(new Vector3(-10,10)).ToEnd(new Vector3(-10, 10, 20));
            var d = 2.0;
            var profile = Profiles.WideFlangeProfile(1.0, d, 0.1, 0.1);

            var material = new Material("orange", 1.0f, 1.0f, 0.0f, 1.0f, 0.1f, 0.0f);

            var col1 = Beam.AlongLine(colA)
                            .WithProfile(profile)
                            .OfMaterial(material);
            var col2 = Beam.AlongLine(colB)
                            .WithProfile(profile)
                            .OfMaterial(material);
            var col3 = Beam.AlongLine(colC)
                            .WithProfile(profile)
                            .OfMaterial(material);
            var col4 = Beam.AlongLine(colD)
                            .WithProfile(profile)
                            .OfMaterial(material);

            model.AddElements(new[]{col1, col2, col3, col4});

            foreach(var l in lines)
            {
                var a = l.Start;
                var b = l.End;
                var newLine = Line.FromStart(new Vector3(a.X, a.Y, 10)).ToEnd(new Vector3(b.X, b.Y, 10));
                var newLine2 = Line.FromStart(new Vector3(a.X, a.Y, 20)).ToEnd(new Vector3(b.X, b.Y, 20));
                var b1 = Beam.AlongLine(newLine)
                                .WithProfile(profile)
                                .OfMaterial(material);
                var b2 = Beam.AlongLine(newLine2)
                                .WithProfile(profile)
                                .OfMaterial(material);
                model.AddElements(new[]{b1,b2});
            }

            var slab1 = Slab.WithinPerimeter(perimeter)
                            .AtElevation(10.0 + d/2)
                            .WithThickness(0.3)
                            .OfMaterial(BuiltIntMaterials.Concrete);

            var slab2 = Slab.WithinPerimeter(perimeter)
                            .AtElevation(20.0 + d/2)
                            .WithThickness(0.3)
                            .OfMaterial(BuiltIntMaterials.Concrete);

            model.AddElements(new[]{slab1,slab2});

            var wf = Profiles.WideFlangeProfile(0.5, 1.0, 0.1, 0.1);
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

            model.SaveGlb("building.glb");

            Console.WriteLine($"{sw.Elapsed} for creating building.");
        }
    }
}
