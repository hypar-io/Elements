#pragma warning disable CS0067

using System;

namespace Hypar.Commands
{
    internal class HelpCommand : IHyparCommand
    {
        public string Message { get; set; }

        public event EventHandler CanExecuteChanged;

        public string Name
        {
            get{return "help";}
        }

        public string[] Arguments
        {
            get{return new string[]{};}
        }

        public string Description
        {
            get{return "";}
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Help();
        }

        public void Help()
        {
            ShowHelp();
        }

        private void ShowHelp()
        {
            Logger.LogInfo(
@"Hypar Command Line Usage:

hypar <command> <options>

Available Commands:
    execute     Execute a generator.
    generators  List all generators available in the system.
    help        Show the help.
    new         Create a new generator.
    publish     Publish a generator to Hypar.
    version     Show the version information.

For additional command help:
    hypar <command> help");
        }
    }
}