using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Hypar.Revit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HyparHubStartCommand : IExternalCommand
    {
        internal static bool _hubConnectionStarted = false;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (_hubConnectionStarted == true)
            {
                return Result.Cancelled;
            }

            if (!HyparHubApp.HyparApp.Start(commandData.Application.ActiveUIDocument))
            {
                TaskDialog.Show("Hypar Hub Error", "The connection to the hub could not be started. Is the hub running?");
                _hubConnectionStarted = false;
                return Result.Failed;
            }
            commandData.Application.ViewActivated += (sender, args) =>
            {
                HyparHubApp.HyparApp.RefreshView(commandData.Application.ActiveUIDocument);
            };
            HyparHubApp.IsSyncing = true;
            _hubConnectionStarted = true;

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HyparHubStopCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (HyparHubStartCommand._hubConnectionStarted == false || !HyparHubApp.HyparApp.Stop())
            {
                TaskDialog.Show("Hypar Hub Error", "The connection to the hub could not be stopped. Was the connection running?");
            }
            HyparHubApp.IsSyncing = false;
            return Result.Succeeded;
        }
    }
}