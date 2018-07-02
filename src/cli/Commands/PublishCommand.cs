#pragma warning disable CS0067

using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Hypar.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Hypar.API;

namespace Hypar.Commands
{
    internal class PublishCommand : IHyparCommand
    {
        private string _framework = "netcoreapp2.0";
        private string _runtime = "linux-x64";
        private HyparConfig _config;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;

            if(args[0] != "publish")
            {
                return false;
            }

            // Avoid falling through to publish.
            if(args.Length == 2 && args[1] == "help")
            {
                return true;
            }

            var path = Path.Combine(System.Environment.CurrentDirectory, Program.HYPAR_CONFIG);
            if(!File.Exists(path))
            {
                Logger.LogError("The hypar.json file could not be located in the current directory.");
                return false;
            }
            var json = File.ReadAllText(path);
            _config = HyparConfig.FromJson(json);

            return Cognito.Login();
        }

        public void Execute(object parameter)
        {
            Publish();
        }

        public void Help()
        {
            Console.WriteLine("Publish your function to Hypar.");
            Console.WriteLine("Usage: hypar publish");
        }

        private void Publish()
        {
            // Inject the logged in user's email into the config.
            var userDetails = Task.Run(()=>Cognito.User.GetUserDetailsAsync()).Result;
            foreach(var kvp in userDetails.UserAttributes)
            {
                Console.WriteLine(kvp.Name + ":" + kvp.Value);
                if(kvp.Name == "email")
                {
                    _config.Email = kvp.Name;
                    break;
                }
            }
            
            var process = new Process()
            {
                // https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-how-to-create-deployment-package.html
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    FileName="dotnet",
                    Arguments=$"publish -c Release /p:GenerateRuntimeConfigurationFiles=true -r linux-x64"
                }
            };
            process.Start();
            process.WaitForExit();

            var credentials = Task.Run(()=>Cognito.User.GetCognitoAWSCredentials(Cognito.IdentityPoolId, RegionEndpoint.USWest2)).Result;
            var functionName = $"{Cognito.User.UserID}-{_config.FunctionId}";

            var zipPath = ZipProject(functionName);
            try
            {
                CreateBucketAndUpload(credentials, functionName, zipPath);
                CreateOrUpdateLambda(credentials, functionName);
                PostFunction();
            }
            catch(Exception ex)
            {
                Logger.LogError("There was an error during publish.");
                Logger.LogError(ex.Message);
                Logger.LogError(ex.StackTrace);
            }
            finally
            {
                if(File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
            }
        }

        private void CreateOrUpdateLambda(Amazon.CognitoIdentity.CognitoAWSCredentials credentials, string functionName)
        {
            using(var client = new AmazonLambdaClient(credentials, RegionEndpoint.GetBySystemName(Program.Configuration["aws_default_region"])))
            {
                try
                {
                    // Attempt to get the existing function. If an exception
                    // is thrown, then create the function.
                    Task.Run(()=>client.GetFunctionAsync(functionName)).Wait();
                }
                catch
                {
                   Logger.LogInfo($"Creating {functionName}...");
                    var createRequest = new CreateFunctionRequest{
                        FunctionName = functionName,
                        Runtime = _config.Runtime,
                        Handler = _config.Function,
                        Role = Program.Configuration["aws_iam_role_lambda"],
                        Code = new FunctionCode{
                            S3Bucket = functionName,
                            S3Key = functionName + ".zip"
                        },
                        Description = _config.Description,
                        MemorySize = 1024,
                        Timeout = 30
                    };

                    Task.Run(()=>client.CreateFunctionAsync(createRequest)).Wait();
                }
                
                Logger.LogInfo($"Updating {functionName} function...");
                var updateRequest = new UpdateFunctionCodeRequest{
                    FunctionName = functionName,
                    S3Bucket = functionName,
                    S3Key = functionName + ".zip"
                };

                var response = Task.Run(()=>client.UpdateFunctionCodeAsync(updateRequest)).Result;

                Logger.LogSuccess($"{functionName} updated successfully.");
            }
        }

        private string ZipProject(string functionName)
        {
            //TODO: Implement windows compatible zipping - https://github.com/aws/aws-extensions-for-dotnet-cli/blob/c29333812c317b6ac41a44cf8f5ac7e3798fccc2/src/Amazon.Lambda.Tools/LambdaPackager.cs
            var publishDir = Path.Combine(System.Environment.CurrentDirectory , $"bin/Release/{_framework}/{_runtime}/publish");
            var zipPath = Path.Combine(publishDir, $"{functionName}.zip");

            if(File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            var args = $"{functionName}.zip";
            foreach(var fi in Directory.GetFiles(publishDir))
            {
                args += $" \"{Path.GetFileName(fi)}\"";
            }

            var process = new Process()
            {
                // https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-how-to-create-deployment-package.html
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName="zip",
                    WorkingDirectory = publishDir,
                    Arguments=args
                }
            };
            process.Start();
            process.WaitForExit();

            return zipPath;
        }

        private void PostFunction()
        {
            Logger.LogInfo("Updating function record...");

            var client = new RestClient(Program.Configuration["hypar_api_url"]);

            var request = new RestRequest("functions", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("x-api-key", Program.Configuration["hypar_api_key"]);
            request.AddBody(_config);

            var response = client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var functions = JsonConvert.DeserializeObject<Function>(response.Content);
            }
            else
            {
                Logger.LogError("There was an error getting the functions from hypar.");
                Logger.LogError(response.Content);
            }
            return;
        }

        private void CreateBucketAndUpload(Amazon.CognitoIdentity.CognitoAWSCredentials credentials, string functionName, string zipPath)
        {
            
            using (var client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(Program.Configuration["aws_default_region"])))
            {   
                try
                {
                    Logger.LogInfo($"Looking for existing storage for {functionName}...");
                    // Attempt to get the object metadata. If it's not found
                    // then the object doesn't exist (TODO: Find a better test than this.)
                    var response = Task.Run(()=>client.GetObjectMetadataAsync(functionName, functionName + ".zip")).Result;
                    Logger.LogInfo($"Existing storage located for {functionName}...");
                }
                catch
                {
                    Logger.LogWarning($"Existing storage for {functionName} was not found. Creating new storage...");
                    var putResponse = Task.Run(()=>client.PutBucketAsync(functionName)).Result;
                    if(putResponse.HttpStatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception("There was an error creating the function storage.");
                    }
                }

                Logger.LogInfo($"Uploading {functionName}...");
                var fileTransferUtility = new TransferUtility(client);
                
                Task.Run(()=>fileTransferUtility.UploadAsync(zipPath, functionName)).Wait();
                Logger.LogSuccess($"Upload of {functionName} complete.");
            }
        }
    }
}