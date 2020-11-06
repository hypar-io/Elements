using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    public class ModelCurveBuilder : BuilderBase
    {
        Curve _curve = new Line(Vector3.Origin, new Vector3(5, 5, 5));
        Model _model;

        public ModelCurveBuilder(Model model, List<string> errors) : base(errors)
        {
            _model = model;
        }

        public ModelCurve Build()
        {
            var mc = new ModelCurve(_curve);
            _model.AddElement(mc);
            return mc;
        }

        public ModelCurveBuilder FromCurve(Curve curve)
        {
            _curve = curve;
            return this;
        }
    }
}