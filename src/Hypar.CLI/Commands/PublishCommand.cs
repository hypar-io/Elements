#pragma warning disable CS0067

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Hypar.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

            var path = Path.Combine(System.Environment.CurrentDirectory, Constants.HYPAR_CONFIG);
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
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    FileName="dotnet",
                    Arguments=$"publish -c Release"
                }
            };
            process.Start();
            process.WaitForExit();

            var publishDir = Path.Combine(System.Environment.CurrentDirectory , "bin/Release/netstandard2.0/publish");
            var zipPath = Path.Combine(System.Environment.CurrentDirectory, $"{_config.FunctionId}.zip");
            ZipFile.CreateFromDirectory(publishDir, zipPath);

            // CreateS3Bucket(config.FunctionId);
            // SendProjectToS3();
            // PostFunction();
        }

        private void BuildProject()
        {

        }

        private void ZipProject()
        {

        }

        private void SendProjectToS3()
        {

        }

        private void PostFunction()
        {
            // Read the hypar.json

            // Get the authenticated user's email address.

            // Build the request body

            // Direct user to email confirmation for link.
        }

        private void CreateS3Bucket(string name)
        {
            Console.WriteLine($"Creating the {name} bucket...");
            var credentials = Task.Run(()=>Cognito.User.GetCognitoAWSCredentials(Cognito.IdentityPoolId, RegionEndpoint.USWest2)).Result;

            using (var client = new AmazonS3Client(credentials, RegionEndpoint.USWest1))
            {
                var response = Task.Run(()=>client.PutBucketAsync(name)).Result;
            }
        }
    }
}