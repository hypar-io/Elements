using Autodesk.Revit.UI;
using System.IO;
using Serilog;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using Hypar.Model;
using System.Linq;
using Autodesk.Revit.DB;

namespace Hypar.Revit
{
    public class HyparHubApp : IExternalApplication
    {
        public static HyparHubApp HyparApp;
        public static ILogger HyparLogger;
        private static HubConnection hyparConnection;
        public static Workflow CurrentWorkflow;
        public static Dictionary<string, ElementId> FunctionInstanceGroupCache = new Dictionary<string, ElementId>();
        
        public static List<string> GroupCache = new List<string>();
        
        public Result OnShutdown(UIControlledApplication application)
        {
            if(hyparConnection != null)
            {
                HyparLogger.Debug("Stopping the hypar hub application...");
                Task.Run(async () => await hyparConnection.StopAsync()).Wait();
                HyparLogger.Debug("Hypar hub application stopped.");
            }
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            HyparApp = this;

            HyparLogger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hypar", "hypar-revit.log"))
                            .CreateLogger();

            return Result.Succeeded;
        }

        public void Start()
        {
            var workflowEvent = new WorkflowUpdatedEvent(HyparLogger);
            var executionEvent = new ExecutionRequestedEvent(HyparLogger);

            HyparLogger.Information("Creating hypar connection...");
            hyparConnection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/functionHub")
                    .Build();

            hyparConnection.On<Workflow>("WorkflowUpdated", (workflow) =>
            {
                HyparLogger.Information("Received workflow updated for {WorkflowId}", workflow);
                CurrentWorkflow = workflow;
                // workflowEvent.Raise(workflow);
            });

            hyparConnection.On<Execution>("ExecutionRequested", (execution) =>
            {
                HyparLogger.Information("Received execution request for {ExecutionId}", execution);
                executionEvent.Raise(execution);
            });
            
            HyparLogger.Information("Starting hypar connection...");
            Task.Run(async ( )=> await hyparConnection.StartAsync()).Wait();
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender,
                                                                 ResolveEventArgs args)
        {
            var dllName = args.Name.Split(',')[0];
            HyparLogger.Debug("Resolving {Name}", dllName);
            string execAsmPath = Path.GetDirectoryName(
                    System.Reflection.Assembly
                    .GetExecutingAssembly().Location );

            var filename  = Path.Combine( execAsmPath, $"{dllName}.dll");
            if( File.Exists( filename ) )
            {
                return System.Reflection.Assembly.LoadFrom( filename );
            }
            return null;
        }
    }
}
