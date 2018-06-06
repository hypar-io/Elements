namespace Hypar.Geometry
{
    public class Color
    {
        double Red{get;}
        double Green{get;}
        double Blue{get;}
        double Alpha{get;}
        public Color(double red, double green, double blue, double alpha)
        {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.Alpha = alpha;
        }

        public double[] ToArray()
        {
            return new[]{Red, Green, Blue, Alpha};
        }
    }
}