using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using IFC;

namespace Elements.Serialization.IFC
{
    /// <summary>
    /// Extension methods for converting elements to IFC entities.
    /// </summary>
    public static class ElementsExtensions
    {
        internal static List<IfcProduct> ToIfcProducts(this Element e, IfcRepresentationContext context, Document doc, Dictionary<string, IfcSurfaceStyle> styles)
        {
            var products = new List<IfcProduct>();

            IfcProductDefinitionShape shape = null;
            var localPlacement = e.Transform.ToIfcLocalPlacement(doc);
            IfcGeometricRepresentationItem geom = null;

            if(e is IGeometry)
            {
                var geo = (IGeometry)e;
                geo.UpdateSolidOperations();
                foreach(var op in geo.Geometry.SolidOperations)
                {
                    if(op is Sweep)
                    {
                        var sweep = (Sweep)op;
                        geom = sweep.ToIfcSurfaceCurveSweptAreaSolid(e.Transform, doc);
                    }
                    else if(op is Extrude)
                    {
                        var extrude = (Extrude)op;
                        geom = extrude.ToIfcExtrudedAreaSolid(e.Transform, doc);
                    }
                    else if(op is Lamina)
                    {
                        var lamina = (Lamina)op;
                        geom = lamina.ToIfcShellBasedSurfaceModel(e.Transform, doc);
                    }
                    else
                    {
                        throw new Exception("Only IExtrude, ISweepAlongCurve, and ILamina representations are currently supported.");
                    }
                }
            }
            
            shape = ToIfcProductDefinitionShape(geom, context, doc);

            doc.AddEntity(shape);
            doc.AddEntity(localPlacement);
            doc.AddEntity(geom);

            var product = ConvertElementToIfcProduct(e, localPlacement, shape);
            products.Add(product);
            doc.AddEntity(product);

            var ifcOpenings = doc.AllEntities.Where(ent=>ent.GetType() == typeof(IfcOpeningElement)).Cast<IfcOpeningElement>();

            // If the element has openings, make opening relationships in
            // the IfcElement.
            if(e is IHasOpenings)
            {
                var openings = (IHasOpenings)e;
                if(openings.Openings.Count > 0)
                {
                    foreach(var o in openings.Openings)
                    {
                        var element = (IfcElement)product;
                        // TODO: Find the opening that we've already created that relates here
                        var opening = ifcOpenings.First(ifcO=>ifcO.GlobalId == IfcGuid.ToIfcGuid(o.Id));
                        var voidRel = new IfcRelVoidsElement(IfcGuid.ToIfcGuid(Guid.NewGuid()), element, opening);
                        element.HasOpenings.Add(voidRel);
                        doc.AddEntity(voidRel);
                    }
                }
            }

            if(e is IMaterial)
            {
                var m = (IMaterial)e;
                IfcSurfaceStyle style = null;
                if(styles.ContainsKey(m.Material.Name))
                {
                    style = styles[m.Material.Name];
                }
                else
                {
                    style = m.Material.ToIfcSurfaceStyle(doc);
                    styles.Add(m.Material.Name, style);
                }
                var styledItem = CreateIfcStyledItem(geom, style, doc);
                geom.StyledByItem = new List<IfcStyledItem>{styledItem};
                doc.AddEntity(styledItem);
            }

            return products;
        }

        private static IfcOpeningElement ToIfc(this Opening opening, IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcOpening = new IfcOpeningElement(IfcGuid.ToIfcGuid(opening.Id),
                                                   null,
                                                   null,
                                                   null,
                                                   null,
                                                   localPlacement,
                                                   shape,
                                                   null,
                                                   IfcOpeningElementTypeEnum.OPENING);
            return ifcOpening;
        }

        internal static IfcLocalPlacement ToIfcLocalPlacement(this Transform transform, Document doc, IfcObjectPlacement parent = null)
        {
            var placement = transform.ToIfcAxis2Placement3D(doc);
            var localPlacement = new IfcLocalPlacement(new IfcAxis2Placement(placement));
            if(parent != null)
            {
                localPlacement.PlacementRelTo = parent;
            }
            
            doc.AddEntity(placement);
            return localPlacement;
        }

        internal static IfcExtrudedAreaSolid ToIfcExtrudedAreaSolid(this Extrude extrude, Transform transform, Document doc)
        {
            var position = transform.ToIfcAxis2Placement3D(doc);

            var extrudeDepth = extrude.Height;
            var extrudeProfile = extrude.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var extrudeDirection = Vector3.ZAxis.ToIfcDirection();;

            var solid = new IfcExtrudedAreaSolid(extrudeProfile, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(extrude.Height));

            doc.AddEntity(extrudeProfile);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(solid);

            return solid;
        }

