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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Hypar.API;
using System.Text;

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
                if(kvp.Name == "email")
                {
                    _config.Email = kvp.Value;
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

            var credentials = Task.Run(()=>Cognito.User.GetCognitoAWSCredentials(Program.Configuration["cognito_identity_pool_id"], RegionEndpoint.USWest2)).Result;
            var functionName = $"{Cognito.User.UserID}-{_config.FunctionId}";
            
            // Logger.LogInfo($"Account id: {credentials.AccountId}");
            // Logger.LogInfo($"Identity pool id: {credentials.IdentityPoolId}");
            // Logger.LogInfo($"Auth role arn: {credentials.AuthRoleArn}");

            var zipPath = ZipProject(functionName);
            Logger.LogInfo($"Created archive {zipPath}.");

            try
            {
                PostFunction();
                CreateBucketAndUpload(credentials, functionName, zipPath);
                CreateOrUpdateLambda(credentials, functionName);
            }
            catch(Exception ex)
            {
                Logger.LogError($"There was an error during publish: {ex.Message}");
                // Logger.LogError(ex.StackTrace);
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
                            S3Bucket = Program.Configuration["s3_functions_bucket"],
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
                    S3Bucket = Program.Configuration["s3_functions_bucket"],
                    S3Key = functionName + ".zip"
                };

                var response = Task.Run(()=>client.UpdateFunctionCodeAsync(updateRequest)).Result;

                Logger.LogSuccess($"{functionName} updated successfully.");
            }
        }

        private string ZipProject(string functionName)
        {
            // https://github.com/aws/aws-extensions-for-dotnet-cli/blob/c29333812c317b6ac41a44cf8f5ac7e3798fccc2/src/Amazon.Lambda.Tools/LambdaPackager.cs
            var publishDir = Path.Combine(System.Environment.CurrentDirectory , $"bin/Release/{_framework}/{_runtime}/publish");
            var zipPath = Path.Combine(publishDir, $"{functionName}.zip");

            if(File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ZipFile.CreateFromDirectory(publishDir, zipPath);
            }
            else
            {
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
            }

            return zipPath;
        }

        private void PostFunction()
        {
            Logger.LogInfo("Updating function record...");

            var client = new RestClient(Program.Configuration["hypar_api_url"]);

            // Find a function record
            var getRequest = new RestRequest($"functions/{_config.FunctionId}", Method.GET);
            getRequest.AddHeader("x-api-key", Program.Configuration["hypar_api_key"]);
            var getResponse = client.Execute(getRequest);
            if(getResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogInfo("The function does not exist. Adding a function record...");
                // POST
                var request = new RestRequest("functions", Method.POST);
                request.AddHeader("x-api-key", Program.Configuration["hypar_api_key"]);

                // Send raw json so we can use the json.net serializer.
                request.AddParameter("application/json", JsonConvert.SerializeObject(_config), ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;
                var postResponse = client.Execute(request);
                if(postResponse.StatusCode == HttpStatusCode.OK)
                {
                    var functions = JsonConvert.DeserializeObject<Function>(postResponse.Content);
                    Logger.LogSuccess($"{_config.FunctionId} version {_config.Version} was added successfully.");
                }
                else
                {
                    throw new Exception($"There was an error adding the function record on Hypar: {postResponse.Content}");
                }
            }
            else if(getResponse.StatusCode == HttpStatusCode.OK)
            {
                Logger.LogInfo("The function already exists. Updating the function record...");

                // PUT
                var request = new RestRequest($"functions/{_config.FunctionId}", Method.PUT);
                request.AddHeader("x-api-key", Program.Configuration["hypar_api_key"]);

                // Send raw json so we can use the json.net serializer.
                request.AddParameter("application/json", JsonConvert.SerializeObject(_config), ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;
                var putResponse = client.Execute(request);
                if(putResponse.StatusCode == HttpStatusCode.OK)
                {
                    var functions = JsonConvert.DeserializeObject<Function>(putResponse.Content);
                    Logger.LogSuccess($"{_config.FunctionId} version {_config.Version} was updated successfully.");
                }
                else
                {
                    throw new Exception($"There was an error updating the function record on Hypar: {putResponse.Content}");
                }
            }
            else
            {
                throw new Exception($"There was an error creating or updating the function record: {getResponse.Content}");
            }

            return;
        }

        private void CreateBucketAndUpload(Amazon.CognitoIdentity.CognitoAWSCredentials credentials, string functionName, string zipPath)
        {

            if(credentials == null)
            {
                throw new Exception("The credentials were invalid.");
            }
            
            var endPoint = RegionEndpoint.GetBySystemName(Program.Configuration["aws_default_region"]);
            // Logger.LogInfo($"Storing the function in {endPoint.DisplayName}...");
            
            using (var client = new AmazonS3Client(credentials, endPoint))
            {   
                Logger.LogInfo($"Uploading {functionName}...");
                var fileTransferUtility = new TransferUtility(client);
                
                var bucket = Program.Configuration["s3_functions_bucket"];
                Task.Run(()=>fileTransferUtility.UploadAsync(zipPath, bucket)).Wait();
                Logger.LogSuccess($"Upload of {functionName} complete.");
            }
        }
    }
}