using System;
using Elements.Validators;
using Newtonsoft.Json;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// An extrusion of a profile, in a direction, to a height.
    /// </summary>
    public class Extrude : SolidOperation, System.ComponentModel.INotifyPropertyChanged
    {
        private Profile _profile;
        private double _height;
        private Vector3 _direction;
        private bool _flipped;

        /// <summary>The id of the profile to extrude.</summary>
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

        /// <summary>The height of the extrusion.</summary>
        [JsonProperty("Height", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
        public double Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>The direction in which to extrude.</summary>
        [JsonProperty("Direction", Required = Required.AllowNull)]
        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>Is the extrusion flipped inside out?</summary>
        [JsonProperty("Flipped")]
        public bool Flipped
        {
            get { return _flipped; }
            set
            {
                if (_flipped != value)
                {
                    _flipped = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Construct an extrusion.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="height"></param>
        /// <param name="direction"></param>
        /// <param name="isVoid"></param>
        [JsonConstructor]
        public Extrude(Profile @profile, double @height, Vector3 @direction, bool @isVoid = false, bool flipped = false)
            : base(isVoid)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (direction.Length() == 0)
                {
                    throw new ArgumentException("The extrude cannot be created. The provided direction has zero length.");
                }
            }

            this._profile = @profile;
            this._height = @height;
            this._direction = @direction;
            this._flipped = flipped;

            this.PropertyChanged += (sender, args) => { UpdateGeometry(); };
            UpdateGeometry();
        }

        private void UpdateGeometry()
        {
            this._solid = Kernel.Instance.CreateExtrude(this._profile, this._height, this._direction, this._flipped);
        }
    }
}