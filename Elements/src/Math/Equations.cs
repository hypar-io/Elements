using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements
{
    public static class Equations
    {
        public static IEnumerable<double> SolveQuadratic(double a, double b, double c)
        {
            double discriminant = b * b - 4 * a * c;

            if (discriminant.ApproximatelyEquals(0))
            {
                double t = -b / (2 * a);
                yield return t;
            }
            else if (discriminant < 0)
            {
                yield break;
            }
            else
            {
                double t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
                yield return t1;
                double t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
                yield return t2;
            }
        }

        public static IEnumerable<double> SolveIterative(
            double start, double end, Func<double, double> evaluate)
        {
            var step = (end - start) / 45;
            int lastSign = 0;
            double lastFx = 0;
            double lastDelta = 0;

            for (double i = start; i < end; i += step)
            {
                var fx = evaluate(i);
                if (fx.ApproximatelyEquals(0, Vector3.EPSILON * Vector3.EPSILON))
                {
                    yield return i;
                    lastSign = 0;
                }
                else
                {
                    var sign = Math.Sign(fx);
                    if (lastSign != 0 && sign != lastSign)
                    {
                        foreach(var r in SolveIterative(i - step, i, evaluate))
                        {
                            yield return r;
                        }
                    }
                    else if (lastDelta < 0 && fx - lastFx >= 0 && Math.Min(fx, lastFx) < step * 2)
                    {
                        foreach (var r in SolveIterative(i - step * 2, i, evaluate))
                        {
                            yield return r;
                        }
                    }
                    lastSign = sign;
                }
                lastFx = fx;

                if (lastFx != 0)
                {
                    lastDelta = fx - lastFx;
                }
            }
        }
    }
}
