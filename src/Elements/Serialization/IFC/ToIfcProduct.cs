using System;
using System.Collections.Generic;
using Elements.Geometry;
using IFC;

namespace Elements.Serialization.IFC
{
    public static partial class IFCExtensions
    {
        /// <summary>
        /// Convert a Floor to an IfcSlab.
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="context"></param>
        /// /// <param name="doc"></param>
        /// <param name="products"></param>
        private static void ToIfcSlab(this Floor floor, 
            IfcRepresentationContext context, Document doc, List<IfcProduct> products)
        {
            var position = floor.Transform.ToIfcAxis2Placement3D(doc);
            var sweptArea = floor.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var extrudeDirection = Vector3.ZAxis.ToIfcDirection();
            var repItem = new IfcExtrudedAreaSolid(sweptArea, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(floor.Thickness()));
            var localPlacement = new Transform().ToIfcLocalPlacement(doc);

            var placement = floor.Transform.ToIfcAxis2Placement3D(doc);
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", new List<IfcRepresentationItem>{repItem});
            var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});

            var slab = new IfcSlab(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, null, null, 
                null, localPlacement, productRep, null, IfcSlabTypeEnum.FLOOR);

            doc.AddEntity(sweptArea);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(repItem);
            doc.AddEntity(rep);
            doc.AddEntity(localPlacement);
            doc.AddEntity(productRep);
            doc.AddEntity(slab);

            foreach(var o in floor.Openings)
            {
                var opening = o.ToIfcOpeningElement(context, doc);
                var voidRel = new IfcRelVoidsElement(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, slab, opening);
                slab.HasOpenings.Add(voidRel);
                doc.AddEntity(voidRel);
            }

            products.Add(slab);
        }

        /// <summary>
        /// Convert a space to an IfcSpace.
        /// </summary>
        /// <param name="space"></param>
        /// <param name="context"></param>
        /// <param name="doc"></param>
        /// <param name="products"></param>
        private static void ToIfcSpace(this Space space, 
            IfcRepresentationContext context, Document doc, List<IfcProduct> products)
        {
            var position = space.Transform.ToIfcAxis2Placement3D(doc);
            var sweptArea = space.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var extrudeDirection = Vector3.ZAxis.ToIfcDirection();
            var repItem = new IfcExtrudedAreaSolid(sweptArea, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(space.ExtrudeDepth));
            var localPlacement = new Transform().ToIfcLocalPlacement(doc);
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", new List<IfcRepresentationItem>{repItem});
            var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});

            var ifcSpace = new IfcSpace(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, null, null, 
                null, localPlacement, productRep, null, IfcElementCompositionEnum.ELEMENT, IfcInternalOrExternalEnum.NOTDEFINED, new IfcLengthMeasure(space.Transform.Origin.Z));
            
            doc.AddEntity(sweptArea);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(repItem);
            doc.AddEntity(rep);
            doc.AddEntity(localPlacement);
            doc.AddEntity(productRep);
            doc.AddEntity(ifcSpace);

            products.Add(ifcSpace);
        }
        
        /// <summary>
        /// Convert a wall to an IfcWallStandardCase
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="context"></param>
        /// <param name="doc"></param>
        /// <param name="products"></param>
        /// <returns></returns>
        private static void ToIfcWallStandardCase(this StandardWall wall, 
            IfcRepresentationContext context, Document doc, List<IfcProduct> products)
        {
            var sweptArea = wall.CenterLine.Thicken(wall.Thickness()).ToIfcArbitraryClosedProfileDef(doc);
            var extrudeDirection = Vector3.ZAxis.ToIfcDirection();

            // We don't use the Wall's transform for positioning, because
            // our walls have a transform that lays the wall "flat". Just
            // use a identity transform or a transform that includes
            // the elevation.
            var position = new Transform().ToIfcAxis2Placement3D(doc);
            var repItem = new IfcExtrudedAreaSolid(sweptArea, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(wall.Height));
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", new List<IfcRepresentationItem>{repItem});
            var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});
            var id = IfcGuid.ToIfcGuid(Guid.NewGuid());
            var localPlacement = new Transform(0,0,wall.CenterLine.Start.Z).ToIfcLocalPlacement(doc);
            var ifcWall = new IfcWallStandardCase(new IfcGloballyUniqueId(id), 
                null, wall.Name, null, null, localPlacement, productRep, null);

            doc.AddEntity(sweptArea);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(repItem);
            doc.AddEntity(rep);
            doc.AddEntity(localPlacement);
            doc.AddEntity(productRep);
            doc.AddEntity(ifcWall);

            products.Add(ifcWall);
        }

        /// <summary>
        /// Convert a wall to an IfcWall.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="context"></param>
        /// <param name="doc"></param>
        /// <param name="products"></param>
        /// <returns></returns>
        private static void ToIfcWall(this Wall wall, 
            IfcRepresentationContext context, Document doc, List<IfcProduct> products)
        {
            var sweptArea = wall.Transform.OfProfile(wall.Profile).Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var extrudeDirection = Vector3.ZAxis.ToIfcDirection();

            // We don't use the Wall's transform for positioning, because
            // our walls have a transform that lays the wall "flat". Just
            // use a identity transform or a transform that includes
            // the elevation.
            var position = new Transform().ToIfcAxis2Placement3D(doc);
            var repItem = new IfcExtrudedAreaSolid(sweptArea, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(wall.Height));
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", new List<IfcRepresentationItem>{repItem});
            var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});
            var id = IfcGuid.ToIfcGuid(Guid.NewGuid());
            var localPlacement = new Transform().ToIfcLocalPlacement(doc);
            var ifcWall = new IfcWall(new IfcGloballyUniqueId(id), 
                null, wall.Name, null, null, localPlacement, productRep, null);

            doc.AddEntity(sweptArea);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(repItem);
            doc.AddEntity(rep);
            doc.AddEntity(localPlacement);
            doc.AddEntity(productRep);
            doc.AddEntity(ifcWall);

            products.Add(ifcWall);
        }

        /// <summary>
        /// Convert a beam to an IfcBeam
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="context"></param>
        /// <param name="doc"></param>
        /// <param name="products"></param>
        /// <returns></returns>
        private static void ToIfcBeam(this Beam beam, 
            IfcRepresentationContext context, Document doc, List<IfcProduct> products)
        {
            var sweptArea = beam.ElementType.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var line = beam.Curve as Line;
            if(line == null) {
                throw new Exception("The beam could not be exported to IFC. Only linear beams are currently supported.");
            }
            
            // We use the Z extrude direction because the direction is 
            // relative to the local placement, which is a transform at the
            // beam's end with the Z axis pointing along the direction.
            
            var extrudeDirection = Vector3.ZAxis.ToIfcDirection();

            var position = new Transform().ToIfcAxis2Placement3D(doc);
            var repItem = new IfcExtrudedAreaSolid(sweptArea, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(beam.Curve.Length()));
            var localPlacement = beam.Curve.TransformAt(0.0).ToIfcLocalPlacement(doc);
            // var placement = beam.Transform.ToIfcAxis2Placement3D(doc);
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", new List<IfcRepresentationItem>{repItem});
            var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});
            var ifcBeam = new IfcBeam(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, null, null, null, localPlacement, productRep, null);
            
            doc.AddEntity(sweptArea);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(repItem);
            doc.AddEntity(rep);
            doc.AddEntity(localPlacement);
            doc.AddEntity(productRep);
            doc.AddEntity(ifcBeam);

            products.Add(ifcBeam);
        }
    }
}