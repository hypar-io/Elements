using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Serialization.glTF;
using Hypar.Revit;
using RevitServices.Persistence;
using Elements;
using Elements.Generate;
using Elements.Geometry;

using ADSK = Autodesk.Revit.DB;
using Autodesk.Revit.DB;

namespace HyparDynamo.Hypar
{
    /// <summary>
    /// Convert a Revit Area into a SpaceBoundary element. This currently only
    /// stores the perimeter polygon, and no geometric representation, so you will 
    /// not see it in 3D views in Hypar.
    /// </summary>
    /// <param name="area">The area that is meant to be converted to a Hypar SpaceBoundary.</param>
    public static class SpaceBoundary
    {
        public static Elements.Element[] FromArea(Revit.Elements.Element area)
        {
            var areaElement = (Autodesk.Revit.DB.Area)area.InternalElement;
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return Create.SpaceBoundaryFromRevitArea(areaElement, doc);
        }
    }

    /// <summary>
    /// Convert a list of points from Dynamo into a ModelPoints element, with the option 
    /// to add a tag that will be stored in the elements Name field.
    /// </summary>
    /// <param name="Points">The points that will be stored.</param>
    /// <returns name="Tag">The tag to assign to the model points.</param>
    public static class ModelPoints
    {
        public static Elements.Element FromPoints(List<Autodesk.DesignScript.Geometry.Point> points, string tag = "")
        {
            return Create.ModelPointsFromPoints(points.Select(p => new XYZ(p.X, p.Y, p.Z)), tag);
        }
    }

    public static class Wall
    {
        /// <summary>
        /// Convert a Revit wall to Elements.WallByProfile(s) for use in Hypar models.
        /// Sometimes a single wall in Revit needs to be converted to multiple Hypar walls.
        /// </summary>
        /// <param name="RevitWall">The walls to be exported.</param>
        /// <returns name="WallByProfile">The Hypar walls.</param>
        public static Elements.WallByProfile[] FromRevitWall(this Revit.Elements.Wall revitWall)
        {
            var r_Wall = (Autodesk.Revit.DB.Wall)revitWall.InternalElement;

            // wrapped exception catching to deliver more meaningful message in Dynamo
            try
            {
                return Create.WallsFromRevitWall(r_Wall, DocumentManager.Instance.CurrentDBDocument);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class Floor
    {
        /// <summary>
        /// Convert a Revit floor to Elements.Floor(s) for use in Hypar models.
        /// Sometimes a single floor in Revit needs to be converted to multiple Hypar floors.
        /// </summary>
        /// <param name="revitFloor">The floor to be exported.</param>
        /// <returns name="floor">The Hypar floors.</param>
        public static Elements.Floor[] FromRevitFloor(this Revit.Elements.Floor revitFloor)
        {
            var r_Floor = (Autodesk.Revit.DB.Floor)revitFloor.InternalElement;

            // wrapped exception catching to deliver more meaningful message in Dynamo
            try
            {
                return Create.FloorsFromRevitFloor(DocumentManager.Instance.CurrentDBDocument, r_Floor);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class Column
    {
        public static Elements.Column FromRevitColumn(this Revit.Elements.Element column)
        {
            var r_col = (Autodesk.Revit.DB.FamilyInstance)column.InternalElement;
            try
            {
                return Create.ColumnFromRevitColumn(r_col);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class Model
    {
        public static void WriteJson(string filePath, Elements.Model model)
        {
            try
            {
                var json = model.ToJson();
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static void WriteGlb(string filePath, Elements.Model model)
        {
            try
            {
                model.ToGlTF(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static Elements.Model FromElements(IList<object> elements)
        {
            var model = new Elements.Model();
            var elems = elements.Cast<Elements.Element>().Where(e => e != null);

            model.AddElements(elems);

            return model;
        }
    }
}