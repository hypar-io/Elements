namespace Hypar.Geometry
{
    public class Vector2
    {
        public double X{get;}
        public double Y{get;}

        public Vector2()
        {
            this.X = 0.0;
            this.Y = 0.0;
        }
        
        public Vector2(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"X:{X},Y:{Y}";
        }
    }
}