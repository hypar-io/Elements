using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements;
using IFC;

namespace Elements.Serialization.IFC.IFCToHypar
{
    /// <summary>
    /// Extension methods for converting IFC entities to elements.
    /// </summary>
    internal static class IFCExtensions
    {
        internal static DoorOpeningSide GetDoorOpeningSide(this IfcDoor ifcDoor)
        {
            switch (ifcDoor.OperationType)
            {
                case IfcDoorTypeOperationEnum.SINGLE_SWING_LEFT:
                case IfcDoorTypeOperationEnum.DOUBLE_SWING_LEFT:
                    return DoorOpeningSide.LeftHand;
                case IfcDoorTypeOperationEnum.SINGLE_SWING_RIGHT:
                case IfcDoorTypeOperationEnum.DOUBLE_SWING_RIGHT:
                    return DoorOpeningSide.RightHand;
                case IfcDoorTypeOperationEnum.DOUBLE_DOOR_SINGLE_SWING:
                case IfcDoorTypeOperationEnum.DOUBLE_DOOR_DOUBLE_SWING:
                    return DoorOpeningSide.DoubleDoor;
            }
            return DoorOpeningSide.Undefined;
        }

        internal static DoorOpeningType GetDoorOpeningType(this IfcDoor ifcDoor)
        {
            switch (ifcDoor.OperationType)
            {
                case IfcDoorTypeOperationEnum.SINGLE_SWING_LEFT:
                case IfcDoorTypeOperationEnum.SINGLE_SWING_RIGHT:
                case IfcDoorTypeOperationEnum.DOUBLE_DOOR_SINGLE_SWING:
                    return DoorOpeningType.SingleSwing;
                case IfcDoorTypeOperationEnum.DOUBLE_SWING_LEFT:
                case IfcDoorTypeOperationEnum.DOUBLE_SWING_RIGHT:
                case IfcDoorTypeOperationEnum.DOUBLE_DOOR_DOUBLE_SWING:
                    return DoorOpeningType.DoubleSwing;
            }
            return DoorOpeningType.Undefined;
        }

        // TODO: In IFC an IfcOpeningElement may have several extrudes.
        // Now they are extracted as separate Openings. As the result
        // initial Guid of IfcOpeningElement is not saved.
        internal static List<Opening> ToOpenings(this IfcOpeningElement opening)
        {
            var relativePlacement = ((IfcLocalPlacement)opening.ObjectPlacement).RelativePlacement.ToTransform();
            var resultOpenings = new List<Opening>();

            var extrudes = opening.RepresentationsOfType<IfcExtrudedAreaSolid>();

            if (extrudes is null)
            {
                return resultOpenings;
            }

            foreach (var extrude in extrudes)
            {
                var extrudePosition = extrude.Position.ToTransform();
                var openingTransform = extrudePosition.Concatenated(relativePlacement);

                var profile = (Polygon)extrude.SweptArea.ToCurve();
                var extrudeDir = extrude.ExtrudedDirection.ToVector3();

                var profileTransformed = profile.TransformedPolygon(openingTransform);
                var extrudeDirTransformed = openingTransform.OfVector(extrudeDir);

                var extrudeDepth = (IfcLengthMeasure)extrude.Depth;
                var newOpening = new Opening(profileTransformed, extrudeDirTransformed, extrudeDepth, 0.0);
                resultOpenings.Add(newOpening);
            }

            return resultOpenings;
        }

        internal static IEnumerable<T> RepresentationsOfType<T>(this IfcProduct product) where T : IfcGeometricRepresentationItem
        {
            var reps = product.Representation.Representations.SelectMany(r => r.Items);
            if (reps.Any())
            {
                return reps.OfType<T>();
            }
            return null;
        }

        internal static ICurve ToCurve(this IfcProfileDef profile)
        {
            if (profile is IfcCircleProfileDef cpd)
            {
                // TODO: Remove this conversion to a polygon when downstream
                // functions support arcs and circles.
                return new Circle((IfcLengthMeasure)cpd.Radius).ToPolygon(10);
            }
            else if (profile is IfcParameterizedProfileDef ipd)
            {
                return ipd.ToCurve();
            }
            else if (profile is IfcArbitraryOpenProfileDef aopd)
            {
                return aopd.ToCurve();
            }
            else if (profile is IfcArbitraryClosedProfileDef acpd)
            {
                return acpd.ToCurve();
            }
            else if (profile is IfcCompositeProfileDef)
            {
                throw new Exception("IfcCompositeProfileDef is not supported yet.");
            }
            else if (profile is IfcDerivedProfileDef)
            {
                throw new Exception("IfcDerivedProfileDef is not supported yet.");
            }
            return null;
        }

