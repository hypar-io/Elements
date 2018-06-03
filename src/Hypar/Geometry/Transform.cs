namespace Hypar.Geometry
{
    public class Transform
    {
        public Vector3 Origin{get;}

        public Vector3 XAxis{get;}

        public Vector3 YAxis{get;}

        public Vector3 ZAxis{get;}

        public Transform(Vector3 origin, Vector3 xAxis, Vector3 zAxis)
        {
            this.Origin = origin;
            this.XAxis = xAxis.Normalized();
            this.ZAxis = zAxis.Normalized();
            this.YAxis = this.ZAxis.Cross(this.XAxis);
        }

        public override string ToString()
        {
            return $"X Axis:{this.XAxis}, Y Axis:{this.YAxis}, Z Axis: {this.ZAxis}";
        }
    }
}