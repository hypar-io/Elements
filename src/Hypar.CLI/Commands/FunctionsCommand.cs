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
        private RestClient _client = new RestClient(Constants.HYPAR_API_URL);

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;
            return args[0] == "functions";
        }

        public void Execute(object parameter)
        {
            Functions();
        }

        public void Help()
        {
            Console.WriteLine("List all functions available in Hypar.");
            Console.WriteLine("Usage: hypar functions");
        }

        private void Functions()
        {
            var request = new RestRequest("functions", Method.GET);
            request.AddHeader("x-api-key", Constants.HYPAR_API_KEY);
            var response = _client.Execute(request);
        
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var functions = JsonConvert.DeserializeObject<List<Function>>(response.Content);
                foreach(var f in functions)
                {
                    Console.WriteLine($"{f.Id}, {f.Description}");
                }
            }
            else
            {
                Console.WriteLine("There was an error getting the functions from hypar.");
            }
            return;
        }
    }
}