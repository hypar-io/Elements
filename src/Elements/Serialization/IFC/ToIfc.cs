using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using IFC;

namespace Elements.Serialization.IFC
{
    public static partial class IFCExtensions
    {
        private static IfcCurve ToIfcCurve(this ICurve curve, Document doc)
        {
            if (curve is Line)
            {
                return ((Line)curve).ToIfcTrimmedCurve();
            }
            else if(curve is Arc)
            {
                return ((Arc)curve).ToIfcTrimmedCurve(doc);
            }
            else if(curve is Polyline)
            {
                return ((Polyline)curve).ToIfcPolyline(doc);
            }
            else if(curve is Polygon)
            {
                return ((Polygon)curve).ToIfcPolyline(doc);
            }
            else
            {
                throw new Exception($"The curve type, {curve.GetType()}, is not yet supported.");
            }
        }

        private static IfcProduct ConvertElementToIfcProduct(dynamic element, IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            try
            {
                var e = element.ToIfc(localPlacement, shape);
                return e;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.GetType()} cannot be serialized to IFC.");
            }
            return null;
        }

        private static IfcExtrudedAreaSolid ToIfcExtrudedAreaSolid(this IExtrude extrude, Transform transform, Document doc)
        {
            var position = transform.ToIfcAxis2Placement3D(doc);

            double extrudeDepth = 0.0;
            IfcArbitraryClosedProfileDef extrudeProfile = null;
            IfcDirection extrudeDirection = null;

            if (extrude is StandardWall)
            {
                // TODO: Implement boolean operations so
                // that we don't have to keep doing this.

                var w = (StandardWall)extrude;

                // We don't use the Wall's transform for positioning, because
                // our walls have a transform that lays the wall "flat". Just
                // use an identity transform or a transform that includes
                // the elevation.
                extrudeProfile = w.CenterLine.Thicken(w.Thickness()).ToIfcArbitraryClosedProfileDef(doc);
                extrudeDirection = Vector3.ZAxis.ToIfcDirection();
                extrudeDepth = w.Height;
            } 
            else
            {
                extrudeProfile = extrude.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
                extrudeDirection = extrude.ExtrudeDirection.ToIfcDirection();
                extrudeDepth = extrude.ExtrudeDepth;
            }

            var solid = new IfcExtrudedAreaSolid(extrudeProfile, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(extrude.ExtrudeDepth));
            doc.AddEntity(extrudeProfile);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(solid);

            return solid;
        }

        private static IfcSurfaceCurveSweptAreaSolid ToIfcSurfaceCurveSweptAreaSolid(this ISweepAlongCurve sweep, Transform transform, Document doc)
        {
            var position = transform.ToIfcAxis2Placement3D(doc);
            var sweptArea = sweep.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var directrix = sweep.Curve.ToIfcCurve(doc);
            var curvePlane = sweep.Curve.ToPlane();
            var elementarySurface = curvePlane.ToIfcPlane(doc);
            
            var solid = new IfcSurfaceCurveSweptAreaSolid(sweptArea, position, directrix, sweep.StartSetback, sweep.EndSetback, elementarySurface);
            
            doc.AddEntity(position);
            doc.AddEntity(sweptArea);
            doc.AddEntity(directrix);
            doc.AddEntity(elementarySurface);
            doc.AddEntity(solid);

            return solid;
        }

        private static Plane ToPlane(this ICurve curve)
        {
            if (curve is Line)
            {
                var l = (Line)curve;
                if(l.Direction() == Vector3.ZAxis)
                {
                    return new Plane(l.Start, Vector3.ZAxis);
                }
                
                var normal = l.Direction().Cross(Vector3.ZAxis);
                return new Plane(l.Start, normal);

            }
            else if(curve is Arc)
            {
                return ((Arc)curve).Plane;
            }
            else if(curve is Polyline)
            {
                return ((Polyline)curve).Plane();
            }
            else if(curve is Polygon)
            {
                return ((Polygon)curve).Plane();
            }
            else
            {
                throw new Exception($"The curve type, {curve.GetType()}, is not yet supported.");
            }
        }

