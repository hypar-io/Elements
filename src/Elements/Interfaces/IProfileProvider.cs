using Elements.Geometry;

namespace Elements.Interfaces
{
    /// <summary>
    /// Interface implemented by types which provide a Profile.
    /// </summary>
    public interface IProfileProvider
    {
        /// <summary>
        /// A Profile.
        /// </summary>
        Profile Profile {get;}
    }
}