        internal static ICurve ToCurve(this IfcParameterizedProfileDef profile)
        {
            var transform = new Transform(profile.Position.Location.ToVector3());
            if (profile is IfcRectangleProfileDef ifcRectangle)
            {
                var rectangle = Polygon.Rectangle((IfcLengthMeasure)ifcRectangle.XDim, (IfcLengthMeasure)ifcRectangle.YDim);
                return rectangle.Transformed(transform);
            }
            else if (profile is IfcCircleProfileDef ifcCircle)
            {
                var circle = new Circle((IfcLengthMeasure)ifcCircle.Radius);
                return circle.Transformed(transform);
            }
            else
            {
                throw new Exception($"The IfcParameterizedProfileDef type, {profile.GetType().Name}, is not supported.");
            }
        }

        internal static ICurve ToCurve(this IfcArbitraryOpenProfileDef profile)
        {
            return profile.Curve.ToCurve(false);
        }

        internal static ICurve ToCurve(this IfcArbitraryClosedProfileDef profile)
        {
            return profile.OuterCurve.ToCurve(true);
        }

        internal static ICurve ToCurve(this IfcCurve curve, bool closed)
        {
            if (curve is IfcBoundedCurve)
            {
                if (curve is IfcCompositeCurve)
                {
                    throw new Exception("IfcCompositeCurve is not supported yet.");
                }
                else if (curve is IfcPolyline pl)
                {
                    if (closed)
                    {
                        return pl.ToPolygon(true);
                    }
                    else
                    {
                        return pl.ToPolyline();
                    }
                }
                else if (curve is IfcTrimmedCurve)
                {
                    throw new Exception("IfcTrimmedCurve is not supported yet.");
                }
                else if (curve is IfcBSplineCurve)
                {
                    throw new Exception("IfcBSplineCurve is not supported yet.");
                }
                else if (curve is IfcIndexedPolyCurve ipc)
                {
                    return ipc.ToIndexedPolycurve();
                }
            }
            else if (curve is IfcConic)
            {
                throw new Exception("IfcConic is not supported yet.");
            }
            else if (curve is IfcOffsetCurve2D)
            {
                throw new Exception("IfcOffsetCurve2D is not supported yet.");
            }
            else if (curve is IfcOffsetCurve3D)
            {
                throw new Exception("IfcOffsetCurve3D is not supported yet.");
            }
            return null;
        }

        internal static Profile ToProfile(this IfcProfileDef ifcProfile)
        {
            Polygon outer = null;
            List<Polygon> inner = new List<Polygon>();

            if (ifcProfile is IfcRectangleProfileDef ifcRectangle)
            {
                var rectangle = Polygon.Rectangle((IfcLengthMeasure)ifcRectangle.XDim, (IfcLengthMeasure)ifcRectangle.YDim);
                var transform = new Transform(ifcRectangle.Position.Location.ToVector3());
                outer = (Polygon)rectangle.Transformed(transform);
            }
            else if (ifcProfile is IfcCircleProfileDef ifcCircle)
            {
                var circle = new Circle((IfcLengthMeasure)ifcCircle.Radius).ToPolygon();
                var transform = new Transform(ifcCircle.Position.Location.ToVector3());
                outer = (Polygon)circle.Transformed(transform);
            }
            else if (ifcProfile is IfcArbitraryClosedProfileDef closedProfile)
            {
                var outerCurve = closedProfile.OuterCurve.ToCurve(true);
                if (outerCurve is Polygon pc)
                {
                    outer = pc;
                }
                else if (outerCurve is IndexedPolycurve ipc)
                {
                    outer = ipc.ToPolygon();
                }

                if (ifcProfile is IfcArbitraryProfileDefWithVoids)
                {
                    var voidProfile = (IfcArbitraryProfileDefWithVoids)ifcProfile;
                    inner.AddRange(voidProfile.InnerCurves.Select(c =>
                    {
                        var elCurve = c.ToCurve(true);
                        if (elCurve is Polygon voidP)
                        {
                            return voidP;
                        }
                        else if (elCurve is IndexedPolycurve voidPc)
                        {
                            return voidPc.ToPolygon();
                        }
                        return null;
                    }));
                }
            }
            else
            {
                throw new Exception($"The profile type, {ifcProfile.GetType().Name}, is not supported.");
            }

            // var name = profile.ProfileName == null ? null : profile.ProfileName;
            var newProfile = new Profile(outer, inner, ifcProfile.Id, string.Empty);
            return newProfile;
        }

