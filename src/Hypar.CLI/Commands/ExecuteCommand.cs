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
        int _limit = -1;

        private RestClient _client = new RestClient(Constants.HYPAR_API_URL);

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
            
            if(args.Length == 3)
            {
                if(!int.TryParse((string)args[2], out _limit))
                {
                    Console.WriteLine("The specified limit is invalid.");
                    return false;
                }
            }

            return true;
        }

        public void Execute(object parameter)
        {
            var args = (string[])parameter;
            var functionId = args[1];
            Execute(functionId, _limit);
        }

        public void Help()
        {
            Console.WriteLine("Execute a function on hypar by providing its function id.");
            Console.WriteLine("Usage: hypar execute <function_id> <limit>");
        }

        private void Execute(string functionId, int? limit = null)
        {
            var request = new RestRequest("executions", Method.POST);
            request.AddHeader("x-api-key", Constants.HYPAR_API_KEY);
            request.RequestFormat = DataFormat.Json;

            var body = new Dictionary<string,object>();
            body.Add("function_id", functionId);
            body.Add("max_executions", limit!=null?limit:1);
            if(Console.IsInputRedirected)
            {
                var input = Console.In.ReadToEnd();
                var args = JsonConvert.DeserializeObject<Dictionary<string,object>>(input);
                body.Add("args", args);
            }
            request.AddBody(body);
            var response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var executions = JsonConvert.DeserializeObject<List<Execution>>(response.Content);
                Console.WriteLine(JsonConvert.SerializeObject(executions, Formatting.Indented));
            }
            else
            {
                Console.WriteLine($"There was an error executing the function, {functionId}, on Hypar.");
            }
            return;
        }
    }
}