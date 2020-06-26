using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Hypar.Revit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HyparHubStartCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (HyparHubApp.IsSyncing())
            {
                TaskDialog.Show("Hypar Hub Error", "The connection to the hub is already running.");
                return Result.Cancelled;
            }

            if (!HyparHubApp.HyparApp.Start(commandData.Application.ActiveUIDocument))
            {
                TaskDialog.Show("Hypar Hub Error", "The connection to the hub could not be started. Is the hub running?");
                return Result.Failed;
            }
            else
            {
                TaskDialog.Show("Hypar Hub", "The connection to the hub is now running.");
            }

            commandData.Application.ViewActivated += (sender, args) =>
            {
                HyparHubApp.HyparApp.RefreshView(commandData.Application.ActiveUIDocument);
            };

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HyparHubStopCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (!HyparHubApp.IsSyncing())
            {
                TaskDialog.Show("Hypar Hub Error", "The connection to the hub could not be stopped. Was the connection running?");
                return Result.Failed;
            }
            else
            {
                if (!HyparHubApp.HyparApp.Stop())
                {
                    TaskDialog.Show("Hypar Hub Error", "The connection to the hub could not be stopped.");
                    return Result.Failed;
                }
            }

            TaskDialog.Show("Hypar Hub", "The connection to the hub is stopped.");
            return Result.Succeeded;
        }
    }
}