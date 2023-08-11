using Elements.Geometry;
using Elements.Geometry.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Elements
{
    public class CurveTests
    {
        [Fact]
        public void CurveIntersectsNotThrows()
        {
            ICurve left = new InfiniteLine(Vector3.Origin, Vector3.XAxis);
            ICurve right = new Circle(Vector3.Origin, 4);
            left.Intersects(right, out _);
            right.Intersects(left, out _);
            right = new Ellipse(Vector3.Origin, 4, 5);
            left.Intersects(right, out _);
            right.Intersects(left, out _);
            left = new InfiniteLine(Vector3.Origin, Vector3.YAxis);
            left.Intersects(right, out _);

            right = new Arc(Vector3.Origin, 3, 45, 90);
            left.Intersects(right, out _);
            right.Intersects(left, out _);
            left = new Line(new Vector3(0, 4, 0), new Vector3(2, 2, 0));
            left.Intersects(right, out _);
            right.Intersects(left, out _);

            right = new Polygon(new Vector3[]
            {
                (2, 2), (4, 2), (4, 4), (2, 4)
            });
            left.Intersects(right, out _);
            right.Intersects(left, out _);

            left = new Circle(Vector3.Origin, 4);
            left.Intersects(right, out _);
            right.Intersects(left, out _);

            left = new IndexedPolycurve(new List<BoundedCurve>
            {
                new Line((0, 0), (0, 5)),
                new Arc((0, 10), 5, 180, 0),
                new Line((0, 10), (0, 15))
            });
            left.Intersects(right, out _);
            right.Intersects(left, out _);

            left = new Bezier(new List<Vector3>()
            {
                (0, 0),
                (1, 2),
                (3, 3)
            });

            //left.Intersects(right, out _);
            //right.Intersects(left, out _);

            //right = new Line((0, 0), (10, 10));
            //left.Intersects(right, out _);
            //right.Intersects(left, out _);
        }

        [Fact]
        public void DynamicsCalls()
        {
            Random r = new Random();
            ICurve left = new Line(Vector3.Origin, Vector3.XAxis);
            for (int i = 0; i < 500_000; i++)
            {
                ICurve right = new Arc(Vector3.Origin, r.NextDouble(), -45, 90);
                left.Intersects(right, out _);
            }
        }

        [Fact]
        public void DirectCalls()
        {
            Random r = new Random();
            Line left = new Line(Vector3.Origin, Vector3.XAxis);
            for (int i = 0; i < 500_000; i++)
            {
                Arc right = new Arc(Vector3.Origin, r.NextDouble(), -45, 90);
                left.Intersects(right, out _);
            }
        }
    }
}
