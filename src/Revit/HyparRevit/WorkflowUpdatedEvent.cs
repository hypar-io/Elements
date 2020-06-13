using System;
using System.IO;
using Autodesk.Revit.UI;
using Serilog;
using Hypar.Model;

namespace Hypar.Revit
{
    // file:///C:/Users/Ian/Downloads/External%20Event.pdf
    internal class WorkflowUpdatedEvent : RevitEventWrapper<Workflow>
    {
        public WorkflowUpdatedEvent(ILogger logger) : base(logger) { }

        public override void Execute(UIApplication app, Workflow workflow)
        {
            // This will be called whenever the workflow is updated.
            this.logger.Debug("We got a workflow update: {Id}", workflow.Id);

            // Read the workflow from disk
            // var workflowPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".hypar/workflows/{workflow.Id}");
            // if (!Directory.Exists(workflowPath))
            // {
            //     this.logger.Debug("The workflow path {Path} could not be found.", workflowPath);
            //     return;
            // }


        }
    }
}