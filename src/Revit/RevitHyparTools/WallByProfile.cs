using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace RevitHyparTools {
    [UserElement]
    public class WallByProfile : Wall
    {
        public Line CenterLine {get;}
        public double Thickness {get;}
        public WallByProfile(Line centerLine,
                             Profile profile,
                             double thickness,
                             Material material = null,
                             Transform transform = null,
                             Representation representation = null,
                             bool isElementDefinition = false,
                             Guid id = default,
                             string name = null) : base(transform != null ? transform : new Transform(),
                                                 material != null ? material : BuiltInMaterials.Concrete,
                                                 representation != null ? representation : new Representation(new List<SolidOperation>()),
                                                 isElementDefinition,
                                                 id != default(Guid) ? id : Guid.NewGuid(),
                                                 name)
        {
            Profile = profile;
            CenterLine = centerLine;
            Thickness = thickness;
        }

        public override void UpdateRepresentations() {
            this.Representation.SolidOperations.Clear();
            // The wall is a simple extrusion of the profile
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, Profile.Perimeter.Normal(), false));
        }
    }
}