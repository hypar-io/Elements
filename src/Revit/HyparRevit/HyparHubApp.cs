using Autodesk.Revit.UI;
using System.IO;
using Serilog;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using Hypar.Model;
using Autodesk.Revit.DB.ExternalService;
using System.Collections.Generic;

namespace Hypar.Revit
{
    public class HyparHubApp : IExternalApplication
    {
        private static HubConnection _hyparConnection;
        private static Dictionary<string, WorkflowSettings> _settings;

        public static HyparHubApp HyparApp { get; private set; }
        public static ILogger HyparLogger { get; private set; }
        public static Dictionary<string, Workflow> CurrentWorkflows { get; private set; }
        public static bool IsSyncing { get; set; }

        public static bool RequiresRedraw { get; set; }

        public Result OnShutdown(UIControlledApplication application)
        {
            if (_hyparConnection != null)
            {
                HyparLogger.Debug("Stopping the hypar hub application...");
                _hyparConnection.StopAsync().Wait();
                HyparLogger.Debug("Hypar hub application stopped.");
            }
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Use an assembly resolver to resolve assemblies relative to the executing assembly.
            // This is necessary if the addin is not in the application path, because
            // assembly resolution will try the application path first, then stop looking.
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            HyparApp = this;
            CurrentWorkflows = new Dictionary<string, Workflow>();
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
            // https://www.autodesk.com/autodesk-university/class/DirectContext3D-API-Displaying-External-Graphics-Revit-2017
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

        public bool Start(UIDocument uiDocument)
        {
            HyparLogger.Information("Creating hypar connection...");
            _hyparConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/functionHub")
                .Build();

            _hyparConnection.On<Dictionary<string, WorkflowSettings>>("WorkflowSettings", (settings) =>
            {
                _settings = settings;
            });

            _hyparConnection.On<Workflow>("WorkflowUpdated", (workflow) =>
            {
                HyparLogger.Information("Received workflow updated for {WorkflowId}", workflow);

                // Check if the settings include sync with this document.
                if (!_settings.ContainsKey(workflow.Id))
                {
                    return;
                }

                var workflowSettings = _settings[workflow.Id];
                var fileName = Path.GetFileName(uiDocument.Document.PathName);

                if (workflowSettings.Revit == null ||
                    workflowSettings.Revit.FileName == null ||
                    workflowSettings.Revit.FileName != fileName)
                {
                    HyparLogger.Debug("The current Revit file, {RevitFileName} was not associated with the workflow settings. No sync will occur.", fileName);
                    return;
                }

                var depPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hypar", "workflows", workflow.Id, $"{workflow.Id}.dll");
                if (File.Exists(depPath))
                {
                    HyparLogger.Information("Loading the dependencies assembly at {DepPath}.", depPath);
                    var asmBytes = File.ReadAllBytes(depPath);
                    var depAsm = AppDomain.CurrentDomain.Load(asmBytes);
                }

                if (!CurrentWorkflows.ContainsKey(workflow.Id))
                {
                    CurrentWorkflows.Add(workflow.Id, workflow);
                }
                else
                {
                    CurrentWorkflows[workflow.Id] = workflow;
                }

                RefreshView(uiDocument);
            });

            try
            {
                HyparLogger.Information("Starting the hypar connection...");
                _hyparConnection.StartAsync().Wait();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            try
            {
                HyparLogger.Information("Stopping the hypar connection...");
                _hyparConnection.StopAsync().Wait();
            }
            catch (Exception ex)
            {
                HyparLogger.Debug(ex.Message);
                HyparLogger.Debug(ex.StackTrace);
                return false;
            }
            return true;
        }

        public void RefreshView(UIDocument uiDocument)
        {
            RequiresRedraw = true;
            HyparLogger.Debug("Requires redraw has been set.");
            uiDocument.RefreshActiveView();
            HyparLogger.Debug("Refresh complete.");
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender,
                                                                 ResolveEventArgs args)
        {
            var dllName = args.Name.Split(',')[0];
            string execAsmPath = Path.GetDirectoryName(
                    System.Reflection.Assembly
                    .GetExecutingAssembly().Location);

            var filename = Path.Combine(execAsmPath, $"{dllName}.dll");

            if (File.Exists(filename))
            {
                HyparLogger.Information("Loading {AsmName}...", args.Name);
                return System.Reflection.Assembly.LoadFrom(filename);
            }
            return null;
        }
    }
}
