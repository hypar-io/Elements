using System;
using System.Collections.Generic;
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
        private bool _reverseWinding;

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

        /// <summary>Is the extrusion's profile reversed relative to its extrusion vector, resulting in inward-facing face normals?</summary>
        [JsonProperty("Reverse Winding")]
        public bool ReverseWinding
        {
            get { return _reverseWinding; }
            set
            {
                if (_reverseWinding != value)
                {
                    _reverseWinding = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Construct an extrusion.
        /// </summary>
        /// <param name="profile">The profile to extrude.</param>
        /// <param name="height">The height/length of the extrusion.</param>
        /// <param name="direction">The direction of the extrusion.</param>
        /// <param name="isVoid">If true, the extrusion is a "void" in a group
        /// of solid operations, subtracted from other solids.</param>
        /// <param name="reverseWinding">True if the extrusion should be flipped inside
        /// out, with face normals facing in instead of out. Use with caution if
        /// using with other solid operations in a representation — boolean
        /// results may be unexpected.</param>
        [JsonConstructor]
        public Extrude(Profile profile, double height, Vector3 direction, bool isVoid = false, bool reverseWinding = false)
            : base(isVoid)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (direction.Length() == 0)
                {
                    throw new ArgumentException("The extrude cannot be created. The provided direction has zero length.");
                }
            }

            this._profile = profile;
            this._height = height;
            this._direction = direction;
            this._reverseWinding = reverseWinding;

            this.PropertyChanged += (sender, args) => { UpdateGeometry(); };
            UpdateGeometry();
        }

        internal override List<SnappingPoints> CreateSnappingPoints(GeometricElement element)
        {
            var result = new List<SnappingPoints>();
            var localTransform = new Transform(Direction * Height);
            var bottomVertices = new List<Vector3>();
            // add perimeter bottom points
            result.Add(new SnappingPoints(Profile.Perimeter.Vertices, true));
            bottomVertices.AddRange(Profile.Perimeter.Vertices);
            // add perimeter top points
            result.Add(new SnappingPoints(Profile.Perimeter.TransformedPolygon(localTransform).Vertices, true));

            // add each void
            foreach (var item in Profile.Voids)
            {
                result.Add(new SnappingPoints(item.Vertices, true));
                bottomVertices.AddRange(item.Vertices);
                result.Add(new SnappingPoints(item.TransformedPolygon(localTransform).Vertices, true));
            }

            // connect top and bottom points
            foreach (var item in bottomVertices)
            {
                result.Add(new SnappingPoints(new List<Vector3> { item, localTransform.OfPoint(item) }));
            }

            return result;
        }

        private void UpdateGeometry()
        {
            this._solid = Kernel.Instance.CreateExtrude(this._profile, this._height, this._direction, this._reverseWinding);
        }
    }
}