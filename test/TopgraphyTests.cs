using System;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class TopographyTests: ModelTest
    {
        [Fact]
        public void Topography()
        {
            this.Name = "Topography";
            var w = 100;
            var elevations = new double[(int)Math.Pow(w+1,2)];
            var r = new Random();
            var attractor = new Vector3(10,10);
            for(var i=0; i < elevations.Length; i++)
            {
                elevations[i] = r.NextDouble() * r.NextDouble();
            }

            Func<Vector3,Color> colorizer = n => {
                var slope = n.AngleTo(Vector3.ZAxis);
                if(slope >=0.0 && slope < 15.0)
                {
                    return Colors.Green;
                }
                else if(slope >= 15.0 && slope < 30.0)
                {
                    return Colors.Yellow;
                }
                else if(slope >= 30.0 && slope < 45.0)
                {
                    return Colors.Orange;
                }
                else if(slope >= 45.0)
                {
                    return Colors.Red;
                }
                return Colors.Red;
            };

            var topo = new Topography(Vector3.Origin, 0.5, 0.5, elevations, w, colorizer);
            this.Model.AddElement(topo);
        }
    }
}