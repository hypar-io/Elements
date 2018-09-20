using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Hypar.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;
using Hypar.Commands;

namespace Hypar
{
    class Program
    {
        public const string HYPAR_CONFIG = "hypar.json";
        public static IConfiguration Configuration { get; set; }

        static int Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                            .AddJsonFile("appsettings.json");
            
            Configuration = builder.Build();

            var commands = new List<IHyparCommand>();

            var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t=>typeof(IHyparCommand).IsAssignableFrom(t) && typeof(IHyparCommand) != t);
            foreach(var t in commandTypes)
            {
                var instance = (IHyparCommand)Activator.CreateInstance(t);
                commands.Add(instance);
            }

            if(args.Length == 0)
            {
                var help = new HelpCommand();
                help.Execute(args);
                return 0;
            }

            var commandName = args[0];
            var commandArgs = args.Skip(1).ToArray();
            var command = commands.FirstOrDefault(c=>c.Name == args[0]);
            
            if(command == null)
            {
                Logger.LogError($"The {commandName} command was not recognized. Try 'hypar help'.");
                return 1;
            }

            if(commandArgs.Length > 0 && commandArgs[0] == "help")
            {
                Logger.LogInfo(command.Description);
                Logger.LogInfo($"Usage: hypar {command.Name} {string.Join(" ", command.Arguments)}");
                return 0;
            }

            Console.WriteLine(string.Join(",", commandArgs));
            if(command.Arguments.Length > 0 && commandArgs != command.Arguments)
            {
                Logger.LogError($"Usage: hypar {command.Name} {string.Join(" ", command.Arguments)}");
                return 1;
            }

            if(command.CanExecute(commandArgs))
            {
                command.Execute(commandArgs);
                return 0;
            }

            return 0;
        } 
    }
}
