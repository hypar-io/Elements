using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Serialization.glTF;
using Elements.Serialization.JSON;
using RevitServices.Persistence;


namespace HyparDynamo.Hypar
{
    public static class Wall
    {
        /// <summary>
        /// gets the walls
        /// </summary>
        /// <param name="wall">The walls to be exported</param>
        /// <returns name="Hypar.Wall">The Hypar Wall element </param>
        public static Elements.Wall FromRevitWall( this Revit.Elements.Wall RevitWall) {
            var r_Wall = (Autodesk.Revit.DB.Wall)RevitWall.InternalElement;

            // wrapped exception catching to deliver more meaningful message in Dynamo
            try {
                return RevitHyparTools.Create.WallFromRevitWall(r_Wall);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class Floor {
        public static Elements.Floor[] FromRevitFloor(this Revit.Elements.Floor RevitFloor) {
            var r_Floor = (Autodesk.Revit.DB.Floor)RevitFloor.InternalElement;
            
            // wrapped exception catching to deliver more meaningful message in Dynamo
            try {
                return RevitHyparTools.Create.FloorsFromRevitFloor(DocumentManager.Instance.CurrentDBDocument, r_Floor);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class Column {
        public static string FromRevitColumn(this Revit.Elements.Element column) {
            throw new NotImplementedException("Conversion of Revit columns is not yet supported.");
        }
    }
    public static class Model {

        public static void WriteJson(string filePath, Elements.Model model) {
            try {
                var json = model.ToJson();
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
        public static void WriteGlb(string filePath, Elements.Model model) {
            try {
                model.ToGlTF(filePath);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public static Elements.Model ModelFromElements(IList<object> elements) {
            var model = new Elements.Model();
            var elems = elements.Cast<Elements.Element>().Where(e => e != null);

            model.AddElements(elems);

            return model;
        }
    }
}