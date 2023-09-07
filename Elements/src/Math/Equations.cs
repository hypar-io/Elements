using Elements.Geometry;
using Elements.Geometry.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements
{
    /// <summary>
    /// Finding roots of a function.
    /// </summary>
    public static class Equations
    {
        /// <summary>
        /// Solve a quadratic equation and return the roots if any.
        /// https://en.wikipedia.org/wiki/Quadratic_equation
        /// </summary>
        /// <param name="a">A parameter of quadratic equation.</param>
        /// <param name="b">B parameter of quadratic equation.</param>
        /// <param name="c">C parameter of quadratic equation.</param>
        /// <param name="tolerance">Zero discriminant tolerance.</param>
        /// <returns>One or two roots if equation can be solved, empty if it can't be.</returns>
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

        /// <summary>
        /// Solve equation by iterating through parametric range with certain step.
        /// At each step only one root can be found.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="steps"></param>
        /// <param name="evaluate"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static IEnumerable<double> SolveIterative(
            double start, double end, int steps,
            Func<double, double> evaluate,
            double tolerance = Vector3.EPSILON)
        {
            var step = (end - start) / steps;
            int lastSign = 0;
            double lastFx = 0;
            double lastDelta = 0;

            for (int i = 0; i <= steps; i++)
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
                        var r = FindCrossingRoot(t - step, lastFx, t, fx, evaluate, tolerance);
                        yield return r;
                    }
                    else if (lastDelta < 0 && fx - lastFx >= 0 && Math.Min(fx, lastFx) > 0)
                    {
                        if (TryFindTouchingRoot(t - step * 2, t, evaluate, tolerance, out var r))
                        {
                            yield return r;
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

        private static double FindCrossingRoot(
            double start, double startFn, double end, double endFn,
            Func<double, double> evaluate,
            double tolerance)
        {
            while (true)
            {
                // https://en.wikipedia.org/wiki/Secant_method
                var t = end - endFn * (end - start) / (endFn - startFn);
                var tFn = evaluate(t);
                if (tFn.ApproximatelyEquals(0, tolerance))
                {
                    return t;
                }

                start = end;
                startFn = endFn;
                end = t;
                endFn = tFn;
            }
        }

        private static bool TryFindTouchingRoot(
            double start, double end,
            Func<double, double> evaluate,
            double tolerance,
            out double t)
        {
            t = 0;
            var startFn = evaluate(start);
            var endFn = evaluate(end);
            while (true)
            {
                var d = (end - start) / 3;
                var t0 = start + d;
                var t0Fn = evaluate(t0);
                if (t0Fn.ApproximatelyEquals(0, tolerance))
                {
                    t = t0;
                    return true;
                }

                var t1 = start + d * 2;
                var t1Fn = evaluate(t1);
                if (t1Fn.ApproximatelyEquals(0, tolerance))
                {
                    t = t1;
                    return true;
                }

                if (t0Fn.ApproximatelyEquals(t1Fn, tolerance))
                {
                    // Touching root can't be searched forever.
                    // Stop if function value is more than searching domain.
                    // This may fail if curve has sharp angles.
                    var factor = Math.Max(startFn - t0Fn, endFn - t1Fn);
                    if (t0Fn > factor * 2)
                    {
                        break;
                    }

                    start = t0;
                    startFn = t0Fn;
                    end = t1;
                    endFn = t1Fn;
                }
                else if (t0Fn > t1Fn)
                {
                    var factor = Math.Max(t0Fn - t1Fn, endFn - t1Fn);
                    if (t1Fn > factor * 2)
                    {
                        break;
                    }
                    start = t0;
                    startFn = t0Fn;
                }
                else
                {
                    var factor = Math.Max(startFn - t0Fn, t0Fn - t0Fn);
                    if (t0Fn > factor * 2)
                    {
                        break;
                    }
                    end = t1;
                    endFn = t1Fn;
                }
            }
            return false;
        }
    }
}
