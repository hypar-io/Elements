#pragma warning disable CS0067

using Hypar.API;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace Hypar.Commands
{
    internal class GeneratorsCommand : IHyparCommand
    {
        private RestClient _client = new RestClient(Program.Configuration["hypar_api_url"]);

        public event EventHandler CanExecuteChanged;

        public string Name
        {
            get{return "generators";}
        }

        public string[] Arguments
        {
            get{return new string[]{};}
        }

        public string Description
        {
            get{return "List all generators available in Hypar.";}
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(!Cognito.Login())
            {
                return;
            }
            Functions();
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
                Logger.LogError("There was an error getting the generators from Hypar.");
            }
            return;
        }
    }
}