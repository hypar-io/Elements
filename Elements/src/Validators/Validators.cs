#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Validators
{
    [Obsolete]
    public class ElementValidator : IValidator
    {
        public Type ValidatesType => typeof(Element);

        public void PostConstruct(object obj)
        {
            var elem = (Element)obj;
            if (elem.Id == default(Guid))
            {
                elem.Id = System.Guid.NewGuid();
            }
        }

        public void PreConstruct(object[] args)
        {
            // Do nothing.
        }
    }

    [Obsolete]
    public class GeometricElementValidator : IValidator
    {
        public Type ValidatesType => typeof(GeometricElement);

        public void PostConstruct(object obj)
        {
            var geom = (GeometricElement)obj;
            if (geom.Material == null)
            {
                geom.Material = BuiltInMaterials.Default;
            }
            if (geom.Transform == null)
            {
                geom.Transform = new Transform();
            }
        }

        public void PreConstruct(object[] args)
        {
            // Do nothing.
        }
    }

    [Obsolete]
    public class ArcValidator : IValidator
    {
        public Type ValidatesType => typeof(Arc);

        public void PostConstruct(object obj)
        {
            return;
        }

        public void PreConstruct(object[] args)
        {
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

    [Obsolete]
    public class LineValidator : IValidator
    {
        public Type ValidatesType => typeof(Line);

        public void PostConstruct(object obj)
        {
            return;
        }

        public void PreConstruct(object[] args)
        {
            var start = (Vector3)args[0];
            var end = (Vector3)args[1];

            if (start.IsAlmostEqualTo(end))
            {
                throw new ArgumentException($"The line could not be created. The start and end points of the line cannot be the same: start {start}, end {end}");
            }
        }
    }

    [Obsolete]
    public class ProfileValidator : IValidator
    {
        public Type ValidatesType => typeof(Profile);

        public void PostConstruct(object obj)
        {
            // TODO: NJsonSchema does not have a way to specify that a property
            // should have a default value, but also allow a null value. Remove
            // this when we make Profile.Voids non nullable.
            var profile = (Profile)obj;
            if (profile.Voids == null)
            {
                profile.Voids = new List<Polygon>();
            }
            profile.OrientVoids();
        }

        public void PreConstruct(object[] args)
        {
            var perimeter = (Polygon)args[0];
            if (perimeter != null && !perimeter.Vertices.AreCoplanar())
            {
                throw new Exception("To construct a profile, all points must lie in the same plane.");
            }
        }
    }

    [Obsolete]
    public class MaterialValidator : IValidator
    {
        public Type ValidatesType => typeof(Material);

        public void PostConstruct(object obj)
        {
            return;
        }

        public void PreConstruct(object[] args)
        {
            var color = (Color)args[0];
            var specularFactor = (double)args[1];
            var glossinessFactor = (double)args[2];
            var unlit = (bool)args[3];
            var texture = (string)args[4];
            var doubleSided = (bool)args[5];
            var repeatTexture = (bool)args[6];
            var normalTexture = (string)args[7];
            var interpolateTexture = (bool)args[8];
            var id = (Guid)args[9];
            var name = (string)args[10];

            if (texture != null && !File.Exists(texture))
            {
                // If the file doesn't exist, set the texture to null,
                // so the material is still created.
                texture = null;
            }

            if (normalTexture != null && !File.Exists(normalTexture))
            {
                // If the file doesn't exist, set the normalTexture to null,
                // so the material is still created.
                normalTexture = null;
            }

            if (specularFactor < 0.0 || glossinessFactor < 0.0)
            {
                throw new ArgumentOutOfRangeException("The material could not be created. Specular and glossiness values must be less greater than 0.0.");
            }

            if (specularFactor > 1.0 || glossinessFactor > 1.0)
            {
                throw new ArgumentOutOfRangeException("The material could not be created. Color, specular, and glossiness values must be less than 1.0.");
            }
        }
    }

    [Obsolete]
    public class PlaneValidator : IValidator
    {
        public Type ValidatesType => typeof(Plane);

        public void PostConstruct(object obj)
        {
            var p = (Plane)obj;
            if (!p.Normal.IsUnitized())
            {
                p.Normal = p.Normal.Unitized();
            }

            return;
        }

        public void PreConstruct(object[] args)
        {
            var origin = (Vector3)args[0];
            var normal = (Vector3)args[1];

            if (normal.IsZero())
            {
                throw new ArgumentException($"The plane could not be constructed. The normal, {normal}, has zero length.");
            }
        }
    }

    [Obsolete]
    public class Vector3Validator : IValidator
    {
        public Type ValidatesType => typeof(Vector3);

        public void PostConstruct(object obj)
        {
            return;
        }

        public void PreConstruct(object[] args)
        {
            var x = (double)args[0];
            var y = (double)args[1];
            var z = (double)args[2];

            if (Double.IsNaN(x) || Double.IsNaN(y) || Double.IsNaN(z))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was NaN.");
            }

            if (Double.IsInfinity(x) || Double.IsInfinity(y) || Double.IsInfinity(z))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was infinity.");
            }
        }
    }

    [Obsolete]
    public class ColorValidator : IValidator
    {
        public Type ValidatesType => typeof(Color);

        public void PostConstruct(object obj)
        {
            return;
        }

        public void PreConstruct(object[] args)
        {
            var red = (double)args[0];
            var green = (double)args[1];
            var blue = (double)args[2];
            var alpha = (double)args[3];

            if (red < 0.0 || green < 0.0 || blue < 0.0 || alpha < 0.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value greater than 0.0.");
            }

            if (red > 1.0 || green > 1.0 || blue > 1.0 || alpha > 1.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value less than or equal to 1.0.");
            }
        }
    }

    [Obsolete]
    public class ExtrudeValidator : IValidator
    {
        public Type ValidatesType => typeof(Extrude);

        public void PostConstruct(object obj)
        {
            var extrude = (Extrude)obj;
            extrude.PropertyChanged += (sender, args) => { UpdateGeometry(extrude); };
            UpdateGeometry(extrude);
        }

        private void UpdateGeometry(Extrude extrude)
        {
            extrude._solid = Kernel.Instance.CreateExtrude(extrude.Profile, extrude.Height, extrude.Direction);
        }

        public void PreConstruct(object[] args)
        {
            var profile = (Profile)args[0];
            var height = (double)args[1];
            var direction = (Vector3)args[2];
            var isVoid = (bool)args[3];

            if (direction.Length() == 0)
            {
                throw new ArgumentException("The extrude cannot be created. The provided direction has zero length.");
            }
        }
    }

    [Obsolete]
    public class SweepValidator : IValidator
    {
        public Type ValidatesType => typeof(Sweep);

        public void PostConstruct(object obj)
        {
            var sweep = (Sweep)obj;
            sweep.PropertyChanged += (sender, args) => { UpdateGeometry(sweep); };
            UpdateGeometry(sweep);
        }

        private void UpdateGeometry(Sweep sweep)
        {
            sweep._solid = Kernel.Instance.CreateSweepAlongCurve(sweep.Profile,
                                                                 sweep.Curve,
                                                                 sweep.StartSetback,
                                                                 sweep.EndSetback,
                                                                 sweep.ProfileRotation);
        }

        public void PreConstruct(object[] args)
        {
            return;
        }
    }

    [Obsolete]
    public class LaminaValidator : IValidator
    {
        public Type ValidatesType => typeof(Lamina);

        public void PostConstruct(object obj)
        {
            var lamina = (Lamina)obj;
            lamina.PropertyChanged += (sender, args) => { UpdateGeometry(lamina); };
            UpdateGeometry(lamina);
        }

        private void UpdateGeometry(Lamina lamina)
        {
            lamina._solid = Kernel.Instance.CreateLamina(lamina.Perimeter, lamina.Voids);
        }

        public void PreConstruct(object[] args)
        {
            return;
        }
    }

    [Obsolete]
    public class MatrixValidator : IValidator
    {
        public Type ValidatesType => typeof(Matrix);

        public void PostConstruct(object obj)
        {
            return;
        }

        public void PreConstruct(object[] args)
        {
            var components = (IList<double>)args[0];
            if (components.Count != 12)
            {
                throw new ArgumentOutOfRangeException("The matrix could not be created. The component array must have 12 values.");
            }
        }
    }

    [Obsolete]
    public class PolylineValidator : IValidator
    {
        public Type ValidatesType => typeof(Polyline);

        public void PostConstruct(object obj)
        {
            if (obj is Polygon)
            {
                // we don't need to validate twice —
                // the Polygon PostConstruct validator will handle all of this correctly.
                return;
            }
            var polyline = obj as Polyline;
            polyline.Vertices = Vector3.RemoveSequentialDuplicates(polyline.Vertices);
            Polyline.CheckSegmentLengthAndThrow(polyline.Edges());
            return;
        }

        public void PreConstruct(object[] args)
        {
            return;
        }
    }

    [Obsolete]
    public class PolygonValidator : IValidator
    {
        public Type ValidatesType => typeof(Polygon);

        public void PostConstruct(object obj)
        {
            var polygon = obj as Polygon;
            polygon.Vertices = Vector3.RemoveSequentialDuplicates(polygon.Vertices, true);
            Polyline.CheckSegmentLengthAndThrow(polygon.Edges());
            var t = polygon.Vertices.ToTransform();
            Polyline.CheckSelfIntersectionAndThrow(t, polygon.Edges());
            return;
        }

        public void PreConstruct(object[] args)
        {
            var vertices = (IList<Vector3>)args[0];
            if (!vertices.AreCoplanar())
            {
                throw new ArgumentException("The polygon could not be created. The provided vertices are not coplanar.");
            }
        }
    }
}

