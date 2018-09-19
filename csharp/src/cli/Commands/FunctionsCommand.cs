#pragma warning disable CS0067

using Hypar.API;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace Hypar.Commands
{
    internal class FunctionsCommand : IHyparCommand
    {
        private RestClient _client = new RestClient(Program.Configuration["hypar_api_url"]);

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;
            return args[0] == "functions";
        }

        public void Execute(object parameter)
        {
            if(!Cognito.Login())
            {
                return;
            }
            Functions();
        }

        public void Help()
        {
            Logger.LogInfo("List all functions available in Hypar.");
            Logger.LogInfo("Usage: hypar functions");
        }

        private void Functions()
        {
            var request = new RestRequest("functions", Method.GET);
            request.AddHeader("x-api-key", Program.Configuration["hypar_api_key"]);
            request.AddHeader("Authorization", Cognito.IdToken);

            var response = _client.Execute(request);
        
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var functions = JsonConvert.DeserializeObject<List<Function>>(response.Content);
                foreach(var f in functions)
                {
                    Logger.LogInfo($"{f.Id}, {f.Description}");
                }
            }
            else
            {
                Logger.LogError("There was an error getting the functions from hypar.");
            }
            return;
        }
    }
}