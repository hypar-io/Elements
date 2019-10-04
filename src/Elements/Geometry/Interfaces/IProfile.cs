#pragma warning disable CS1591

using System;
using Elements.Interfaces;

namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// Interface implemented by types which provide a Profile.
    /// </summary>
    public interface IProfile: IReference<Profile>
    {
        /// <summary>
        /// A profile.
        /// </summary>
        Profile Profile { get; }

        Guid ProfileId { get; }
    }
}