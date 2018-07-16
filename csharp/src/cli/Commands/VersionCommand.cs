#pragma warning disable CS0067

using System;
using System.Reflection;

namespace Hypar.Commands
{
    internal class VersionCommand : IHyparCommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;

            if(args[0] != "version")
            {
                return false;
            }
            return true;
        }

        public void Execute(object parameter)
        {
            Version();
        }

        public void Help()
        {
            Console.WriteLine("Show the version of hypar and the hypar CLI.");
            Console.WriteLine("Usage: hypar version");
        }

        private void Version()
        {
            Console.WriteLine($"Hypar Version {typeof(Hypar.Elements.Model).Assembly.GetName().Version.ToString()}");
            Console.WriteLine($"Hypar CLI Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            return;
        }
    }
}