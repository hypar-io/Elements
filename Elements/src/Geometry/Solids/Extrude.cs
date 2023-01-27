using System;
using Elements.Serialization.JSON;
using Elements.Validators;
using System.Text.Json.Serialization;

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

        /// <summary>The id of the profile to extrude.</summary>
        [JsonConverter(typeof(ElementConverter<Profile>))]
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

        /// <summary>
        /// Construct an extrusion.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="height"></param>
        /// <param name="direction"></param>
        /// <param name="isVoid"></param>
        [JsonConstructor]
        public Extrude(Profile @profile, double @height, Vector3 @direction, bool @isVoid)
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

            this.PropertyChanged += (sender, args) => { UpdateGeometry(); };
            UpdateGeometry();
        }

        private void UpdateGeometry()
        {
            this._solid = Kernel.Instance.CreateExtrude(this._profile, this._height, this._direction);
        }
    }
}