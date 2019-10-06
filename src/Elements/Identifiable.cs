#pragma warning disable CS1591

using System;
using Elements.Serialization.JSON;

namespace Elements
{
    /// <summary>
    /// An object which is identified 
    /// with a unique identifier and a name.
    /// </summary>
    [JsonInheritanceAttribute("Elements.Element", typeof(Elements.Element))]
    [JsonInheritanceAttribute("Elements.Material", typeof(Elements.Material))]
    [JsonInheritanceAttribute("Elements.ElementTypes.ElementType", typeof(Elements.ElementTypes.ElementType))]
    [JsonInheritanceAttribute("Elements.ElementTypes.WallType", typeof(Elements.ElementTypes.WallType))]
    [JsonInheritanceAttribute("Elements.ElementTypes.FloorType", typeof(Elements.ElementTypes.FloorType))]
    [JsonInheritanceAttribute("Elements.ElementTypes.StructuralFramingType", typeof(Elements.ElementTypes.StructuralFramingType))]
    [JsonInheritanceAttribute("Elements.Geometry.Profile", typeof(Elements.Geometry.Profile))]
    [JsonInheritanceAttribute("Elements.Geometry.Profiles.WideFlangeProfile", typeof(Elements.Geometry.Profiles.WideFlangeProfile))]
    [JsonInheritanceAttribute("Elements.Geometry.Profiles.HSSPipeProfile", typeof(Elements.Geometry.Profiles.HSSPipeProfile))]
    public abstract partial class Identifiable
    {
        public Identifiable()
        {
            this.Id = Guid.NewGuid();
        }

        public Identifiable(Guid id, string name=null)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}