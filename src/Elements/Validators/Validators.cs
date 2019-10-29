#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Validators
{
    public class ArcValidator : IValidator
    {
        public Type ValidatesType => typeof(Arc);

        public void Validate(object[] args)
        {
            //Vector3 @center, double @radius, double @startAngle, double @endAngle
            var center = (Vector3)args[0];
            var radius = (double)args[1];
            var startAngle = (double)args[2];
            var endAngle = (double)args[3];
            
            if (endAngle > 360.0 || startAngle > 360.00)
            {
                throw new ArgumentOutOfRangeException("The arc could not be created. The start and end angles must be greater than -360.0");
            }

            if (endAngle == startAngle)
            {
                throw new ArgumentException($"The arc could not be created. The start angle ({startAngle}) cannot be equal to the end angle ({endAngle}).");
            }

            if (radius <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The arc could not be created. The provided radius ({radius}) must be greater than 0.0.");
            }
        }
    }

    public class LineValidator : IValidator
    {
        public Type ValidatesType => typeof(Line);

        public void Validate(object[] args)
        {
            var start= (Vector3)args[0];
            var end = (Vector3)args[1];

            if (start.IsAlmostEqualTo(end))
            {
                throw new ArgumentException($"The line could not be created. The start and end points of the line cannot be the same: start {start}, end {end}");
            }
        }
    }

    public class ProfileValidator : IValidator
    {
        public Type ValidatesType => typeof(Profile);

        public void Validate(object[] args)
        {
            var perimeter = (Polygon)args[0];
            if (perimeter != null && !perimeter.Vertices.AreCoplanar())
            {
                throw new Exception("To construct a profile, all points must line in the same plane.");
            }
        }
    }

    public class MaterialValidator : IValidator
    {
        public Type ValidatesType => typeof(Material);

        public void Validate(object[] args)
        {
            var red = (Color)args[0];
            var specularFactor = (double)args[1];
            var glossinessFactor = (double)args[2];
            var id = (Guid)args[3];
            var name = (string)args[4];
            
            if(specularFactor < 0.0 || glossinessFactor < 0.0)
            {
                throw new ArgumentOutOfRangeException("The material could not be created. Specular and glossiness values must be less greater than 0.0.");
            }

            if(specularFactor > 1.0 || glossinessFactor > 1.0)
            {
                throw new ArgumentOutOfRangeException("The material could not be created. Color, specular, and glossiness values must be less than 1.0.");
            }
        }
    }

    public class PlaneValidator : IValidator
    {
        public Type ValidatesType => typeof(Plane);

        public void Validate(object[] args)
        {
            var origin = (Vector3)args[0];
            var normal = (Vector3)args[1];

            if(normal.IsParallelTo(origin))
            {
                throw new ArgumentException("The plane could not be constructed. The normal and origin are parallel.");
            }
        }
    }

    public class Vector3Validator : IValidator
    {
        public Type ValidatesType => typeof(Vector3);

        public void Validate(object[] args)
        {
            var x = (double)args[0];
            var y = (double)args[1];
            var z = (double)args[2];

            if(Double.IsNaN(x) || Double.IsNaN(y) || Double.IsNaN(z))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was NaN.");
            }

            if(Double.IsInfinity(x) || Double.IsInfinity(y) || Double.IsInfinity(z))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was infinity.");
            }
        }
    }

    public class ColorValidator : IValidator
    {
        public Type ValidatesType => typeof(Color);

        public void Validate(object[] args)
        {
            var red = (double)args[0];
            var green = (double)args[1];
            var blue = (double)args[2];
            var alpha = (double)args[3];

            if(red < 0.0 || green < 0.0 || blue < 0.0 || alpha < 0.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value greater than 0.0.");
            }

            if(red > 1.0 || green > 1.0 || blue > 1.0 || alpha > 1.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value less than 1.0.");
            }
        }
    }

    public class ExtrudeValidator : IValidator
    {
        public Type ValidatesType => typeof(Extrude);

        public void Validate(object[] args)
        {
            var profile = (Profile)args[0];
            var height = (double)args[1];
            var direction = (Vector3)args[2];
            var rotation = (double)args[3];
            var isVoid = (bool)args[4];

            if(direction.Length() == 0)
            {
                throw new ArgumentException("The extrude cannot be created. The provided direction has zero length.");
            }
        }
    }

    public class MatrixValidator : IValidator
    {
        public Type ValidatesType => typeof(Matrix);

        public void Validate(object[] args)
        {
            var components = (IList<double>)args[0];
            if(components.Count != 12)
            {
                throw new ArgumentOutOfRangeException("The matrix could not be created. The component array must have 16 values.");
            }
        }
    }

    public class PolylineValidator : IValidator
    {
        public Type ValidatesType => typeof(Polyline);

        public void Validate(object[] args)
        {
            var vertices = (IList<Vector3>)args[0];

            if(!vertices.AreCoplanar())
            {
                throw new ArgumentException("The polygon could not be created. The provided vertices are not coplanar.");
            }

            var segments = Polyline.SegmentsInternal(vertices);
            Polyline.CheckSegmentLengthAndThrow(segments);
        }
    }

    public class PolygonValidator : IValidator
    {
        public Type ValidatesType => typeof(Polygon);

        public void Validate(object[] args)
        {
            var vertices = (IList<Vector3>)args[0];

            if(!vertices.AreCoplanar())
            {
                throw new ArgumentException("The polygon could not be created. The provided vertices are not coplanar.");
            }

            var segments = Polygon.SegmentsInternal(vertices);
            Polyline.CheckSegmentLengthAndThrow(segments);

            var t = vertices.ToTransform();
            Polyline.CheckSelfIntersectionAndThrow(t, segments);
        }
    }
}

