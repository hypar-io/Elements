#pragma warning disable CS0067

using Hypar.API;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace Hypar.Commands
{
    internal class ExecutionsCommand : IHyparCommand
    {
        private RestClient _client = new RestClient(Program.Configuration["hypar_api_url"]);

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;

            if(args[0] != "executions")
            {
                return false;
            }

            if(args.Length != 2)
            {
                Logger.LogError("Hypar executions requires a function id parameter.");
                return false;
            }

            return true;
        }

        public void Execute(object parameter)
        {
            if(!Cognito.Login())
            {
                return;
            }

            var args = (string[])parameter;
            var functionId = args[1];
            Executions(functionId);
        }

        public void Help()
        {
            Logger.LogInfo("Gets all executions in Hypar for the specified function id and writes them to stdout.");
            Logger.LogInfo("Usage: hypar executions <function_id>");
        }

        private void Executions(string functionId)
        {
            var request = new RestRequest("executions", Method.GET);
            request.AddHeader("x-api-key", Program.Configuration["hypar_api_key"]);
            request.AddHeader("Authorization", Cognito.IdToken);
            request.AddParameter("function_id",functionId);
            var response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var executions = JsonConvert.DeserializeObject<List<Execution>>(response.Content);
                Logger.LogInfo(JsonConvert.SerializeObject(executions, Formatting.Indented));
            }
            else
            {
                Logger.LogError($"There was an error executing the function, {functionId}, on Hypar.");
            }
            return;
        }
    }
}