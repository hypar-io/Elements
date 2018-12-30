#pragma warning disable CS1591

namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// Interface implemented by types which provide a Profile.
    /// </summary>
    public interface IProfileProvider
    {
        /// <summary>
        /// A Profile.
        /// </summary>
        IProfile Profile {get;}
    }
}