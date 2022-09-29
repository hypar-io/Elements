using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A zero-thickness solid defined by a profile.
    /// </summary>
    public class Lamina : SolidOperation
    {
        private Polygon _perimeter;
        private IList<Polygon> _voids;

        /// <summary>The perimeter.</summary>
        [JsonProperty("Perimeter", Required = Required.AllowNull)]
        public Polygon Perimeter
        {
            get { return _perimeter; }
            set
            {
                if (_perimeter != value)
                {
                    _perimeter = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// A collection of voids.
        /// </summary>
        [JsonProperty("Voids", Required = Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<Polygon> Voids
        {
            get { return _voids; }
            set
            {
                if (_voids != value)
                {
                    _voids = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Construct a lamina.
        /// </summary>
        /// <param name="perimeter"></param>
        /// <param name="voids"></param>
        /// <param name="isVoid"></param>
        [JsonConstructor]
        public Lamina(Polygon @perimeter, IList<Polygon> @voids, bool @isVoid)
            : base(isVoid)
        {
            this._perimeter = @perimeter;
            this._voids = @voids;

            this.PropertyChanged += (sender, args) => { UpdateGeometry(); };
            UpdateGeometry();
        }


        /// <summary>
        /// Construct a lamina from a perimeter.
        /// </summary>
        /// <param name="perimeter">The lamina's perimeter</param>
        /// <param name="isVoid">Should the lamina be considered a void?</param>
        public Lamina(Polygon @perimeter, bool @isVoid = false) : this(@perimeter, new List<Polygon>(), @isVoid)
        {
            // This additional constructor is necessary for backwards compatibility with the old generated constructor.
        }

        /// <summary>
        /// Construct a lamina from a profile.
        /// </summary>
        /// <param name="profile">The profile of the lamina</param>
        /// <param name="isVoid">Should the lamina be considered a void?</param>
        /// <returns></returns>
        public Lamina(Profile profile, bool isVoid = false) : this(profile.Perimeter, profile.Voids, isVoid)
        {

        }

        private void UpdateGeometry()
        {
            this._solid = Kernel.Instance.CreateLamina(this._perimeter, this._voids);
        }
    }
}