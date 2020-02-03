using System;
using System.Linq;
using Elements.Serialization.glTF;
using RevitServices.Persistence;


namespace HyparDynamo.Hypar
{
    public class Wall
    {
        private Wall() {}

        /// <summary>
        /// gets the walls
        /// </summary>
        /// <param name="wall">The walls to be exported</param>
        /// <returns name="Hypar.Wall">The Hypar Wall element </param>
        public static Elements.Wall FromRevitWall( Revit.Elements.Wall incomingWall) {
            var r_Wall = (Autodesk.Revit.DB.Wall)incomingWall.InternalElement;

            // wrapped exception catching to deliver more meaningful message in Dynamo
            try {
                return RevitHyparTools.Create.WallFromRevitWall(r_Wall);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }

    public class Floor {
        private Floor() {}
        public static Elements.Floor[] FromRevitFloor(Revit.Elements.Floor incomingFloor) {
            var r_Floor = (Autodesk.Revit.DB.Floor)incomingFloor.InternalElement;
            
            // wrapped exception catching to deliver more meaningful message in Dynamo
            try {
                return RevitHyparTools.Create.FloorsFromRevitFloor(DocumentManager.Instance.CurrentDBDocument, r_Floor);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }

    public class Column {
        private Column() {}
        public static string ConvertRevitColumn(Revit.Elements.Element column) {
            throw new NotImplementedException("Conversion of Revit columns is not yet supported.");
        }
    }
    public class Model {
        private Model() {}

        public static void WriteGlb(string filePath, Elements.Model model) {
            try {
                model.ToGlTF(filePath);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public static Elements.Model ModelFromElements(object[] elements) {
            var model = new Elements.Model();
            var elems = elements.Cast<Elements.Element>().Where(e => e != null);
            
            model.AddElements(elems);

            return model;
        }
    }
}