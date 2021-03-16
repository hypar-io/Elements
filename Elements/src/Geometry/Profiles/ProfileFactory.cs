using System;
using System.Collections.Generic;
using System.IO;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// Base class for profile factories.
    /// A profile factory is constructed with a table of data where each row
    /// represents a comma-delimited set of profile properties.
    /// </summary>
    public abstract class ProfileFactory<T, TProfile> where T : Enum
    {
        /// <summary>
        /// A collection of profile data.
        /// </summary>
        protected List<string[]> _profileData = new List<string[]>();

        /// <summary>
        /// Construct a profile factory.
        /// </summary>
        /// <param name="data">A comma separated data value representing the properties of the profiles.</param>
        public ProfileFactory(string data)
        {
            using (var reader = new StringReader(data))
            {
                var lineCount = -1;
                while (true)
                {
                    lineCount++;
                    var line = reader.ReadLine();

                    // Skip the header line containing
                    // the column names.
                    if (lineCount == 0)
                    {
                        continue;
                    }

                    if (line != null)
                    {
                        var values = line.Split(',');
                        try
                        {
                            // Profile data is stored as strings because
                            // values may represent numbers or strings.
                            // The factory methods determine how to interpret
                            // the string values.
                            _profileData.Add(values);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Section data could not be loaded.");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get a profile by name from the factory.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        /// <returns>A profile. Throws an exception if a profile with the specified name cannot be found.</returns>
        /// <exception>Thrown when a profile with the specfied name cannot be found.</exception>
        public abstract TProfile GetProfileByName(string name);

        /// <summary>
        /// Get a profile by type enumeration from the factory.
        /// </summary>
        /// <param name="type">The enumerated type of the profile.</param>
        /// <returns>A profile. Throws an exception if a profile with the specified name cannot be found.</returns>
        /// <exception>Thrown when a profile of the specfied type cannot be found.</exception>
        public abstract TProfile GetProfileByType(T type);

        /// <summary>
        /// Get all Profiles available in the factory.
        /// </summary>
        /// <returns>A collection of all profiles available from the factory.</returns>
        public IEnumerable<TProfile> AllProfiles()
        {
            for (var i = 0; i < _profileData.Count; i++)
            {
                yield return CreateProfile(i);
            }
        }

        /// <summary>
        /// Create the profile give a type.
        /// </summary>
        /// <param name="typeIndex">The index of the type of profile to create.</param>
        protected abstract TProfile CreateProfile(int typeIndex);
    }
}