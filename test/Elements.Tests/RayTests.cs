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
			Vector3 xsect;
			Assert.True(r.Intersects(t, out xsect));

			r = new Ray(Vector3.Origin, Vector3.ZAxis.Negate());
			Assert.False(r.Intersects(t, out xsect));
		}

		[Fact]
		public void IntersectsAtVertex()
		{
			var a = new Vertex(new Vector3(-0.5,-0.5, 1.0));
			var b = new Vertex(new Vector3(0.5, -0.5, 1.0));
			var c = new Vertex(new Vector3(0, 0.5, 1.0));
			var t = new Triangle(a,b,c);
			var r = new Ray(new Vector3(-0.5, -0.5, 0.0), Vector3.ZAxis);
			Vector3 xsect;
			Assert.True(r.Intersects(t, out xsect));
		}

		[Fact]
		public void IsParallelTo()
		{
			var a = new Vertex(new Vector3(-0.5,-0.5, 1.0));
			var b = new Vertex(new Vector3(0.5, -0.5, 1.0));
			var c = new Vertex(new Vector3(0, 0.5, 1.0));
			var t = new Triangle(a,b,c);
			var r = new Ray(Vector3.Origin, Vector3.XAxis);
			Vector3 xsect;
			Assert.False(r.Intersects(t, out xsect));
		}

		[Fact]
		public void RayIntersectsTopography()
		{
			this.Name = "RayIntersectTopo";

			var elevations = new double[25];

			int e = 0;
			for(var x=0; x<5; x++)
			{
				for(var y=0; y<5; y++)
				{
					elevations[e] = Math.Sin(((double)x/5.0)*Math.PI) * 10;
					e++;
				}
			}
			var topo = new Topography(Vector3.Origin, 1.0, 1.0, elevations, 4, (tri)=>{return Colors.White;});
			this.Model.AddElement(topo);

			var modelPoints = new ModelPoints(new List<Vector3>(), new Material("begin", Colors.Blue));
			this.Model.AddElement(modelPoints);
			foreach(var t in topo.Mesh.Triangles)
			{
				var c = Center(t);
				var o = new Vector3(c.X, c.Y);
				modelPoints.Locations.Add(o);

				var ray = new Ray(o, Vector3.ZAxis);
				
				Vector3 xsect;
				if(ray.Intersects(t, out xsect))
				{
					try
					{
						var l = new Line(o, xsect);
						var ml = new ModelCurve(l);
						this.Model.AddElement(ml);
					}
					catch
					{
						continue;
					}
				}
			}
		}

		private static Vector3 Center(Triangle t)
		{
			return new Vector3[]{t.Vertices[0].Position, t.Vertices[1].Position, t.Vertices[2].Position}.Average();
		}


		private static ModelCurve ModelCurveFromRay(Ray r)
		{
			var line = new Line(r.Origin, r.Origin + r.Direction * 20);
			return new ModelCurve(line);		
		}

		private static ModelPoints ModelPointsFromIntersection(Vector3 xsect)
		{
			return new ModelPoints(new List<Vector3>(){xsect});
		}
    }
}