using Autodesk.Revit.UI;
using System.IO;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Hypar.Model;
using Autodesk.Revit.DB.ExternalService;

namespace Hypar.Revit
{
    public class HyparHubApp : IExternalApplication
    {
        private static HubConnection _hyparConnection;

        public static HyparHubApp HyparApp { get; private set; }
        public static ILogger HyparLogger { get; private set; }
        public static Workflow CurrentWorkflow { get; private set; }
        public static bool IsSyncing { get; set; }

        public Result OnShutdown(UIControlledApplication application)
        {
            if (_hyparConnection != null)
            {
                HyparLogger.Debug("Stopping the hypar hub application...");
                Task.Run(async () => await _hyparConnection.StopAsync()).Wait();
                HyparLogger.Debug("Hypar hub application stopped.");
            }
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            HyparApp = this;
            IsSyncing = false;
            HyparLogger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hypar", "hypar-revit.log"))
                            .CreateLogger();

            application.ControlledApplication.ApplicationInitialized += OnApplicationInitialized;

            return Result.Succeeded;
        }

        private void OnApplicationInitialized(object sender, EventArgs e)
        {
            // file:///C:/Users/Ian/Downloads/ee1ea1c8-7911-4ecd-abe5-8f1e2ee1d81a.ClassHandoutAS125032DirectContext3DAPIforDisplayingExternalGraphicsinRevitAlexPytel1%20(2).pdf
            var service = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService) as MultiServerService;
            if (service != null)
            {
                HyparLogger.Debug("Registering the hypar service for drawing...");
                var hyparServer = new HyparDirectContextServer(HyparLogger);
                service.AddServer(hyparServer);
                var active = service.GetActiveServerIds();
                active.Add(hyparServer.GetServerId());
                service.SetActiveServers(active);
            }
            else
            {
                HyparLogger.Debug("Could not find the Direct3DContextService.");
            }
        }

        public void Start()
        {
            HyparLogger.Information("Creating hypar connection...");
            _hyparConnection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/functionHub")
                    .Build();

            _hyparConnection.On<Workflow>("WorkflowUpdated", (workflow) =>
            {
                HyparLogger.Information("Received workflow updated for {WorkflowId}", workflow);
                CurrentWorkflow = workflow;

                var depPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hypar", "workflows", workflow.Id, $"{workflow.Id}.dll");
                if(File.Exists(depPath))
                {
                    var asmBytes = File.ReadAllBytes(depPath);
                    var depAsm = AppDomain.CurrentDomain.Load(asmBytes);
                    HyparLogger.Information("Loading the dependencies assembly at {DepPath}.", depPath);
                    Assembly.LoadFile(depPath);
                }
            });

            HyparLogger.Information("Starting hypar connection...");
            Task.Run(async () => await _hyparConnection.StartAsync()).Wait();
        }

        public void Stop()
        {
            if (_hyparConnection != null)
            {
                Task.Run(async () => await _hyparConnection.StopAsync()).Wait();
            }
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender,
                                                                 ResolveEventArgs args)
        {
            var dllName = args.Name.Split(',')[0];
            string execAsmPath = Path.GetDirectoryName(
                    System.Reflection.Assembly
                    .GetExecutingAssembly().Location);

            var filename = Path.Combine(execAsmPath, $"{dllName}.dll");
            HyparLogger.Information("Resolving {AsmName}...", args.Name);
            if (File.Exists(filename))
            {
                return System.Reflection.Assembly.LoadFrom(filename);
            }
            return null;
        }
    }
}
