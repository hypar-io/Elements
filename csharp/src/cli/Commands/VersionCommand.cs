#pragma warning disable CS0067

using System;
using System.Reflection;

namespace Hypar.Commands
{
    internal class VersionCommand : IHyparCommand
    {
        public event EventHandler CanExecuteChanged;

        public string Name
        {
            get{return "version";}
        }

        public string Description
        {
            get{return "Show the version of hypar and the hypar CLI.";}
        }

        public string[] Arguments
        {
            get{return new string[]{};}
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Version();
        }

        
        private void Version()
        {
            Logger.LogInfo($"Hypar Version {typeof(Hypar.Elements.Model).Assembly.GetName().Version.ToString()}");
            Logger.LogInfo($"Hypar CLI Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            return;
        }
    }
}