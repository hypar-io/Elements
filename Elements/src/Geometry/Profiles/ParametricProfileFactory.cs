using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A profile factory which creates parametric profiles.
    /// </summary>
    /// <typeparam name="TProfileType"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    public class ParametricProfileFactory<TProfileType, TProfile>
        where TProfileType : Enum
        where TProfile : ParametricProfile
    {
        private readonly Dictionary<string, Dictionary<string, double>> _profileData = new Dictionary<string, Dictionary<string, double>>();

        private readonly Dictionary<TProfileType, TProfile> _profileCache = new Dictionary<TProfileType, TProfile>();

        /// <summary>
        /// Create a parametric profile factory by reading from a csv.
        /// </summary>
        /// <param name="csvPath">The path of the csv. This can be an absolute
        /// path, a path relative to the current working directory, or a path
        /// relative to the location of the type's assembly.</param>
        /// <param name="conversion">The conversion factor applied to values in the catalogue.</param>
        public ParametricProfileFactory(string csvPath, double conversion)
        {
            // Try first as an absolute path, or possibly as a path relative
            // to the current working directory.
            if (!File.Exists(csvPath))
            {
                // Try the path as a relative path to the assembly.
                var dir = Path.GetDirectoryName(GetType().Assembly.Location);
                var path = Path.GetFullPath(Path.Combine(dir, csvPath));
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"The profile factory could not be created. The specified file does not exist: {path}");
                }
                csvPath = path;
            }

            string[] keys = null;

            using (var stream = File.OpenRead(csvPath))
            using (var reader = new StreamReader(stream))
            {
                var lineCount = -1;
                while (true)
                {
                    lineCount++;
                    var line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    var values = line.Split(',');

                    // Skip the header line containing
                    // the column names.
                    if (lineCount == 0)
                    {
                        keys = line.Split(',');
                        continue;
                    }

                    try
                    {
                        // Profile data is stored as strings because
                        // values may represent numbers or strings.
                        // The factory methods determine how to interpret
                        // the string values.
                        var name = values[0];
                        var currentProfileData = new Dictionary<string, double>();

                        // Start at 1 to skip the name column
                        for (var i = 1; i < keys.Length; i++)
                        {
                            double.TryParse(values[i], out double value);
                            currentProfileData.Add(ToSafeIdentifier(keys[i]), value * conversion);
                        }

                        _profileData.Add(ToSafeIdentifier(values[0]), currentProfileData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Section data could not be loaded.");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Get all profiles.
        /// </summary>
        public async Task<IEnumerable<TProfile>> AllProfilesAsync()
        {
            var tasks = _profileData.Select(pd => GetProfileByNameAsync(pd.Key));
            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Get a profile by name.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        public async Task<TProfile> GetProfileByNameAsync(string name)
        {
            TProfileType profileType;
            try
            {
                profileType = (TProfileType)Enum.Parse(typeof(TProfileType), ToSafeIdentifier(name));
            }
            catch (ArgumentException)
            {
                // A corresponding enum type could not be found.
                return null;
            }

            var profile = await GetOrCreateInstanceAndSetGeometryAsync(profileType);

            return profile;
        }

        /// <summary>
        /// Get a profile by type.
        /// </summary>
        /// <param name="type"></param>
        public async Task<TProfile> GetProfileByTypeAsync(TProfileType type)
        {
            return await GetOrCreateInstanceAndSetGeometryAsync(type);
        }

        private async Task<TProfile> GetOrCreateInstanceAndSetGeometryAsync(TProfileType profileType)
        {
            if (_profileCache.ContainsKey(profileType))
            {
                return _profileCache[profileType];
            }

            // Enums will have names that don't contain special characters.
            // We need to pass the clean name.
            var name = ToSafeIdentifier(profileType.ToString());
            if (!_profileData.ContainsKey(name))
            {
                return null;
            }
            var profileData = _profileData[name];

            // Create instance of the type
            var profile = (TProfile)Activator.CreateInstance(typeof(TProfile));
            profile.Name = name;

            // Set the properties on the instance.
            profile.SetPropertiesFromProfileData(profileData, name);

            // Set the geometry on the instance.
            // TODO: Is there a better way to run this?
            await profile.SetGeometryAsync();

            _profileCache.Add(profileType, profile);

            return profile;
        }

        private static string ToSafeIdentifier(string name)
        {
            return name.Replace("-", "__").Replace('/', '_').Replace('.', '_');
        }
    }
}