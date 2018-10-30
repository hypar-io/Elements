using System;
using System.IO;
using System.IO.Compression;
using Autodesk.Forge;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace RevitAutomationDeploy
{
    internal class NewAppBundleRequest
    {
        [JsonProperty("id")]
        public string Id{get;set;}
        [JsonProperty("engine")]
        public string Engine{get;set;}
        [JsonProperty("description")]
        public string Description{get;set;}
    }

    internal class UploadParameters
    {
        [JsonProperty("endpointURL")]
        public string EndpointUrl{get;set;}
        [JsonProperty("formData")]
        public Dictionary<string,string> FormData{get;set;}
    }

    internal class NewAppBundleResponse
    {
        [JsonProperty("uploadParameters")]
        public UploadParameters UploadParameters{get;set;}
        [JsonProperty("engine")]
        public string Engine{get;set;}
        [JsonProperty("description")]
        public string Description{get;set;}
        [JsonProperty("version")]
        public int Version{get;set;}
        [JsonProperty("id")]
        public string Id{get;set;}
    }

    internal class NewVersionResponse
    {
        [JsonProperty("package")]
        public string Package{get;set;}
        [JsonProperty("engine")]
        public string Engine{get;set;}
        [JsonProperty("description")]
        public string Description{get;set;}
        [JsonProperty("version")]
        public int Version{get;set;}
        [JsonProperty("id")]
        public string Id{get;set;}
    }

    class Program
    {
        private const string EXPLORE_CLIENT_ID = "EXPLORE_CLIENT_ID";
        private const string EXPLORE_CLIENT_SECRET = "EXPLORE_CLIENT_SECRET";

        private const string _appId = "HyparRevit";
        private static string _baseUrl = "https://developer.api.autodesk.com/da/us-east";
        
        private static RestSharp.RestClient _client;

        static void Main(string[] args)
        {
            _client = new RestSharp.RestClient(_baseUrl);
        
            ZipAppBundle();

            var token = GetAccessToken();

            CreateOrUpdateAppBundle(token);

            // Upload app bundle zip.
            //https://dasprod-store.s3.amazonaws.com
        }

        private static void CreateOrUpdateAppBundle(string token)
        {
            var request = new RestSharp.RestRequest("/v3/appbundles", RestSharp.Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            var body = new NewAppBundleRequest(){
                Id = _appId,
                Engine = "Autodesk.Revit+2019",
                Description = "Convert Hypar Models to Revit."
            };
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(body);
            var response = _client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK)
            {
                if(response.StatusCode == HttpStatusCode.Conflict)
                {
                    Console.WriteLine("Package already exists, updating...");
                    var newVersion = UpdateAppBundle(token);
                }
                else
                {
                    throw new Exception($"There was an error creating a new app bundle: {response.Content}");
                }
            } 
            else
            {
                Console.WriteLine(response.Content);
                var bundleResponse = JsonConvert.DeserializeObject<NewAppBundleResponse>(response.Content);
            }
        }

        private static NewVersionResponse UpdateAppBundle(string token)
        {
            var request = new RestSharp.RestRequest($"/v3/appbundles/{_appId}/versions", RestSharp.Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = RestSharp.DataFormat.Json;
            var body = new NewAppBundleRequest(){
                Id = null,
                Engine = "Autodesk.Revit+2019",
                Description = "Convert Hypar Models to Revit."
            };
            request.AddBody(body);
            var response = _client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"There was an error creating a new app bundle: {response.Content}");
            }
            Console.WriteLine(response.Content);
            var newVersionResponse = JsonConvert.DeserializeObject<NewVersionResponse>(response.Content);
            return newVersionResponse;
        }
        private static string GetAccessToken()
        {
            var client = new Autodesk.Forge.TwoLeggedApi();
            var clientId = Environment.GetEnvironmentVariable(EXPLORE_CLIENT_ID, EnvironmentVariableTarget.User);
            var clientSecret = Environment.GetEnvironmentVariable(EXPLORE_CLIENT_SECRET, EnvironmentVariableTarget.User);
            if(clientId == string.Empty || clientSecret == string.Empty)
            {
                throw new Exception($"The {EXPLORE_CLIENT_ID} and {EXPLORE_CLIENT_SECRET} environment variables must be set.");
            }
            var result = (Autodesk.Forge.Model.DynamicJsonResponse)client.Authenticate(clientId, clientSecret, "client_credentials", new []{Scope.CodeAll});
            Console.WriteLine(result);
            return (string)result.Dictionary["access_token"];
        }

        private static void ZipAppBundle()
        {
            var root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../HyparRevitApp"));
            var bundleDir = Path.Combine(root, "HyparRevit.bundle");
            var zipPath = Path.Combine(root, "HyparRevitApp.zip");
            
            if(!Directory.Exists(bundleDir))
            {
                throw new Exception($"The specified bundle directory, {bundleDir}, does not exist.");
            }

            if(File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            ZipFile.CreateFromDirectory(bundleDir, zipPath);
        }
    }
}
