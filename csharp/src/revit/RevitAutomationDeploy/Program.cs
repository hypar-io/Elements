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
    class Program
    {
        private const string FORGE_CLIENT_ID = "FORGE_CLIENT_ID";
        private const string FORGE_CLIENT_SECRET = "FORGE_CLIENT_SECRET";
        private const string REVIT_AUTOMATION_BUCKET_NAME = "HYPAR_REVIT_BUCKET_NAME";
        private const string REVIT_AUTOMATION_KEY = "HYPAR_REVIT_KEY";
        private const string _appId = "HyparRevit";
        private static string _baseUrl = "https://developer.api.autodesk.com/da/us-east";
        
        private static RestSharp.RestClient _client;

        static void Main(string[] args)
        {
            if(args.Length > 0 && args.Contains("-t"))
            {
                var model = CreateTestModel();
                var data = new Dictionary<string,string>();
                data["id"] = Guid.NewGuid().ToString();
                data["model"] = model.ToJson();
                var json = JsonConvert.SerializeObject(data);
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "execution.json"), json);
                return;
            }

            _client = new RestSharp.RestClient(_baseUrl);
            
            PublishApp();

            var zipBundlePath = ZipAppBundle();

            var token = GetAccessToken();

            CreateOrUpdateAppBundle(token, zipBundlePath);
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

        private static void CreateOrUpdateAppBundle(string token, string bundleZipPath)
        {
            Console.WriteLine("Attempting to create the app.");
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
                    Console.WriteLine("App already exists, updating...");
                    var newVersion = UpdateAppBundle(token);
                    UploadAppBundle(newVersion.UploadParameters, bundleZipPath);
                }
                else
                {
                    throw new Exception($"There was an error creating a new app bundle: {response.Content}");
                }
            } 
            else
            {
                Console.WriteLine(response.Content);
                var bundleResponse = JsonConvert.DeserializeObject<AppBundleResponse>(response.Content);
                UploadAppBundle(bundleResponse.UploadParameters, bundleZipPath);
            }
        }

        private static AppBundleResponse UpdateAppBundle(string token)
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
            var newVersionResponse = JsonConvert.DeserializeObject<AppBundleResponse>(response.Content);
            return newVersionResponse;
        }
        private static string GetAccessToken()
        {
            var client = new Autodesk.Forge.TwoLeggedApi();
            var clientId = Environment.GetEnvironmentVariable(FORGE_CLIENT_ID, EnvironmentVariableTarget.User);
            var clientSecret = Environment.GetEnvironmentVariable(FORGE_CLIENT_SECRET, EnvironmentVariableTarget.User);
            if(clientId == string.Empty || clientSecret == string.Empty)
            {
                throw new Exception($"The {FORGE_CLIENT_ID} and {FORGE_CLIENT_SECRET} environment variables must be set.");
            }
            var result = (Autodesk.Forge.Model.DynamicJsonResponse)client.Authenticate(clientId, clientSecret, "client_credentials", new []{Scope.CodeAll});
            Console.WriteLine(result);
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
            ZipFile.CreateFromDirectory(bundleDir, zipPath);
            return zipPath;
        }

        private static string GeneratePreSignedURL()
        {
            //https://docs.aws.amazon.com/AmazonS3/latest/dev/ShareObjectPreSignedURLDotNetSDK.html

            var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest1);
            string urlString = "";
            try
            {
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = Environment.GetEnvironmentVariable(REVIT_AUTOMATION_BUCKET_NAME, EnvironmentVariableTarget.User),
                    Key = Environment.GetEnvironmentVariable(REVIT_AUTOMATION_KEY, EnvironmentVariableTarget.User),
                    Expires = DateTime.Now.AddMinutes(5)
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
            Console.WriteLine(response.Content);
        }
    }
}
