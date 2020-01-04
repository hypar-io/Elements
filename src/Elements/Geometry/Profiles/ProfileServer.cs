using System;
using System.Collections.Generic;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// Base class for all types which serve profiles.
    /// </summary>
    public abstract class ProfileServer<T> where T: struct
    {
        /// <summary>
        /// A conversion factor from inches to meters.
        /// </summary>
        protected const double InchesToMeters = 0.0254;

        /// <summary>
        /// The map of Profiles.
        /// </summary>
        protected Dictionary<T, Profile> _profiles = new Dictionary<T, Profile>();

        /// <summary>
        /// Get a profile by name from the server.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A profile. Throws an exception if a profile with the specified name cannot be found.</returns>
        /// <exception>Thrown when a profile with the specfied name cannot be found.</exception>
        [ObsoleteAttribute("GetProfileByName will no longer be supported. Use GetProfileByType instead.")]
        public Profile GetProfileByName(string name)
        {
            if(!Enum.TryParse(name, true, out T result))
            {
                throw new Exception($"The profile with the name, {name}, is not recognized for this profile server.");
            }

            if (!this._profiles.ContainsKey(result))
            {
                throw new Exception($"The profile with the name, {name}, could not be found.");
            }
            return this._profiles[result];
        }

        /// <summary>
        /// Get a profile by type enumeration from the server.
        /// </summary>
        /// <param name="type">The enumerated type of the profile.</param>
        /// <returns>A profile. Throws an exception if a profile with the specified name cannot be found.</returns>
        /// <exception>Thrown when a profile of the specfied type cannot be found.</exception>
        public Profile GetProfileByType(T type)
        {
            if (!this._profiles.ContainsKey(type))
            {
                throw new Exception($"The profile with the name, {type.ToString()}, could not be found.");
            }
            return this._profiles[type];
        }

        /// <summary>
        /// Get all Profiles available in the WideFlangeProfileServer.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Profile> AllProfiles()
        {
            var n = this._profiles.GetEnumerator();
            while (n.MoveNext())
            {
                yield return n.Current.Value;
            }
        }
    }
}