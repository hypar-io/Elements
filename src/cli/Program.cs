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
            
            foreach(var c in commands)
            {
                if(c.CanExecute(args))
                {
                    if(args.Length > 1 && args[1] == "help")
                    {
                        c.Help();
                        return 0;
                    }
                    else
                    {
                        c.Execute(args);
                        return 0;
                    }
                }
            }

            Console.WriteLine($"The {args[0]} command was not recognized. Try 'hypar help'.");

            return 0;
        } 
    }
}
