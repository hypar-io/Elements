using System;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A square hollow section profile.
    /// </summary>
    public class SHSProfile : RHSProfile
    {
        /// <summary>
        /// Construct an square hollow section profile.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        [JsonConstructor]
        public SHSProfile(string name,
                          Guid id,
                          double a,
                          double b,
                          double t) : base(name,
                                           id, a, b, t)
        { }
    }
}