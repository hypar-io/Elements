using Hypar.Geometry;

namespace Hypar.Elements
{
    public static class Profiles
    {
        public static Polyline Square(Vector3 origin = null, double width = 1.0, double height = 1.0, double vo = 0.0, double ho = 0.0)
        {
            if(origin == null)
            {
                origin = Vector3.Origin();
            }

            var a = new Vector3(origin.X - width/2 + ho, origin.Y - height/2 + vo);
            var b = new Vector3(origin.X + width/2 + ho, origin.Y - height/2 + vo);
            var c = new Vector3(origin.X + width/2 + ho, origin.Y + height/2 + vo);
            var d = new Vector3(origin.X - width/2 + ho, origin.Y + height/2 + vo);

            return new Polyline(new []{a, b, c, d});
        }

        public static Polyline WideFlangeProfile(double width = 0.1, double height = 0.05, double thicknessFlange = 0.005, double thicknessWeb = 0.005, double verticalOffset = 0.0, double horizontalOffset = 0.0)
        {
            var o = new Vector3();
            // Left
            var a = new Vector3(o.X - width/2 + horizontalOffset, o.Y + height/2 + verticalOffset);
            var b = new Vector3(o.X - width/2 + horizontalOffset, o.Y + height/2 - thicknessFlange + verticalOffset);
            var c = new Vector3(o.X - thicknessWeb/2 + horizontalOffset, o.Y + height/2 - thicknessFlange + verticalOffset);
            var e = new Vector3(o.X - thicknessWeb/2 + horizontalOffset, o.Y - height/2 + thicknessFlange + verticalOffset);
            var f = new Vector3(o.X - width/2 + horizontalOffset, o.Y - height/2 + thicknessFlange + verticalOffset);
            var g = new Vector3(o.X - width/2 + horizontalOffset, o.Y - height/2 + verticalOffset);

            // Right
            var h = new Vector3(o.X + width/2 + horizontalOffset, o.Y - height/2 + verticalOffset);
            var i = new Vector3(o.X + width/2 + horizontalOffset, o.Y - height/2 + thicknessFlange + verticalOffset);
            var j = new Vector3(o.X + thicknessWeb/2 + horizontalOffset, o.Y - height/2 + thicknessFlange + verticalOffset);
            var k = new Vector3(o.X + thicknessWeb/2 + horizontalOffset, o.Y + height/2 - thicknessFlange + verticalOffset);
            var l = new Vector3(o.X + width/2 + horizontalOffset, o.Y + height/2 - thicknessFlange + verticalOffset);
            var m = new Vector3(o.X + width/2 + horizontalOffset, o.Y + height/2 + verticalOffset);

            return new Polyline(new []{a,b,c,e,f,g,h,i,j,k,l,m});
        }
    }
}