        private static IfcCurve ToIfcCurve(this ICurve curve, Document doc)
        {
            if (curve is Line)
            {
                return ((Line)curve).ToIfcTrimmedCurve(doc);
            }
            else if(curve is Arc)
            {
                return ((Arc)curve).ToIfcTrimmedCurve(doc);
            }
            // Test Polygon before Polyline to avoid 
            // Polygons being treated as Polylines.
            else if(curve is Polygon)
            {
                return ((Polygon)curve).ToIfcPolyline(doc);
            }
            else if(curve is Polyline)
            {
                return ((Polyline)curve).ToIfcPolyline(doc);
            }
            else
            {
                throw new Exception($"The curve type, {curve.GetType()}, is not yet supported.");
            }
        }

        private static IfcProduct ConvertElementToIfcProduct(Element element, IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            try
            {
                IfcProduct e = null;
                if(element is Beam)
                {
                    e = ((Beam)element).ToIfc(localPlacement, shape);
                }
                if(element is Brace)
                {
                    e = ((Brace)element).ToIfc(localPlacement, shape);
                }
                else if(element is Column)
                {
                    e = ((Column)element).ToIfc(localPlacement, shape);
                }
                else if (element is StandardWall)
                {
                    e = ((StandardWall)element).ToIfc(localPlacement, shape);
                }
                else if (element is Wall)
                {
                    e = ((Wall)element).ToIfc(localPlacement, shape);
                }
                else if (element is Floor)
                {
                    e = ((Floor)element).ToIfc(localPlacement, shape);
                }
                else if (element is Space)
                {
                    e = ((Space)element).ToIfc(localPlacement, shape);
                }
                else if (element is Panel)
                {
                    e = ((Panel)element).ToIfc(localPlacement, shape);
                }
                else if (element is Mass)
                {
                    e = ((Mass)element).ToIfc(localPlacement, shape);
                }
                else if (element is Opening)
                {
                    e = ((Opening)element).ToIfc(localPlacement, shape);
                }
                return e;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"{element.GetType()} cannot be serialized to IFC.");
            }
            return null;
        }

        private static IfcSurfaceCurveSweptAreaSolid ToIfcSurfaceCurveSweptAreaSolid(this Sweep sweep, Transform transform, Document doc)
        {
            var position = transform.ToIfcAxis2Placement3D(doc);
            var sweptArea = sweep.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var directrix = sweep.Curve.ToIfcCurve(doc);
            
            var extrudeDir = Vector3.ZAxis.ToIfcDirection();
            var profile = new IfcArbitraryClosedProfileDef(IfcProfileTypeEnum.CURVE, directrix);
            var surface = new IfcSurfaceOfLinearExtrusion(profile, position, extrudeDir, new IfcPositiveLengthMeasure(1.0));

            var solid = new IfcSurfaceCurveSweptAreaSolid(sweptArea, position, directrix, sweep.StartSetback, sweep.EndSetback, surface);
            
            doc.AddEntity(position);
            doc.AddEntity(sweptArea);
            doc.AddEntity(directrix);

            doc.AddEntity(extrudeDir);
            doc.AddEntity(profile);

            doc.AddEntity(surface);
            doc.AddEntity(solid);

            return solid;
        }

