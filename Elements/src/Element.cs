#pragma warning disable CS1591

using Elements.Serialization.JSON;

namespace Elements
{
    /// <summary>
    /// An object which is identified 
    /// with a unique identifier and a name.
    /// </summary>
    [JsonInheritanceAttribute("Elements.Material", typeof(Elements.Material))]
    [JsonInheritanceAttribute("Elements.Geometry.Profile", typeof(Elements.Geometry.Profile))]
    [JsonInheritanceAttribute("Elements.Geometry.Profiles.WideFlangeProfile", typeof(Elements.Geometry.Profiles.WideFlangeProfile))]
    [JsonInheritanceAttribute("Elements.Geometry.Profiles.HSSPipeProfile", typeof(Elements.Geometry.Profiles.HSSPipeProfile))]
    [JsonInheritanceAttribute("Elements.ElementInstance", typeof(Elements.ElementInstance))]
    [JsonInheritanceAttribute("Elements.DirectionalLight", typeof(Elements.DirectionalLight))]
    public abstract partial class Element
    {
    }
}