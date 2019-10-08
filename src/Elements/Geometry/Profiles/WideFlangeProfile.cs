#pragma warning disable CS1591

using System;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    public class WideFlangeProfile : Profile
    {
        [JsonIgnore]
        public double A {get; internal set;}

        public double d {get; internal set;}

        public double tw {get;internal set;}

        public double bf {get;internal set;}

        public double tf {get;internal set;}

        [JsonIgnore]
        public string T {get;internal set;}

        [JsonIgnore]
        public double k {get;internal set;}

        [JsonIgnore]
        public double k1 {get;internal set;}

        [JsonIgnore]
        public string gage {get;internal set;}

        [JsonIgnore]
        public double rt {get;internal set;}

        [JsonIgnore]
        public double dAf {get;internal set;}

        [JsonIgnore]
        public double Ix {get;internal set;}

        [JsonIgnore]
        public double Sx {get;internal set;}

        [JsonIgnore]
        public double rx {get;internal set;}

        [JsonIgnore]
        public double Iy {get;internal set;}

        [JsonIgnore]
        public double Sy {get;internal set;}

        [JsonIgnore]
        public double ry {get;internal set;}

        [JsonIgnore]
        public double Zx {get;internal set;}

        [JsonIgnore]
        public double Zy {get;internal set;}

        [JsonIgnore]
        public double J {get;internal set;}

        [JsonIgnore]
        public double Cw {get;internal set;}

        [JsonIgnore]
        public double Wno {get;internal set;}

        [JsonIgnore]
        public double Sw {get;internal set;}

        [JsonIgnore]
        public double Qf {get;internal set;}

        [JsonIgnore]
        public double Qw {get;internal set;}

        public WideFlangeProfile(string name, double bf = 0.1, double d = 0.05, double tf = 0.005, double tw = 0.005, 
                                    VerticalAlignment verticalAlignment = VerticalAlignment.Center, 
                                    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, 
                                    double verticalOffset = 0.0, double horizontalOffset = 0.0) : base(name)
        {
            this.bf = bf;
            this.d = d;
            this.tf = tf;
            this.tw = tw;
            this.Perimeter = CreateProfile(bf, d, tf, tw, verticalAlignment, horizontalAlignment, verticalOffset, horizontalOffset);
        }

        [JsonConstructor]
        internal WideFlangeProfile(Guid id, string name, double bf = 0.1, double d = 0.05, double tf = 0.005, double tw = 0.005, 
                                    VerticalAlignment verticalAlignment = VerticalAlignment.Center, 
                                    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, 
                                    double verticalOffset = 0.0, double horizontalOffset = 0.0) : base(id, name)
        {
            this.bf = bf;
            this.d = d;
            this.tf = tf;
            this.tw = tw;
            this.Perimeter = CreateProfile(bf, d, tf, tw, verticalAlignment, horizontalAlignment, verticalOffset, horizontalOffset);
        }

        public WideFlangeProfile(string name) : base(name)
        {
            this.Perimeter = CreateProfile(0.1, 0.05, 0.005, 0.005, VerticalAlignment.Center, HorizontalAlignment.Center, 0.0, 0.0);
        }

        private Polygon CreateProfile(double bf, double d, double tf, double tw, 
                                    VerticalAlignment verticalAlignment, 
                                    HorizontalAlignment horizontalAlignment, 
                                    double verticalOffset, double horizontalOffset)
        {
            var o = new Vector3();

            var height = d;
            var width = bf;
            var thicknessWeb = tw;
            var thicknessFlange = tf;

            if(verticalOffset == 0.0)
            {
                switch(verticalAlignment)
                {
                    case VerticalAlignment.Top:
                        verticalOffset = height/2;
                        break;
                    case VerticalAlignment.Center:
                        verticalOffset = 0.0;
                        break;
                    case VerticalAlignment.Bottom:
                        verticalOffset = -height/2;
                    break;
                }
            }

            if(horizontalOffset == 0.0)
            {
                switch(horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        horizontalOffset = -width/2;
                        break;
                    case HorizontalAlignment.Center:
                        horizontalOffset = 0.0;
                        break;
                    case HorizontalAlignment.Right:
                        horizontalOffset = width/2;
                        break;
                }
            }

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

            return new Polygon(new []{a,b,c,e,f,g,h,i,j,k,l,m});
        }
    }
}