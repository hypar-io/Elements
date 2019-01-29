using Elements.Geometry;
using System;
using System.Collections.Generic;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// Base class for types which serve Profiles.
    /// </summary>
    public abstract class ProfileServer
    {
        /// <summary>
        /// A conversion factor from inches to meters.
        /// </summary>
        protected const double InchesToMeters = 0.0254;

        /// <summary>
        /// The map of Profiles.
        /// </summary>
        protected Dictionary<string, Profile> _profiles = new Dictionary<string, Profile>();

        /// <summary>
        /// Get a profile by name from the server.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A Profile. Throws an exception if a profile with the specified name cannot be found.</returns>
        /// <exception cref="System.Exception">Thrown when a Profile with the specfied name cannot be found.</exception>
        public Profile GetProfileByName(string name)
        {
            if (!this._profiles.ContainsKey(name))
            {
                throw new Exception($"The specified Wide Flange profile name, {name}, could not be found.");
            }
            return this._profiles[name];
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