        internal static IndexedPolycurve ToIndexedPolycurve(this IfcIndexedPolyCurve polycurve)
        {
            var vertices = new List<Vector3>();
            foreach (var point in ((IfcCartesianPointList2D)polycurve.Points).CoordList)
            {
                vertices.Add(point.ToVector3());
            }

            IndexedPolycurve pc;
            var curveIndices = new List<IList<int>>();
            if (polycurve.Segments != null)
            {
                foreach (var select in polycurve.Segments)
                {
                    var segmentIndices = new List<int>();
                    if (select.Choice is IfcLineIndex li)
                    {
                        foreach (IfcInteger segmentIndex in (List<IfcPositiveInteger>)li)
                        {
                            segmentIndices.Add(segmentIndex - 1);
                        }
                    }
                    else if (select.Choice is IfcArcIndex ai)
                    {
                        foreach (IfcInteger segmentIndex in (List<IfcPositiveInteger>)ai)
                        {
                            segmentIndices.Add(segmentIndex - 1);
                        }
                    }
                    curveIndices.Add(segmentIndices);
                }
                pc = new IndexedPolycurve(vertices, curveIndices);
            }
            else
            {
                pc = new IndexedPolycurve(vertices);
            }
            return pc;
        }

        internal static Vector3 ToVector3(this IfcCartesianPoint cartesianPoint)
        {
            return cartesianPoint.Coordinates.ToVector3();
        }

        internal static Vector3 ToVector3(this List<IfcLengthMeasure> measures)
        {
            if (measures.Count == 2)
            {
                return new Vector3(measures[0], measures[1]);
            }
            else if (measures.Count == 3)
            {
                return new Vector3(measures[0], measures[1], measures[2]);
            }
            else
            {
                throw new Exception($"{measures.Count} measures could not be converted to a Vector3.");
            }
        }

        internal static Polygon ToPolygon(this IfcPolyline polyline, bool dropLastPoint = false)
        {
            var count = dropLastPoint ? polyline.Points.Count - 1 : polyline.Points.Count;
            var verts = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                var v = polyline.Points[i].ToVector3();
                verts[i] = v;
            }
            return new Polygon(verts);
        }

        internal static Polyline ToPolyline(this IfcPolyline polyline)
        {
            var verts = polyline.Points.Select(p => p.ToVector3()).ToArray();
            return new Polyline(verts);
        }

        internal static bool IsClosed(this IfcPolyline pline)
        {
            var start = pline.Points[0];
            var end = pline.Points[pline.Points.Count - 1];
            return start.Equals(end);
        }

        internal static bool Equals(this IfcCartesianPoint point, IfcCartesianPoint other, double tolerance = Vector3.EPSILON)
        {
            if (point.Coordinates != other.Coordinates)
            {
                return false;
            }

            double distanceSquared = 0.0;

            for (int i = 0; i < point.Coordinates.Count; i++)
            {
                var dif = point.Coordinates[i] - other.Coordinates[i];
                distanceSquared += dif * dif;
            }

            return distanceSquared < tolerance * tolerance;
        }
        internal static Transform ToTransform(this IfcAxis2Placement3D cs)
        {
            var x = cs.RefDirection != null ? cs.RefDirection.ToVector3() : Vector3.XAxis;
            var z = cs.Axis != null ? cs.Axis.ToVector3() : Vector3.ZAxis;
            var y = z.Cross(x);
            var o = cs.Location.ToVector3();
            var t = new Transform(o, x, y, z);
            return t;
        }

        internal static Transform ToTransform(this IfcAxis2Placement2D cs)
        {
            var d = cs.RefDirection.ToVector3();
            var z = Vector3.ZAxis;
            var o = cs.Location.ToVector3();
            return new Transform(o, d, z);
        }

