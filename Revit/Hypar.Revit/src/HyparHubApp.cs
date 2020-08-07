using Autodesk.Revit.UI;
using System.IO;
using Serilog;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using Hypar.Model;
using Autodesk.Revit.DB.ExternalService;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

namespace Hypar.Revit
{
    public class HyparHubApp : IExternalApplication
    {
        private static HubConnection _hyparConnection;
        private static Dictionary<string, WorkflowSettings> _settings;
        public static HyparHubApp HyparApp { get; private set; }
        public static ILogger HyparLogger { get; private set; }
        public static Dictionary<string, Workflow> CurrentWorkflows { get; private set; }
        public static bool RequiresRedraw { get; set; }

        public Result OnShutdown(UIControlledApplication application)
        {
            if (_hyparConnection != null)
            {
                HyparLogger.Debug("Stopping the hypar hub application...");
                Task.Run(async () => await _hyparConnection.StopAsync());
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
            HyparLogger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hypar", "hypar-revit.log"))
                            .CreateLogger();

            application.ControlledApplication.ApplicationInitialized += OnApplicationInitialized;

            AddRibbonButtons(application);

            return Result.Succeeded;
        }

        private void AddRibbonButtons(UIControlledApplication application)
        {
            var startHubButton = new PushButtonData("StartHub", "Start\nHypar\nHub", Assembly.GetCallingAssembly().Location, "Hypar.Revit.HyparHubStartCommand");
            var stopHubButton = new PushButtonData("StopHub", "Stop\nHypar\nHub", Assembly.GetCallingAssembly().Location, "Hypar.Revit.HyparHubStopCommand");
            var convertAllButton = new PushButtonData("HyparConvertAll", "Send All\nto Hypar", Assembly.GetCallingAssembly().Location, "Hypar.Revit.ConvertAllToHypar");
            var convertSelectionButton = new PushButtonData("HyparConvertSelection", "Send Selection\nto Hypar", Assembly.GetCallingAssembly().Location, "Hypar.Revit.ConvertSelectionToHypar");
            var convertViewButton = new PushButtonData("HyparConvertView", "Send Elements\nin View\nto Hypar", Assembly.GetCallingAssembly().Location, "Hypar.Revit.ConvertViewToHypar");
            var hyparHubPanel = application.CreateRibbonPanel("Hypar");
            hyparHubPanel.AddStackedItems(startHubButton, stopHubButton);
            hyparHubPanel.AddItem(convertAllButton);
            hyparHubPanel.AddItem(convertSelectionButton);
            hyparHubPanel.AddItem(convertViewButton);
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
            if (_hyparConnection == null)
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
            }

            HyparLogger.Information("Starting the hypar connection...");
            var task = Task.Run<bool>(async () =>
            {
                try
                {
                    await _hyparConnection.StartAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    HyparLogger.Debug(ex.Message);
                    HyparLogger.Debug(ex.StackTrace);
                    return false;
                }
            });
            return task.Result;
        }

        public bool Stop()
        {
            HyparLogger.Information("Stopping the hypar connection...");
            var task = Task.Run(async () =>
            {
                try
                {
                    await _hyparConnection.StopAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    HyparLogger.Debug(ex.Message);
                    HyparLogger.Debug(ex.StackTrace);
                    return false;
                }
            });
            return task.Result;
        }

        public void RefreshView(UIDocument uiDocument)
        {
            RequiresRedraw = true;
            HyparLogger.Debug("Requires redraw has been set.");
            uiDocument.RefreshActiveView();
            HyparLogger.Debug("Refresh complete.");
        }

        public static bool IsSyncing()
        {
            return _hyparConnection != null && _hyparConnection.State == HubConnectionState.Connected;
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
