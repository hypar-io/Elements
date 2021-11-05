using System;
using System.Collections.Generic;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// Interface for profile factories.
    /// A profile factory is constructed with a table of data where each row
    /// represents a comma-delimited set of profile properties.
    /// </summary>
    public interface IProfileFactory<TProfileType, TProfile> where TProfileType : Enum
    {
        /// <summary>
        /// Get a profile by name from the factory.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        /// <returns>A profile. Throws an exception if a profile with the specified name cannot be found.</returns>
        /// <exception>Thrown when a profile with the specfied name cannot be found.</exception>
        TProfile GetProfileByName(string name);

        /// <summary>
        /// Get a profile by type enumeration from the factory.
        /// </summary>
        /// <param name="type">The enumerated type of the profile.</param>
        /// <returns>A profile. Throws an exception if a profile with the specified name cannot be found.</returns>
        /// <exception>Thrown when a profile of the specfied type cannot be found.</exception>
        TProfile GetProfileByType(TProfileType type);

        /// <summary>
        /// Get all Profiles available in the factory.
        /// </summary>
        /// <returns>A collection of all profiles available from the factory.</returns>
        IEnumerable<TProfile> AllProfiles();
    }
}