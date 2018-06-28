using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Hypar.Configuration;
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
        static int Main(string[] args)
        {
            var commands = new List<IHyparCommand>();

            var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t=>typeof(IHyparCommand).IsAssignableFrom(t) && typeof(IHyparCommand) != t);
            foreach(var t in commandTypes)
            {
                var instance = (IHyparCommand)Activator.CreateInstance(t);
                commands.Add(instance);
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

            Console.WriteLine($"The specified command, '{args[0]}', was not recognized. Try 'hypar help'.");

            return 0;
        } 
    }
}
