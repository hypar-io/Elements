using Elements.Geometry;
using Elements.Geometry.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements
{
    public static class Equations
    {
        public static IEnumerable<double> SolveQuadratic(
            double a, double b, double c,
            double tolerance = Vector3.EPSILON)
        {
            double discriminant = b * b - 4 * a * c;

            if (discriminant.ApproximatelyEquals(0, tolerance))
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
            double start, double end, int steps,
            Func<double, double> evaluate,
            double tolerance = Vector3.EPSILON)
        {
            var step = (end - start) / steps;
            int lastSign = 0;
            double lastFx = 0;
            double lastDelta = 0;

            for (int  i = 0; i <= steps; i++)
            {
                var t = i == steps ? end : start + i * step;
                var fx = evaluate(t);
                if (fx.ApproximatelyEquals(0, tolerance))
                {
                    yield return t;
                    lastSign = 0;
                }
                else
                {
                    var sign = Math.Sign(fx);
                    if (lastSign != 0 && sign != lastSign)
                    {
                        foreach (var r in SolveIterative(t - step, t, steps, evaluate, tolerance))
                        {
                            yield return r;
                        }
                    }
                    else if (lastDelta < 0 && fx - lastFx >= 0)
                    {
                        var min = Math.Min(fx, lastFx);
                        if (min > 0 && min < step * 2)
                        {
                            foreach (var r in SolveIterative(t - step * 2, t, steps, evaluate, tolerance))
                            {
                                yield return r;
                            }
                        }
                    }
                    lastSign = sign;
                }

                if (lastFx != 0)
                {
                    lastDelta = fx - lastFx;
                }
                lastFx = fx;
            }
        }
    }
}
