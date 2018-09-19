#pragma warning disable CS0067

using Hypar.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Commands
{
    internal class ResultsCommand : IHyparCommand
    {
        private List<Execution> _executions;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;

            if(args[0] != "results")
            {
                return false;
            }

            if(Console.IsInputRedirected)
            {
                var input = Console.In.ReadToEnd();
                try
                {
                    _executions = JsonConvert.DeserializeObject<List<Execution>>(input);
                }
                catch
                {
                    Logger.LogError("The input could not be deserialized to an array of executions.");
                    return false;
                }
            }
            else
            {
                Logger.LogError("Hypar results requires an array of executions.");
                return false;
            }

            return true;
        }

        public void Execute(object parameter)
        {
            Results();
        }

        public void Help()
        {
            Logger.LogInfo("Read executions from stdin and write results to stdout.");
            Logger.LogInfo("Usage: hypar results");
        }

        private void Results()
        {
            var results = new Dictionary<string,List<object>>();
            foreach(var e in _executions)
            {
                foreach(var kvp in e.Computed)
                {
                    if(results.ContainsKey(kvp.Key))
                    {
                        results[kvp.Key].Add(kvp.Value);
                    }
                    else
                    {
                        results.Add(kvp.Key, new List<object>{kvp.Value});
                    }
                }
            }
            Logger.LogInfo(string.Join(",",results.Keys));

            var length = results.Values.ElementAt(0).Count;
            for(var i=0; i<length; i++)
            {
                var line = new List<object>();
                foreach(var kvp in results)
                {
                    line.Add(kvp.Value.ElementAt(i));
                }
                Logger.LogInfo(string.Join(",", line));
            }
        }
    }
}