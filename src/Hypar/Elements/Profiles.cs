using Hypar.Geometry;

namespace Hypar.Elements
{
    public abstract class StructuralProfile
    {
        public double Width{get;}
        public double Depth{get;}

        public double VerticalOffset{get;}

        public double HorizontalOffset{get;}

        public Polygon2 Profile{get; protected set;}

        public StructuralProfile(double w, double d, double verticalOffset = 0.0, double horizontalOffset = 0.0)
        {
            this.Width = w;
            this.Depth = d;
            this.VerticalOffset = verticalOffset;
            this.HorizontalOffset = horizontalOffset;
        }
    }

    public class WideFlangeProfile : StructuralProfile
    {
        public WideFlangeProfile(double w, double d, double tf, double tw, double vo = 0.0, double ho = 0.0) : base(w, d, vo, ho)
        {
            var o = new Vector2();
            // Left
            var a = new Vector2(o.X - w/2 + ho, o.Y + d/2 + vo);
            var b = new Vector2(o.X - w/2 + ho, o.Y + d/2 - tf + vo);
            var c = new Vector2(o.X - tw/2 + ho, o.Y + d/2 - tf + vo);
            var e = new Vector2(o.X - tw/2 + ho, o.Y - d/2 + tf + vo);
            var f = new Vector2(o.X - w/2 + ho, o.Y - d/2 + tf + vo);
            var g = new Vector2(o.X - w/2 + ho, o.Y - d/2 + vo);

            // Right
            var h = new Vector2(o.X + w/2 + ho, o.Y - d/2 + vo);
            var i = new Vector2(o.X + w/2 + ho, o.Y - d/2 + tf + vo);
            var j = new Vector2(o.X + tw/2 + ho, o.Y - d/2 + tf + vo);
            var k = new Vector2(o.X + tw/2 + ho, o.Y + d/2 - tf + vo);
            var l = new Vector2(o.X + w/2 + ho, o.Y + d/2 - tf + vo);
            var m = new Vector2(o.X + w/2 + ho, o.Y + d/2 + vo);

            this.Profile = new Polygon2(new []{a,b,c,e,f,g,h,i,j,k,l,m});
        }
    }

    public class SquareProfile : StructuralProfile
    {
        public SquareProfile(double w, double d, double vo = 0.0, double ho = 0.0) : base (w, d, vo, ho)
        {
            var origin = new Vector2();
            var a = new Vector2(origin.X - w/2 + ho, origin.Y - d/2 + vo);
            var b = new Vector2(origin.X + w/2 + ho, origin.Y - d/2 + vo);
            var c = new Vector2(origin.X + w/2 + ho, origin.Y + d/2 + vo);
            var e = new Vector2(origin.X - w/2 + ho, origin.Y + d/2 + vo);

            this.Profile = new Polygon2(new []{a, b, c, e});
        }
    }

    public static class Profiles
    {
        public static Polygon2 Square(Vector2 origin, double width, double length)
        {
            var a = new Vector2(origin.X - width/2, origin.Y - length/2);
            var b = new Vector2(origin.X + width/2, origin.Y - length/2);
            var c = new Vector2(origin.X + width/2, origin.Y + length/2);
            var d = new Vector2(origin.X - width/2, origin.Y + length/2);
            return new Polygon2(new []{a, b, c, d});
        }
    }
}