        private static IfcShellBasedSurfaceModel ToIfcShellBasedSurfaceModel(this Lamina lamina, Transform transform, Document doc)
        {
            var position = transform.ToIfcAxis2Placement3D(doc);

            var plane = lamina.Perimeter.Plane().ToIfcPlane(doc);
            var outer = lamina.Perimeter.ToIfcCurve(doc);
            var bplane = new IfcCurveBoundedPlane(plane, outer, new List<IfcCurve>{});

            var bounds = new List<IfcFaceBound>{};
            var loop = lamina.Perimeter.ToIfcPolyLoop(doc);
            var faceBounds = new IfcFaceBound(loop, true);
            bounds.Add(faceBounds);

            var face = new IfcFaceSurface(bounds, bplane, true);
            var openShell = new IfcOpenShell(new List<IfcFace>{face});

            var shell = new IfcShell(openShell);
            var ssm = new IfcShellBasedSurfaceModel(new List<IfcShell>{shell});

            doc.AddEntity(plane);
            doc.AddEntity(outer);
            doc.AddEntity(bplane);
            doc.AddEntity(loop);
            doc.AddEntity(faceBounds);
            doc.AddEntity(face);
            doc.AddEntity(openShell);
            
            return ssm;
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
                return ((Arc)curve).Plane();
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

        private static IfcProductDefinitionShape ToIfcProductDefinitionShape(this IfcGeometricRepresentationItem geom, IfcRepresentationContext context, Document doc)
        {
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", 
                new List<IfcRepresentationItem>{geom});
            var shape = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});

            doc.AddEntity(rep);

            return shape;
        }

        private static IfcSlab ToIfc(this Floor floor, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var slab = new IfcSlab(IfcGuid.ToIfcGuid(floor.Id), null, null, null, 
                null, localPlacement, shape, null, IfcSlabTypeEnum.FLOOR);
            return slab;
        }

        // TODO: There is a lot of duplicate code used to create products.
        // Can we make a generic method like ToIfc<TProduct>()? There are 
        // exceptions for which this won't work like IfcSpace.

        private static IfcSpace ToIfc(this Space space, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcSpace = new IfcSpace(IfcGuid.ToIfcGuid(space.Id),
                                        null,
                                        null,
                                        null,
                                        null,
                                        localPlacement,
                                        shape,
                                        null,
                                        IfcElementCompositionEnum.ELEMENT,
                                        IfcSpaceTypeEnum.NOTDEFINED,
                                        new IfcLengthMeasure(space.Transform.Origin.Z));
            return ifcSpace;
        }

        private static IfcProduct ToIfc(this StandardWall wall, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcWall = new IfcWallStandardCase(IfcGuid.ToIfcGuid(wall.Id),
                                                  null,
                                                  wall.Name,
                                                  null,
                                                  null,
                                                  localPlacement,
                                                  shape,
                                                  null,
                                                  IfcWallTypeEnum.NOTDEFINED);
            return ifcWall;
        }

        private static IfcWall ToIfc(this Wall wall, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcWall = new IfcWall(IfcGuid.ToIfcGuid(wall.Id),
                                      null,
                                      wall.Name,
                                      null,
                                      null,
                                      localPlacement,
                                      shape,
                                      null,
                                      IfcWallTypeEnum.NOTDEFINED);
            return ifcWall;
        }

        private static IfcBeam ToIfc(this Beam beam, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcBeam = new IfcBeam(IfcGuid.ToIfcGuid(beam.Id), null, 
                null, null, null, localPlacement, shape, null, IfcBeamTypeEnum.BEAM);
            return ifcBeam;
        }

        private static IfcColumn ToIfc(this Column column,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcColumn = new IfcColumn(IfcGuid.ToIfcGuid(column.Id), null,
                null, null, null, localPlacement, shape, null, IfcColumnTypeEnum.COLUMN);
            return ifcColumn;
        }

        private static IfcMember ToIfc(this Brace column,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var member = new IfcMember(IfcGuid.ToIfcGuid(column.Id), null,
                null, null, null, localPlacement, shape, null, IfcMemberTypeEnum.NOTDEFINED);
            return member;
        }

        private static IfcPlate ToIfc(this Panel panel,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var plate = new IfcPlate(IfcGuid.ToIfcGuid(panel.Id), null,
                null, null, null, localPlacement, shape, null, IfcPlateTypeEnum.NOTDEFINED);
            return plate;
        }

        private static IfcBuildingElementProxy ToIfc(this Mass mass, 
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var proxy = new IfcBuildingElementProxy(IfcGuid.ToIfcGuid(mass.Id), null,
                null, null, null, localPlacement, shape, null,IfcBuildingElementProxyTypeEnum.ELEMENT);
            return proxy;
        }

        private static IfcLoop ToIfcPolyLoop(this Polygon polygon, Document doc)
        {
            var loop = new IfcPolyLoop(polygon.Vertices.ToIfcCartesianPointList(doc));
            return loop;
        }

        private static List<IfcCartesianPoint> ToIfcCartesianPointList(this IList<Vector3> pts, Document doc)
        {
            var icps = new List<IfcCartesianPoint>();
            foreach(var pt in pts)
            {
                var icp = pt.ToIfcCartesianPoint();
                doc.AddEntity(icp);
                icps.Add(icp);
            }
            return icps;
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
            // Add the first point to close the curve.
            points.Add(points.First());
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

        private static IfcTrimmedCurve ToIfcTrimmedCurve(this Line line, Document doc)
        {
            var start = line.Start.ToIfcCartesianPoint();
            var end = line.End.ToIfcCartesianPoint();

            var dir = line.Direction().ToIfcVector(doc);

            var ifcLine = new IfcLine(start, dir);
            var trim1 = new IfcTrimmingSelect(start);
            var trim2 = new IfcTrimmingSelect(end);
            var tc = new IfcTrimmedCurve(ifcLine, new List<IfcTrimmingSelect>{trim1}, new List<IfcTrimmingSelect>{trim2}, 
                true, IfcTrimmingPreference.CARTESIAN);
            
            doc.AddEntity(start);
            doc.AddEntity(end);
            doc.AddEntity(dir);
            doc.AddEntity(ifcLine);

            return tc;
        }

        private static IfcTrimmedCurve ToIfcTrimmedCurve(this Arc arc, Document doc)
        {
            var p = arc.Plane();
            var t = new Transform(p.Origin, p.Normal);
            var placement = t.ToIfcAxis2Placement3D(doc);
            var ifcCircle = new IfcCircle(new IfcAxis2Placement(placement), new IfcPositiveLengthMeasure(arc.Radius));
            var start = arc.PointAt(0.0).ToIfcCartesianPoint();
            var end = arc.PointAt(1.0).ToIfcCartesianPoint();
            var trim1 = new IfcTrimmingSelect(start);
            var trim2 = new IfcTrimmingSelect(end);
            var tc = new IfcTrimmedCurve(ifcCircle, new List<IfcTrimmingSelect>{trim1}, new List<IfcTrimmingSelect>{trim2}, 
                true, IfcTrimmingPreference.CARTESIAN);
            
            doc.AddEntity(start);
            doc.AddEntity(end);
            doc.AddEntity(placement);
            doc.AddEntity(ifcCircle);

            return tc;
        }

        private static IfcVector ToIfcVector(this Vector3 v, Document doc)
        {
            var dir = v.ToIfcDirection();
            var vector = new IfcVector(dir, new IfcLengthMeasure(0));
            doc.AddEntity(dir);
            return vector;
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
            return new IfcDirection(new List<IfcReal>{new IfcReal(direction.X), new IfcReal(direction.Y), new IfcReal(direction.Z)});
        }

        private static IfcCartesianPoint ToIfcCartesianPoint(this Vector3 point)
        {
            return new IfcCartesianPoint(new List<IfcLengthMeasure>{point.X, point.Y, point.Z});
        }

        private static IfcPlane ToIfcPlane(this Plane plane, Document doc)
        {
            var t = new Transform(plane.Origin, plane.Normal);
            var position = t.ToIfcAxis2Placement3D(doc);
            var ifcPlane = new IfcPlane(position);
            
            doc.AddEntity(position);

            return ifcPlane;
        }
    
        private static IfcColourRgb ToIfcColourRgb(this Color color)
        {
            var red = new IfcNormalisedRatioMeasure(new IfcRatioMeasure(color.Red));
            var green = new IfcNormalisedRatioMeasure(new IfcRatioMeasure(color.Green));
            var blue = new IfcNormalisedRatioMeasure(new IfcRatioMeasure(color.Blue));
            
            var ifcColor = new IfcColourRgb(red, green, blue);

            return ifcColor;
        }

        private static IfcSurfaceStyle ToIfcSurfaceStyle(this Material material, Document doc)
        {
            var color = material.Color.ToIfcColourRgb();
            var shading = new IfcSurfaceStyleShading(color);

            var styles = new List<IfcSurfaceStyleElementSelect>{};
            styles.Add(new IfcSurfaceStyleElementSelect(shading));
            var surfaceStyle = new IfcSurfaceStyle(material.Name, IfcSurfaceSide.POSITIVE, styles);
            doc.AddEntity(color);
            doc.AddEntity(shading);
            doc.AddEntity(surfaceStyle);
            return surfaceStyle;
        }

        private static IfcStyledItem CreateIfcStyledItem(IfcRepresentationItem shape, IfcSurfaceStyle style, Document doc)
        {
            var styleSelect = new IfcPresentationStyleSelect(style);
            var assign = new IfcStyleAssignmentSelect(style);
            var assignments = new List<IfcStyleAssignmentSelect>();
            assignments.Add(assign);
            var styledItem = new IfcStyledItem(shape, assignments, null);
            // doc.AddEntity(assign);
            return styledItem;
        }
    }
}