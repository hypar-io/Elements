using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using RestSharp;

namespace Hypar
{
    class Program
    {
        private static string _input;
        private static string _hyparApiKey = "xBZJyh85lq9IZnMUx2gKaRZMz8XPmRY6DCmpN8Y3";

        private static RestClient _client;

        static int Main(string[] args)
        {
            _client = new RestClient("https://api.hypar.io/dev");

            if(args.Length == 0)
            {
                ShowHelp(args);
                return 0;
            }

            if(Console.IsInputRedirected)
            {
                _input = Console.In.ReadToEnd();
            }

            try
            {
                ParseCommandLineArgs(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
            
            return 0;
        }

        static void ShowHelp(string[] args)
        {
            Console.WriteLine(@"Hypar Command Line Usage:

hypar COMMAND OPTIONS

Available Commands:
    execute <function_id> <limit>   Executes the function identified by <function_id>, a maximum of <limit> times. If stdin contains a valid set of arguments for the function, then those will be used and <limit> will be ignored.
    executions <function_id>        Writes all executions for the specified <function_id> to stdout.
    functions                       Writes all functions available in Hypar to stdout.
    help                            Shows this help.
    model <output directory>        Write the models generated for the input executions to <output>.
    results                         Write the results of a set of executions from stdin to stdout.
    version                         Returns the current version of hypar.");
        }

        static void ParseCommandLineArgs(string[] args)
        {
            switch(args[0])
            {
                case "execute":
                    if(args.Length == 3)
                    {
                        Execute(args[1], int.Parse(args[2]));
                    }
                    else if(args.Length == 2)
                    {
                        Execute(args[1]);
                    }
                    break;
                case "executions":
                    if(args.Length < 2)
                    {
                        throw new Exception("You must supply a function_id.");
                    }
                    Executions(args[1]);
                    break;
                case "new":
                    if(args.Length < 2)
                    {
                        throw new Exception("You must supply a name for the new hypar function.");
                    }
                    New(args[1]);
                    break;
                case "help":
                    ShowHelp(args);
                    break;
                case "functions":
                    Functions();
                    break;
                case "model":
                    if(_input == null && args.Length != 2)
                    {
                        throw new Exception("You must supply either an Execution or an execution id.");
                    }
                    Model(args[1]);
                    break;
                case "results":
                    Results();
                    break;
                case "version":
                    Version();
                    break;
                default:
                    throw new Exception($"The provided argument, {args[0]}, was not recognized.");
            }
            return;
        }

        static void Execute(string functionId, int? limit = null)
        {
            var request = new RestRequest("executions", Method.POST);
            request.AddHeader("x-api-key", _hyparApiKey);
            request.RequestFormat = DataFormat.Json;

            var body = new Dictionary<string,object>();
            body.Add("function_id", functionId);
            body.Add("max_executions", limit!=null?limit:1);
            if(_input != null)
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string,object>>(_input);
                body.Add("args", args);
            }
            request.AddBody(body);
            var response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var executions = JsonConvert.DeserializeObject<List<Execution>>(response.Content);
                Console.WriteLine(JsonConvert.SerializeObject(executions, Formatting.Indented));
            }
            else
            {
                throw new Exception($"There was an error executing the function, {functionId}, on Hypar.");
            }
            return;
        }

        static void Executions(string functionId)
        {
            var request = new RestRequest("executions", Method.GET);
            request.AddHeader("x-api-key", _hyparApiKey);
            request.AddParameter("function_id",functionId);
            var response = _client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var executions = JsonConvert.DeserializeObject<List<Execution>>(response.Content);
                Console.WriteLine(JsonConvert.SerializeObject(executions, Formatting.Indented));
            }
            else
            {
                throw new Exception($"There was an error executing the function, {functionId}, on Hypar.");
            }
            return;
        }

        static void Functions()
        {
            var request = new RestRequest("functions", Method.GET);
            request.AddHeader("x-api-key", _hyparApiKey);
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
                throw new Exception("There was an error getting the functions from hypar.");
            }
            return;
        }

        static void Model(string outputDirectory)
        {
            var outDir = outputDirectory;

            if(!Path.IsPathRooted(outDir)) 
            {
                outDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), outDir);
            }

            if(!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            var executions = JsonConvert.DeserializeObject<List<Execution>>(_input);
            foreach(var e in executions)
            {
                var client = new RestClient("https://s3-us-west-1.amazonaws.com");
                var request = new RestRequest($"hypar-executions-dev/{e.Id}_model.zip", Method.GET);
                var response = client.DownloadData(request);
                var output = Path.Combine(outDir, $"{e.Id}.zip");
                File.WriteAllBytes(output, response);
            }
            return;
        }

        static void New(string name)
        {
            // Create a directory here.
            var newDir = Path.Combine(Directory.GetCurrentDirectory(), name);
            CreateProject(name);       
            CreateHyparReference(name);
            CreateHyparJson(newDir, name);
            return;
        }

        private static void CreateHyparJson(string dir, string name)
        {
            var hyparPath = Path.Combine(dir, "hypar.json");
            
            var contents = $@"{{
    ""function"":""{name}::{name}.{name}"",
    ""runtime"":""netstandard2.0"",
    ""parameters"": {{}},
    ""returns"": {{}}
}}";
            File.WriteAllText(hyparPath, contents);
        }
        
        private static void CreateProject(string name)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    FileName="dotnet",
                    Arguments=$"new classlib -n {name}"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private static void CreateHyparReference(string name)
        {
            var project = $"./{name}/{name}.csproj";
            Console.WriteLine(project);
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    FileName="dotnet",
                    Arguments=$"add {project} package Hypar -v 0.0.1-beta1"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        static void Results()
        {
            var results = new Dictionary<string,List<object>>();

            var executions = JsonConvert.DeserializeObject<List<Execution>>(_input);
            if(executions != null)
            {
                foreach(var e in executions)
                {
                    foreach(var kvp in e.Computed)
                    {
                        if(results.ContainsKey(kvp.Key))
                        {
                            results[kvp.Key].Add(kvp.Value);
                        }
                        else
                        {
                            results.Add(kvp.Key, new List<object>{kvp.Value});
                        }
                    }
                }
                Console.WriteLine(string.Join(",",results.Keys));


                var length = results.Values.ElementAt(0).Count;
                for(var i=0; i<length; i++)
                {
                    var line = new List<object>();
                    foreach(var kvp in results)
                    {
                        line.Add(kvp.Value.ElementAt(i));
                    }
                    Console.WriteLine(string.Join(",", line));
                }
            }
            else
            {
                throw new Exception("The input could be deserialized to an array of executions.");
            }
            return;
        }

        static void Version()
        {
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            return;
        }
    }
}
