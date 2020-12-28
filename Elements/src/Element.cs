#pragma warning disable CS1591

using System.ComponentModel;
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
    [JsonInheritanceAttribute("Elements.Geometry.Representation", typeof(Elements.DirectionalLight))]
    [JsonInheritanceAttribute("Elements.Geometry.SolidRepresentation", typeof(Elements.Geometry.SolidRepresentation))]
    [JsonInheritanceAttribute("Elements.Geometry.CurveRepresentation", typeof(Elements.Geometry.CurveRepresentation))]
    [JsonInheritanceAttribute("Elements.Geometry.PointsRepresentation", typeof(Elements.Geometry.PointsRepresentation))]
    [JsonInheritanceAttribute("Elements.Geometry.MeshRepresentation", typeof(Elements.Geometry.MeshRepresentation))]
    public abstract partial class Element : INotifyPropertyChanged
    {

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}