        private static IfcProductDefinitionShape ToIfcProductDefinitionShape(this IfcSweptAreaSolid solid, IfcRepresentationContext context, Document doc)
        {
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", 
                new List<IfcRepresentationItem>{solid});
            var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});

            doc.AddEntity(rep);
            doc.AddEntity(productRep);

            return productRep;
        }

        private static IfcSlab ToIfc(this Floor floor, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var slab = new IfcSlab(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, null, null, 
                null, localPlacement, shape, null, IfcSlabTypeEnum.FLOOR);
            return slab;
        }

        private static IfcSpace ToIfc(this Space space, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcSpace = new IfcSpace(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, null, null, 
                null, localPlacement, shape, null, IfcElementCompositionEnum.ELEMENT, IfcInternalOrExternalEnum.NOTDEFINED, 
                new IfcLengthMeasure(space.Transform.Origin.Z));
            return ifcSpace;
        }

        private static IfcProduct ToIfc(this StandardWall wall, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcWall = new IfcWallStandardCase(IfcGuid.ToIfcGuid(Guid.NewGuid()), 
                null, wall.Name, null, null, localPlacement, shape, null);
            return ifcWall;
        }

        private static IfcWall ToIfc(this Wall wall, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcWall = new IfcWall(IfcGuid.ToIfcGuid(Guid.NewGuid()), 
                null, wall.Name, null, null, localPlacement, shape, null);
            return ifcWall;
        }

        private static IfcBeam ToIfc(this Beam beam, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var line = beam.Curve as Line;
            if(line == null) {
                throw new Exception("The beam could not be exported to IFC. Only linear beams are currently supported.");
            }
            
            var ifcBeam = new IfcBeam(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, 
                null, null, null, localPlacement, shape, null);
            return ifcBeam;
        }

        private static IfcArbitraryClosedProfileDef ToIfcArbitraryClosedProfileDef(this Polygon polygon, Document doc)
        {
            var pline = polygon.ToIfcPolyline(doc);
            var profile = new IfcArbitraryClosedProfileDef(IfcProfileTypeEnum.AREA, pline);
            doc.AddEntity(pline);
            return profile;
        }

        private static IfcPolyline ToIfcPolyline(this Polygon polygon, Document doc)
        {
            var points = new List<IfcCartesianPoint>();
            foreach(var v in polygon.Vertices)
            {
                var p = v.ToIfcCartesianPoint();
                doc.AddEntity(p);
                points.Add(p);
            }
            return new IfcPolyline(points);
        }

        private static IfcPolyline ToIfcPolyline(this Polyline polygon, Document doc)
        {
            var points = new List<IfcCartesianPoint>();
            foreach(var v in polygon.Vertices)
            {
                var p = v.ToIfcCartesianPoint();
                doc.AddEntity(p);
                points.Add(p);
            }
            return new IfcPolyline(points);
        }

        private static IfcTrimmedCurve ToIfcTrimmedCurve(this Line line)
        {
            var ifcLine = new IfcLine(line.Start.ToIfcCartesianPoint(), line.Direction().ToIfcVector());
            var trim1 = new IfcTrimmingSelect(line.Start.ToIfcCartesianPoint());
            var trim2 = new IfcTrimmingSelect(line.End.ToIfcCartesianPoint());
            var tc = new IfcTrimmedCurve(ifcLine, new List<IfcTrimmingSelect>{trim1}, new List<IfcTrimmingSelect>{trim2}, true, IfcTrimmingPreference.CARTESIAN);
            return tc;
        }

        private static IfcTrimmedCurve ToIfcTrimmedCurve(this Arc arc, Document doc)
        {
            var t = new Transform(arc.Plane.Origin, arc.Plane.Normal);
            var ifcCircle = new IfcCircle(new IfcAxis2Placement(t.ToIfcAxis2Placement3D(doc)), new IfcPositiveLengthMeasure(arc.Radius));
            var trim1 = new IfcTrimmingSelect(arc.Start.ToIfcCartesianPoint());
            var trim2 = new IfcTrimmingSelect(arc.End.ToIfcCartesianPoint());
            var tc = new IfcTrimmedCurve(ifcCircle, new List<IfcTrimmingSelect>{trim1}, new List<IfcTrimmingSelect>{trim2}, true, IfcTrimmingPreference.CARTESIAN);
            return tc;
        }

        private static IfcVector ToIfcVector(this Vector3 v)
        {
            return new IfcVector(v.ToIfcDirection(), new IfcLengthMeasure(0));
        }

        private static IfcLocalPlacement ToIfcLocalPlacement(this Transform transform, Document doc)
        {
            var placement = transform.ToIfcAxis2Placement3D(doc);
            var localPlacement = new IfcLocalPlacement(new IfcAxis2Placement(placement));
            doc.AddEntity(placement);
            return localPlacement;
        }

        private static IfcAxis2Placement3D ToIfcAxis2Placement3D(this Transform transform, Document doc)
        {
            var origin = transform.Origin.ToIfcCartesianPoint();
            var z = transform.ZAxis.ToIfcDirection();
            var x = transform.XAxis.ToIfcDirection();
            var placement = new IfcAxis2Placement3D(origin, 
                z, x);
            doc.AddEntity(origin);
            doc.AddEntity(z);
            doc.AddEntity(x);
            return placement;
        }
        
        private static IfcDirection ToIfcDirection(this Vector3 direction)
        {
            return new IfcDirection(new List<double>{direction.X, direction.Y, direction.Z});
        }

        private static IfcCartesianPoint ToIfcCartesianPoint(this Vector3 point)
        {
            return new IfcCartesianPoint(new List<IfcLengthMeasure>{point.X, point.Y, point.Z});
        }

        private static IfcPlane ToIfcPlane(this Plane plane, Document doc)
        {
            var t = new Transform(plane.Origin, plane.Normal);
            var ifcPlane = new IfcPlane(t.ToIfcAxis2Placement3D(doc));
            return ifcPlane;
        }
    }
}