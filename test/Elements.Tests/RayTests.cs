using System;
using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class RayTests: ModelTest
    {
        [Fact]
		public void TriangleIntersection()
		{
			var a = new Vertex(new Vector3(-0.5,-0.5, 1.0));
			var b = new Vertex(new Vector3(0.5, -0.5, 1.0));
			var c = new Vertex(new Vector3(0, 0.5, 1.0));
			var t = new Triangle(a,b,c);
			var r = new Ray(Vector3.Origin, Vector3.ZAxis);
			RayIntersectionResult xsect;
			var intersects = r.Intersects(t, out xsect);
			Assert.True(xsect.Type == RayIntersectionResultType.Intersect);

			r = new Ray(Vector3.Origin, Vector3.ZAxis.Negate());
			intersects = r.Intersects(t, out xsect);
			Assert.True(xsect.Type == RayIntersectionResultType.Behind);
		}

		[Fact]
		public void IntersectsAtVertex()
		{
			var a = new Vertex(new Vector3(-0.5,-0.5, 1.0));
			var b = new Vertex(new Vector3(0.5, -0.5, 1.0));
			var c = new Vertex(new Vector3(0, 0.5, 1.0));
			var t = new Triangle(a,b,c);
			var r = new Ray(new Vector3(-0.5, -0.5, 0.0), Vector3.ZAxis);
			RayIntersectionResult xsect;
			var intersects = r.Intersects(t, out xsect);
			Assert.True(xsect.Type == RayIntersectionResultType.IntersectsAtVertex);
		}

		[Fact]
		public void IsParallelTo()
		{
			var a = new Vertex(new Vector3(-0.5,-0.5, 1.0));
			var b = new Vertex(new Vector3(0.5, -0.5, 1.0));
			var c = new Vertex(new Vector3(0, 0.5, 1.0));
			var t = new Triangle(a,b,c);
			var r = new Ray(Vector3.Origin, Vector3.XAxis);
			RayIntersectionResult xsect;
			var intersects = r.Intersects(t, out xsect);
			Assert.True(xsect.Type == RayIntersectionResultType.Parallel);
		}

		[Fact]
		public void OriginRayIntersectsTopography()
		{
			this.Name = "RayIntersectTopo";

			var elevations = new double[100];

			int e = 0;
			for(var x=0; x<10; x++)
			{
				for(var y=0; y<10; y++)
				{
					elevations[e] = Math.Sin(((double)x/10.0)*Math.PI) * 10;
					e++;
				}
			}
			var topo = new Topography(Vector3.Origin, 1.0, 1.0, elevations, 9, (tri)=>{return Colors.White;});
			this.Model.AddElement(topo);

			var ray = new Ray(Vector3.Origin, Vector3.ZAxis);
			this.Model.AddElement(ModelCurveFromRay(ray));

			RayIntersectionResult xsect;
			ray.Intersects(topo, out xsect);
			Assert.True(xsect.Type == RayIntersectionResultType.IntersectsAtVertex);

			this.Model.AddElement(ModelPointsFromIntersection(xsect));
		}

		private static ModelCurve ModelCurveFromRay(Ray r)
		{
			var line = new Line(r.Origin, r.Origin + r.Direction * 20);
			return new ModelCurve(line);		
		}

		private static ModelPoints ModelPointsFromIntersection(RayIntersectionResult xsect)
		{
			return new ModelPoints(new List<Vector3>(){xsect.Point});
		}
    }
}