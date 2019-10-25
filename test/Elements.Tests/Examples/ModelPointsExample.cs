using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class ModelPointsExample : ModelTest
    {
        [Fact]
        public void Example()
        {
            this.Name = "Elements_ModelPoints";

            // <example>
            // Create an empty model points object.
            var pink = new Material("pink", Colors.Pink);
            var modelPoints = new ModelPoints(pink);
            for(var x=0; x<25; x++)
            {
                for(var y=0; y<25; y++)
                {
                    for(var z=0; z<25; z++)
                    {   
                        // Add points to the object.
                        modelPoints.Locations.Add(new Vector3(x,y,z));
                    }
                }
            }
            // </example>
            this.Model.AddElement(modelPoints);            
        }
    }
}