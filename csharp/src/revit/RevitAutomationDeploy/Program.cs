using Amazon.S3;
using Amazon.S3.Model;
using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.IO;
using System.IO.Compression;
using Autodesk.Forge;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RevitAutomationDeploy
{
    //http://v3doc.s3-website-us-west-2.amazonaws.com/#/AppsApi/V3AppbundlesByIdAliasesByAliasIdGet

    class Program
    {
        private const string FORGE_CLIENT_ID = "FORGE_CLIENT_ID";
        private const string FORGE_CLIENT_SECRET = "FORGE_CLIENT_SECRET";
        private const string REVIT_AUTOMATION_BUCKET_NAME = "REVIT_AUTOMATION_BUCKET_NAME";
        private const string REVIT_AUTOMATION_KEY = "REVIT_AUTOMATION_KEY";
        
        private static RestSharp.RestClient _client;

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                throw new Exception("Usage: RevitAutomationDeploy [args] [alias]");
            }

            _client = new RestSharp.RestClient("https://developer.api.autodesk.com/da/us-east/v3");
            var appAlias = args[0];
            var nickname = "Hypar";
            var activityAlias = "dev";
            var appId = "HyparRevit";
            var activityId = $"{appId}Activity";
            var engine = "Autodesk.Revit+2019";
            var clientId = Environment.GetEnvironmentVariable(FORGE_CLIENT_ID, EnvironmentVariableTarget.User);
            var modelUrl = "https://s3-us-west-1.amazonaws.com/hypar-revit/hypar.rvt";

            var token = GetAccessToken(clientId);
            if(args[0] == "-token")
            {
                Console.WriteLine(token);
                return;
            }
            
            if(args[0] == "-t")
            {
                var model = CreateTestModel();
                var data = new Dictionary<string,string>();
                data["id"] = Guid.NewGuid().ToString();
                data["model"] = model.ToJson();
                var json = JsonConvert.SerializeObject(data);
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "execution.json"), json);
                return;
            }

            if(args[0] == "-n")
            {
                if(args.Length == 1)
                {
                    throw new Exception("The 'nickname' argument requires a nickname.");
                }
                CreateNickname(args[1], token);
                return;
            }

            if(args[0] == "-w")
            {
                if(args.Length == 1)
                {
                    throw new Exception("You must supply an execution id as the second argument.");
                }
                var url = GeneratePreSignedURL(args[1]);
                CreateTestWorkItem(args[1], url, modelUrl, token, nickname, activityId, activityAlias);
                return;
            }

            PublishApp();

            var zipBundlePath = ZipAppBundle();

            CreateOrUpdateAppBundle(token, appId, appAlias, zipBundlePath, engine);
            CreateActivity(token, activityId, nickname, appId, appAlias, engine, activityAlias);
        }

        private static void CreateTestWorkItem(string executionId, string presignedUrl, string modelUrl, string token, string nickname, string activityId, string activityAlias)
        {
            var request = new RestSharp.RestRequest("/workitems", RestSharp.Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");

            // var model = CreateTestModel();
            // var modelJson = model.ToJson().Replace("\"","'").Replace("\r\n","").Replace("\t","").Replace("  ","");
            
            var body = new Dictionary<string,object>(){
                {"activityId", $"{nickname}.{activityId}+{activityAlias}"},
                {"arguments", new Dictionary<string,object>(){
                    {"rvtFile", new Dictionary<string,object>(){
                        {"url",modelUrl}
                    }},
                    {"result", new Dictionary<string,object>(){
                        {"verb","put"},
                        {"url", presignedUrl}
                    }},
                    {"execution",new Dictionary<string,object>(){
                        // {"url", $"data:application/json,{{'id': '{executionId}', 'model': {modelJson}}}"}
                        {"url", $"https://s3-us-west-1.amazonaws.com/hypar-executions-dev/{executionId}_elements.json"}
                    }}
                }}
            };
            var bodyJson = JsonConvert.SerializeObject(body);
            request.AddParameter("application/json", bodyJson, RestSharp.ParameterType.RequestBody);

            var response = _client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"There was an error submitting the test work item: {response.StatusCode} - {response.Content}");
            }
            Console.WriteLine(response.Content);
        }

        private static void CreateNickname(string nickname, string token)
        {
            Console.WriteLine($"Getting the existing app nickname.");
            var request = new RestSharp.RestRequest("/forgeapps/me", RestSharp.Method.GET);
            request.AddHeader("Authorization", $"Bearer {token}");
            var response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"The forge app has the nickname: {response.Content}");
            }

            Console.WriteLine($"Creating the nickname '{nickname}'.");

            request = new RestSharp.RestRequest("/forgeapps/me", RestSharp.Method.PATCH);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            var body = new Dictionary<string,string>(){
                {"nickname", nickname}
            };
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(body);
            response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.NoContent)
            {
                Console.WriteLine("Nickname successfully updated.");
            }
            else
            {
                throw new Exception($"There was an error creating the nickname: {response.StatusCode} - {response.Content}");
            }
        }

        private static void PublishApp()
        {
            Console.WriteLine("Publishing the app.");

            var process = new System.Diagnostics.Process();
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "dotnet";
            startInfo.Arguments = "publish -c Release -f netstandard2.0";
            startInfo.WorkingDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../HyparRevitApp"));
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        private static void CreateOrUpdateAppBundle(string token, string appId, string alias, string bundleZipPath, string engine)
        {
            Console.WriteLine("Attempting to create the app.");

            var request = new RestSharp.RestRequest("/appbundles", RestSharp.Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            var body = new NewAppBundleRequest(){
                Id = appId,
                Engine = engine,
                Description = "Convert Hypar Models to Revit."
            };
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(body);
            var response = _client.Execute(request);
            AppBundleResponse appBundleResponse;

            if(response.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("App already exists, updating.");

                appBundleResponse = UpdateAppBundle(token, appId);
                UploadAppBundle(appBundleResponse.UploadParameters, bundleZipPath);
            }
            else if(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
            {
                appBundleResponse = JsonConvert.DeserializeObject<AppBundleResponse>(response.Content);
                UploadAppBundle(appBundleResponse.UploadParameters, bundleZipPath);
            } 
            else
            {
                throw new Exception($"There was an error creating a new app bundle: {response.StatusCode} - {response.Content}");
            }
            UpdateOrCreateAlias(appBundleResponse.Version, appId, alias, token);
        }

        private static void UpdateOrCreateAlias(int version, string appId, string alias, string token)
        {
            Console.WriteLine($"Getting the alias, {alias}, for app, {appId}, version {version}.");

            var request = new RestSharp.RestRequest($"/appbundles/{appId}/aliases/{alias}", RestSharp.Method.GET);
            request.AddHeader("Authorization", $"Bearer {token}");
            var response = _client.Execute(request);
            
            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"The alias, {alias}, could not be found.");
                CreateAppAlias(version, appId, alias, token);
            }
            else if(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine($"The alias, {alias}, was found.");
                UpdateAppAlias(version, appId, alias, token);
            }
        }

        private static void CreateAppAlias(int version, string appId, string alias, string token)
        {
            Console.WriteLine($"Creating the alias, {alias}, for app bundle, {appId}, version {version}.");

            var request = new RestSharp.RestRequest($"/appbundles/{appId}/aliases", RestSharp.Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            var data = new Dictionary<string,object>()
            {
                {"version", version},
                {"id", alias}
            };
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(data);
            var response = _client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"There was an error creating the app alias: {response.StatusCode} - {response.Content}");
            }
        }

        private static void UpdateAppAlias(int version, string appId, string alias, string token)
        {
            Console.WriteLine($"Updating the alias, {alias}, for app bundle, {appId}, version {version}.");
            var request = new RestSharp.RestRequest($"/appbundles/{appId}/aliases/{alias}", RestSharp.Method.PATCH);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            var data = new Dictionary<string,object>()
            {
                {"version",version},
            };
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(data);
            var response = _client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"There was an error updating the app alias: {response.StatusCode} - {response.Content}");
            }
        }

        private static AppBundleResponse UpdateAppBundle(string token, string appId)
        {
            Console.WriteLine($"Updating app {appId}.");

            var request = new RestSharp.RestRequest($"/appbundles/{appId}/versions", RestSharp.Method.POST);
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
                throw new Exception($"There was an error creating a new app bundle: {response.StatusCode} - {response.Content}");
            }
            var newVersionResponse = JsonConvert.DeserializeObject<AppBundleResponse>(response.Content);
            return newVersionResponse;
        }
        
        private static string GetAccessToken(string clientId)
        {
            var client = new Autodesk.Forge.TwoLeggedApi();
            var clientSecret = Environment.GetEnvironmentVariable(FORGE_CLIENT_SECRET, EnvironmentVariableTarget.User);
            if(clientId == string.Empty || clientSecret == string.Empty)
            {
                throw new Exception($"The {FORGE_CLIENT_ID} and {FORGE_CLIENT_SECRET} environment variables must be set.");
            }
            var result = (Autodesk.Forge.Model.DynamicJsonResponse)client.Authenticate(clientId, clientSecret, "client_credentials", new []{Scope.CodeAll});
            return (string)result.Dictionary["access_token"];
        }
        
        private static string ZipAppBundle()
        {
            var root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../HyparRevitApp"));
            var bundleDir = Path.Combine(root, "HyparRevit.bundle");
            var zipPath = Path.Combine(root, "HyparRevitApp.zip");
            Console.WriteLine($"Zipping the app bundle located at {bundleDir}");

            if(!Directory.Exists(bundleDir))
            {
                throw new Exception($"The specified bundle directory, {bundleDir}, does not exist.");
            }

            if(File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            ZipFile.CreateFromDirectory(bundleDir, zipPath, CompressionLevel.Optimal, true);
            return zipPath;
        }

        private static string GeneratePreSignedURL(string key)
        {
            //https://docs.aws.amazon.com/AmazonS3/latest/dev/ShareObjectPreSignedURLDotNetSDK.html

            var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest1);
            string urlString = "";
            try
            {
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = Environment.GetEnvironmentVariable(REVIT_AUTOMATION_BUCKET_NAME, EnvironmentVariableTarget.User),
                    Key = $"{key}.rvt",
                    Expires = DateTime.Now.AddMinutes(60),
                    Verb = HttpVerb.PUT,
                };
                urlString = s3Client.GetPreSignedURL(request1);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return urlString;
        }
    
        private static Model CreateTestModel()
        {
            var model = new Model();
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var beam = new Beam(line, WideFlangeProfileServer.Instance.GetProfileByName("W6x25"), BuiltInMaterials.Steel);
            model.AddElement(beam);

            var wallLine = new Line(new Vector3(10,0,0), new Vector3(15,10,0));
            var wallType = new WallType("concrete", 0.1);
            var wall = new Wall(wallLine, wallType, 5.0, null, BuiltInMaterials.Concrete);
            model.AddElement(wall);

            var column = new Column(Vector3.Origin, 5.0, WideFlangeProfileServer.Instance.GetProfileByName("W6x25"), BuiltInMaterials.Steel);
            model.AddElement(column);

            var floorType = new FloorType("concrete", 0.1);
            var floor = new Floor(Polygon.Rectangle(Vector3.Origin, 10, 10), floorType, 5.0, BuiltInMaterials.Concrete);
            model.AddElement(floor);

            return model;
        }
    
        private static void UploadAppBundle(UploadParameters parameters, string bundleZipPath)
        {
            Console.WriteLine($"Uploading app bundle to {parameters.EndpointUrl}.");

            var client = new RestSharp.RestClient(parameters.EndpointUrl);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.AddHeader("Cache-Control", "no-cache");
            request.AlwaysMultipartFormData = true;
            request.AddFile("file",bundleZipPath);
            foreach(var f in parameters.FormData)
            {
                request.AddParameter(f.Key, f.Value, RestSharp.ParameterType.GetOrPost);
            }
            var response = client.Execute(request);
            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"There was an error uploading the app bundle: {response.Content}");
            }
            Console.WriteLine("App bundle uploaded succesfully.");
        }

        private static void CreateActivity(string token, string activityId, string nickname, string appId, string appAlias, string engine, string activityAlias)
        {
            Console.WriteLine($"Creating new activity, {activityId}.");

            var request = new RestSharp.RestRequest("/activities", RestSharp.Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            var activity = new ActivityRequest(){
                Id = activityId,
                CommandLine = new[]{$"$(engine.path)\\\\revitcoreconsole.exe /i $(args[rvtFile].path) /al $(appbundles[{appId}].path)"},
                Parameters = new ActivityParameters(){
                    RvtFile = new Parameter(){
                        Zip = false,
                        OnDemand = false,
                        Verb = "get",
                        Description = "Input revit model.",
                        Required = true,
                        LocalName = "$(rvtFile)"
                    },
                    Result = new Parameter(){
                        Zip = false,
                        OnDemand = false,
                        Verb = "put",
                        Description = "Results",
                        Required = true,
                        LocalName = "result.rvt"
                    },
                    Execution = new Parameter(){
                        Zip = false,
                        OnDemand = false,
                        Verb = "get",
                        Description = " Hypar execution.",
                        Required = true,
                        LocalName = "execution.json"
                    }
                },
                Engine = engine,
                AppBundles = new[]{$"{nickname}.{appId}+{appAlias}"},
                Description = "Convert Hypar Model to Revit."
            };

            // RestSharp's serializer mangles the body. We need to send the pre-serialized body.
            var body = JsonConvert.SerializeObject(activity, Formatting.Indented);
            request.AddParameter("application/json", body, RestSharp.ParameterType.RequestBody);
            
            var response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine($"The activity, {activityId}, already exists.");
                CreateNewActivityAlias(token, activityId, activityAlias,1);
            }
            else if(response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"The activity, {activityId}, was created.");
                var data = JsonConvert.DeserializeObject<Dictionary<string,object>>(response.Content);
                var version = Int32.Parse(data["version"].ToString());
                CreateNewActivityAlias(token, activityId, activityAlias, version);
            }
            else
            {
                throw new Exception($"There was an error creating the activity: {response.StatusCode} - {response.Content}");
            }
        }

        private static void CreateNewActivityAlias(string token, string activityId, string activityAlias, int version)
        {
            Console.WriteLine($"Creating new activity alias for activity, {activityId}.");

            var request = new RestSharp.RestRequest($"/activities/{activityId}/aliases", RestSharp.Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = RestSharp.DataFormat.Json;
            var data = new Dictionary<string,object>(){
                {"version",1},
                {"id", activityAlias}
            };
            request.AddBody(data);
            var response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine($"The alias, {activityAlias}, already exists.");
            }
            else if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"There was an error creating the activity alias: {response.StatusCode} - {response.Content}");
            }
        }
    }
}
