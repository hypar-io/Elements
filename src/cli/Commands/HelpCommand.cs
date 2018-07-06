#pragma warning disable CS0067

using System;

namespace Hypar.Commands
{
    internal class HelpCommand : IHyparCommand
    {
        public string Message { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;
            return args[0] == "help";
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
            Console.WriteLine(@"Hypar Command Line Usage:

hypar <command> <options>

Available Commands:
    execute
    executions
    functions
    help
    model
    publish
    results
    version

For additional command help:
    hypar <command> help");
        }
    }
}