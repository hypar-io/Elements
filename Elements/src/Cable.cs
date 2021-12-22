using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// A cable defined by polyline and diameter
    /// </summary>
    public class Cable : GeometricElement
    {
        /// <summary>
        /// Gets the center line of the cable
        /// </summary>
        public Polyline CenterLine { get; private set; }

        /// <summary>
        /// Gets the radius of the cable
        /// </summary>
        public double Radius { get; private set; }

        /// <summary>
        /// Construct a cable from the centerline
        /// </summary>
        /// <param name="centerLine">The center line of the cable.</param>
        /// <param name="radius">The radius of the cable.</param>
        /// <param name="material">The material of the cable.</param>
        /// <param name="transform">An optional transform for the cable.</param>
        /// <param name="representation">The cable's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the cable.</param>
        /// <param name="name">The name of the cable.</param>
        public Cable(Polyline centerLine, double radius,
                    Material material = null,
                    Transform transform = null,
                    Representation representation = null,
                    bool isElementDefinition = false,
                    Guid id = default(Guid),
                    string name = null) : base(transform != null ? transform : new Transform(),
                                                 material != null ? material : BuiltInMaterials.Concrete,
                                                 representation != null ? representation : new Representation(new List<SolidOperation>()),
                                                 isElementDefinition,
                                                 id != default(Guid) ? id : Guid.NewGuid(),
                                                 name)
        {
            CenterLine = centerLine;
            Radius = radius;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();
            if (CenterLine.Vertices.Count() < 2)
            {
                return;
            }
            var origin = CenterLine.Vertices.FirstOrDefault();
            var n = (CenterLine.Vertices[1] - CenterLine.Vertices[0]).Unitized();
            var transform = new Transform(origin, n);
            var circle = new Circle(new Vector3(), Radius);
            var profile = new Profile(circle.ToPolygon(20));

            var sweep = new Sweep(profile, CenterLine, 0, 0, 0, false);
            this.Representation.SolidOperations.Add(sweep);
        }

        /// <summary>
        /// Calculates the length of the cable
        /// </summary>
        public double Length
        {
            get { return CenterLine.Length(); }
        }
    }
}