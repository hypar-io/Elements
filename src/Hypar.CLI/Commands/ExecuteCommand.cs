#pragma warning disable CS0067

using Hypar.API;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Collections.Generic;

namespace Hypar.Commands
{
    public class ExecuteCommand : IHyparCommand
    {
        private Dictionary<string,object> _args;

        private RestClient _client = new RestClient(Program.Configuration["hypar_api_url"]);

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;
            if(args[0] != "execute")
            {
                return false;
            }

            if(args.Length == 1)
            {
                Console.WriteLine("Hypar execute requires a function_id parameter.");
                return false;
            }

            // Avoid falling through to reading stdin.
            if(args[1] == "help")
            {
                return true;
            }

            if(!Console.IsInputRedirected)
            {
                Console.WriteLine("Hypar execute expects stdin to contain arguments.");
                return false;
            }
            else
            {
                try
                {
                    var input = Console.In.ReadToEnd();
                    _args = JsonConvert.DeserializeObject<Dictionary<string,object>>(input);
                }
                catch
                {
                    Console.WriteLine("The input data could not be deserialized to execution arguments.");
                    return false;
                }
            }

            return true;
        }

        public void Execute(object parameter)
        {
            var args = (string[])parameter;
            var functionId = args[1];
            Execute(functionId);
        }

        public void Help()
        {
            Console.WriteLine("Execute a function on hypar by providing its function id.");
            Console.WriteLine("Usage: hypar execute <function_id>");
        }

        private void Execute(string functionId, int? limit = null)
        {
            var request = new RestRequest("executions", Method.POST);
            request.AddHeader("x-api-key", Program.Configuration["hypar_api_key"]);
            request.RequestFormat = DataFormat.Json;

            var body = new Dictionary<string,object>();
            body.Add("function_id", functionId);
            body.Add("max_executions", limit!=null?limit:1);
            body.Add("arguments", _args);

            request.AddBody(body);
            var response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var executions = JsonConvert.DeserializeObject<Execution>(response.Content);
                Console.WriteLine(JsonConvert.SerializeObject(executions, Formatting.Indented));
            }
            else
            {
                Console.WriteLine($"There was an error executing {functionId} on Hypar.");
                Console.WriteLine(response.Content);
            }
            return;
        }
    }
}