        internal static Vector3 ToVector3(this IfcDirection direction)
        {
            var ratios = direction.DirectionRatios;
            return new Vector3(ratios[0], ratios[1], ratios[2]);
        }

        internal static Transform ToTransform(this IfcAxis2Placement placement)
        {
            // SELECT IfcAxis2Placement3d, IfcAxis2Placement2d
            if (placement.Choice.GetType() == typeof(IfcAxis2Placement2D))
            {
                var cs = (IfcAxis2Placement2D)placement.Choice;
                return cs.ToTransform();
            }
            else if (placement.Choice.GetType() == typeof(IfcAxis2Placement3D))
            {
                var cs = (IfcAxis2Placement3D)placement.Choice;
                var t = cs.ToTransform();
                return t;
            }
            else
            {
                throw new Exception($"The specified placement of type, {placement.GetType().ToString()}, cannot be converted to a Transform.");
            }
        }

        internal static Transform ToTransform(this IfcLocalPlacement placement)
        {
            var t = placement.RelativePlacement.ToTransform();
            if (placement.PlacementRelTo != null)
            {
                var tr = placement.PlacementRelTo.ToTransform();
                t.Concatenate(tr);
            }
            return t;
        }

        internal static Transform ToTransform(this IfcObjectPlacement placement)
        {
            if (placement.GetType() == typeof(IfcLocalPlacement))
            {
                var lp = (IfcLocalPlacement)placement;
                var t = lp.ToTransform();
                return t;
            }
            else if (placement.GetType() == typeof(IfcGridPlacement))
            {
                throw new Exception("IfcGridPlacement conversion to Transform not supported.");
            }
            return null;
        }

        internal static Transform ToTransform(this IfcCartesianTransformationOperator op)
        {
            if (op is IfcCartesianTransformationOperator2D)
            {
                var op2D = (IfcCartesianTransformationOperator2D)op;
                return op2D.ToTransform();
            }
            else if (op is IfcCartesianTransformationOperator3D)
            {
                var op3D = (IfcCartesianTransformationOperator3D)op;
                return op3D.ToTransform();
            }
            return null;
        }

        internal static Transform ToTransform(this IfcCartesianTransformationOperator2D op)
        {
            var o = op.LocalOrigin.ToVector3();
            var x = op.Axis1 == null ? Vector3.XAxis : op.Axis1.ToVector3().Unitized();
            var y = op.Axis2 == null ? Vector3.YAxis : op.Axis2.ToVector3().Unitized();
            var z = x.Cross(y);
            return new Transform(o, x, y, z);
        }

        internal static Transform ToTransform(this IfcCartesianTransformationOperator3D op)
        {
            var o = op.LocalOrigin.ToVector3();
            var x = op.Axis1 == null ? Vector3.XAxis : op.Axis1.ToVector3().Unitized();
            var y = op.Axis2 == null ? Vector3.YAxis : op.Axis2.ToVector3().Unitized();
            var z = op.Axis3 == null ? Vector3.ZAxis : op.Axis3.ToVector3().Unitized();
            return new Transform(o, x, y, z);
        }

        internal static Polygon ToPolygon(this List<IfcCartesianPoint> loop)
        {
            var verts = new Vector3[loop.Count];
            for (var i = 0; i < loop.Count; i++)
            {
                verts[i] = loop[i].ToVector3();
            }
            return new Polygon(verts);
        }

        internal static Loop ToLoop(this List<IfcCartesianPoint> loop, Solid solid)
        {
            var hes = new HalfEdge[loop.Count];
            for (var i = 0; i < loop.Count; i++)
            {
                var v = solid.AddVertex(loop[i].ToVector3());
                hes[i] = new HalfEdge(v);
            }
            var newLoop = new Loop(hes);
            return newLoop;
        }

        internal static Polygon ToPolygon(this IfcPolyLoop loop)
        {
            return loop.Polygon.ToPolygon();
        }
        internal static Color ToColor(this IfcColourRgb rgb, double transparency)
        {
            return new Color((IfcRatioMeasure)rgb.Red, (IfcRatioMeasure)rgb.Green, (IfcRatioMeasure)rgb.Blue, transparency);
        }
    }
}