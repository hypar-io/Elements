#pragma warning disable CS1591

using System;
using Elements.Serialization.JSON;

namespace Elements
{
    /// <summary>
    /// An object which is identified 
    /// with a unique identifier and a name.
    /// </summary>
    [JsonInheritanceAttribute("Elements.Material", typeof(Elements.Material))]
    [JsonInheritanceAttribute("Elements.Geometry.Profile", typeof(Elements.Geometry.Profile))]
    [JsonInheritanceAttribute("Elements.Opening", typeof(Elements.Opening))]
    [JsonInheritanceAttribute("Elements.Geometry.Profiles.WideFlangeProfile", typeof(Elements.Geometry.Profiles.WideFlangeProfile))]
    [JsonInheritanceAttribute("Elements.Geometry.Profiles.HSSPipeProfile", typeof(Elements.Geometry.Profiles.HSSPipeProfile))]
    public abstract partial class Identifiable
    {
        public Identifiable(Guid id=default(Guid), string name=null)
        {
            this.Id = id == default(Guid) ? Guid.NewGuid() : id;
            this.Name = name;
        }
    }
}