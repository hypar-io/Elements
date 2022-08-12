using Newtonsoft.Json;

namespace Elements.Geometry.Solids
{
    /// <summary>A sweep of a profile along a curve.</summary>
    public partial class Sweep : SolidOperation, System.ComponentModel.INotifyPropertyChanged
    {
        private Profile _profile;
        private Curve _curve;
        private double _startSetback;
        private double _endSetback;
        private double _profileRotation;

        /// <summary>
        /// Construct a sweep.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="curve"></param>
        /// <param name="startSetback"></param>
        /// <param name="endSetback"></param>
        /// <param name="profileRotation"></param>
        /// <param name="isVoid"></param>
        [JsonConstructor]
        public Sweep(Profile @profile, Curve @curve, double @startSetback, double @endSetback, double @profileRotation, bool @isVoid)
            : base(isVoid)
        {
            this._profile = @profile;
            this._curve = @curve;
            this._startSetback = @startSetback;
            this._endSetback = @endSetback;
            this._profileRotation = @profileRotation;

            this.PropertyChanged += (sender, args) => { UpdateGeometry(); };
            UpdateGeometry();
        }

        /// <summary>The id of the profile to be swept along the curve.</summary>
        [JsonProperty("Profile", Required = Required.AllowNull)]
        public Profile Profile
        {
            get { return _profile; }
            set
            {
                if (_profile != value)
                {
                    _profile = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>The curve along which the profile will be swept.</summary>
        [JsonProperty("Curve", Required = Required.AllowNull)]
        public Curve Curve
        {
            get { return _curve; }
            set
            {
                if (_curve != value)
                {
                    _curve = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>The amount to set back the resulting solid from the start of the curve.</summary>
        [JsonProperty("StartSetback", Required = Required.Always)]
        public double StartSetback
        {
            get { return _startSetback; }
            set
            {
                if (_startSetback != value)
                {
                    _startSetback = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>The amount to set back the resulting solid from the end of the curve.</summary>
        [JsonProperty("EndSetback", Required = Required.Always)]
        public double EndSetback
        {
            get { return _endSetback; }
            set
            {
                if (_endSetback != value)
                {
                    _endSetback = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>The rotation of the profile around the sweep's curve.</summary>
        [JsonProperty("ProfileRotation", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double ProfileRotation
        {
            get { return _profileRotation; }
            set
            {
                if (_profileRotation != value)
                {
                    _profileRotation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void UpdateGeometry()
        {
            this._solid = Kernel.Instance.CreateSweepAlongCurve(this._profile, this._curve, this._startSetback, this._endSetback, this._profileRotation);
        }
    }
}