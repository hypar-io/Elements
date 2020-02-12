using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements {
    public partial class WallByProfile 
    {
        public WallByProfile(Profile @profile, double @thickness, Line @centerline, Transform @transform=null, Material @material=null, Representation @representation=null, bool @isElementDefinition=false)
            : base(transform != null ? transform : new Transform(),
                   material != null ? material : BuiltInMaterials.Default,
                   representation != null ? representation: new Representation(new List<SolidOperation>()),
                   isElementDefinition,
                   Guid.NewGuid() ,
                   "Wall by Profile") {

            this.Profile = @profile;
            this.Thickness = @thickness;
            this.Centerline = @centerline;
                   }

        public override void UpdateRepresentations() {
            this.Representation.SolidOperations.Clear();
            // The wall is a simple extrusion of the profile
            // var midpoint = Centerline.PointAt(0.5);
            // var direction = new Line(midpoint, midpoint.Project(Profile.Perimeter.Plane())).Direction();
            var direction = Profile.Perimeter.Normal();
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, direction, false));
        }
    }
}