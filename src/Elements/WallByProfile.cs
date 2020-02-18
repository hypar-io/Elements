using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements {
    public partial class WallByProfile 
    {
        public WallByProfile(Profile @profile, double @thickness, Line @centerline, Transform @transform=null, Material @material=null, Representation @representation=null, bool @isElementDefinition=false)
            : base(transform != null ? transform : new Transform(),
                   material != null ? material : BuiltInMaterials.Concrete,
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

            // to ensure the correct direction, we find the direction form a point on the polygon to the vertical plane of the centerline
            var point = Profile.Perimeter.Vertices.First();
            var centerPlane = new Plane( Centerline.Start, Centerline.End, Centerline.End+Vector3.ZAxis );
            var direction = new Line( point, point.Project(centerPlane) ).Direction();
        
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, direction, false));
        }
    }
}