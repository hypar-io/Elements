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

hypar COMMAND OPTIONS

Available Commands:
    execute <function_id> <limit>   Executes the function identified by <function_id>, a maximum of <limit> times. If stdin contains a valid set of arguments for the function, then those will be used and <limit> will be ignored.
    executions <function_id>        Writes all executions for the specified <function_id> to stdout.
    functions                       Writes all functions available in Hypar to stdout.
    help                            Shows this help.
    model <output directory>        Write the models generated for the input executions to <output>.
    results                         Write the results of a set of executions from stdin to stdout.
    version                         Returns the current version of hypar.");
        }
    }
}