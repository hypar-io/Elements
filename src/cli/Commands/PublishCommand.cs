#pragma warning disable CS0067

using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Hypar.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace Hypar.Commands
{
    internal class PublishCommand : IHyparCommand
    {
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
                Console.WriteLine("The hypar.json file could not be located in the current directory.");
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
            var process = new Process()
            {
                // https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-how-to-create-deployment-package.html
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    FileName="dotnet",
                    Arguments=$"publish -c Release /p:GenerateRuntimeConfigurationFiles=true"
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
                Console.WriteLine("There was an error during publish.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
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
                catch(Exception getFuncEx)
                {
                    Console.WriteLine(getFuncEx.Message);

                    Console.WriteLine("Creating a new function...");
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
                
                Console.WriteLine("Updating an existing function...");
                var updateRequest = new UpdateFunctionCodeRequest{
                    FunctionName = functionName,
                    S3Bucket = functionName,
                    S3Key = functionName + ".zip"
                };

                Task.Run(()=>client.UpdateFunctionCodeAsync(updateRequest)).Wait();
            }
        }

        private string ZipProject(string functionName)
        {
            var publishDir = Path.Combine(System.Environment.CurrentDirectory , "bin/Release/netstandard2.0/publish");
            var zipPath = Path.Combine(System.Environment.CurrentDirectory, $"{functionName}.zip");
            ZipFile.CreateFromDirectory(publishDir, zipPath);
            return zipPath;
        }

        private void PostFunction()
        {
            // Read the hypar.json

            // Get the authenticated user's email address.

            // Build the request body

            // Direct user to email confirmation for link.
        }

        private void CreateBucketAndUpload(Amazon.CognitoIdentity.CognitoAWSCredentials credentials, string functionName, string zipPath)
        {
            Console.WriteLine($"Creating storage for function...");
            
            using (var client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(Program.Configuration["aws_default_region"])))
            {   
                try
                {
                    // Attempt to get the object metadata. If it's not found
                    // then the object doesn't exist (TODO: Find a better test than this.)
                    var response = Task.Run(()=>client.GetObjectMetadataAsync(functionName, functionName + ".zip")).Result;
                }
                catch
                {
                    Console.WriteLine("Existing storage for the function was not found. Creating new storage...");
                    var putResponse = Task.Run(()=>client.PutBucketAsync(functionName)).Result;
                    if(putResponse.HttpStatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception("There was an error creating the function storage.");
                    }
                }

                Console.WriteLine("Uploading the function contents...");
                var fileTransferUtility = new TransferUtility(client);
                
                Task.Run(()=>fileTransferUtility.UploadAsync(zipPath, functionName)).Wait();
                Console.WriteLine("Upload of function contents complete!");
            }
        }
    }
}