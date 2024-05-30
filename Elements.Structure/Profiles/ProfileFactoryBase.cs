using System;
using System.Collections.Generic;
using System.IO;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// Base class for profile factories.
    /// </summary>
    /// <typeparam name="TProfileType"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class ProfileFactoryBase<TProfileType, TProfile>
    {
        /// <summary>
        /// A collection of profile data.
        /// </summary>
        protected List<string[]> _profileData = new List<string[]>();

        /// <summary>
        /// Construct a profile factory.
        /// </summary>
        /// <param name="data">A comma separated data value representing the properties of the profiles.</param>
        public ProfileFactoryBase(string data)
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
        /// Get a profile by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract TProfile GetProfileByName(string name);

        /// <summary>
        /// Get a profile by type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract TProfile GetProfileByType(TProfileType type);

        /// <summary>
        /// Get all profiles.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TProfile> AllProfiles()
        {
            for (var i = 0; i < _profileData.Count; i++)
            {
                yield return CreateProfile(i);
            }
        }

        /// <summary>
        /// Create a profile.
        /// </summary>
        /// <param name="typeIndex"></param>
        /// <returns></returns>
        protected virtual TProfile CreateProfile(int typeIndex)
        {
            throw new NotImplementedException();
        }
    }
}