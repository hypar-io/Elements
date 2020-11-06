using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    public class BeamBuilder : BuilderBase
    {
        // Define defaults for a beam.

        private Curve _curve = new Line(Vector3.Origin, new Vector3(5, 0));
        private Profile _profile = new Profile(Polygon.Rectangle(0.1, 0.1));
        private Material _material = BuiltInMaterials.Steel;
        private double _rotation = 0.0;
        private Transform _transform = new Transform();
        private double _startSetback = 0.0;
        private double _endSetback = 0.0;
        private string _name = "Beam";
        private Model _model;

        public BeamBuilder(Model model, List<string> errors) : base(errors)
        {
            _model = model;
        }

        public Beam Build()
        {
            var beam = new Beam(_curve, _profile, _material, _rotation, _startSetback, _endSetback, _transform, false, Guid.NewGuid(), _name);
            _model.AddElement(beam);
            return beam;
        }

        public BeamBuilder AlongCurve(Curve curve)
        {
            _curve = curve;
            return this;
        }

        public BeamBuilder OfMaterial(Material material)
        {
            _material = material;
            return this;
        }

        public BeamBuilder Rotated(double degrees)
        {
            _rotation = degrees;
            return this;
        }

        public BeamBuilder Transformed(Transform transform)
        {
            _transform = transform;
            return this;
        }

        public BeamBuilder WithStartSetback(double distance)
        {
            if (distance > _curve.Length())
            {
                _errors.Add($"The specified setback distance, {distance}, is greater that the length of the beam. Using a default distance of 0.0.");
            }
            else
            {
                _startSetback = distance;
            }
            return this;
        }

        public BeamBuilder WithEndSetback(double distance)
        {
            if (distance > _curve.Length())
            {
                _errors.Add($"The specified setback distance, {distance}, is greater that the length of the beam. Using a default distance of 0.0.");
            }
            {
                _endSetback = distance;
            }
            return this;
        }
    }
}