using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Dynamo.Extensions;

namespace Hypar.Dynamo
{
    public class Extension : IExtension
    {
        public string UniqueId => Guid.NewGuid().ToString();
        public string Name => "Hypar Loader Extension";
        public void Dispose() { }
        public void Ready(ReadyParams sp) { }
        public void Shutdown() { }
        public void Startup(StartupParams sp)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("System.ComponentModel.Annotations"))
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                try
                {
                    // Even though this method is deperecated it appears to be the only strategy that 
                    // actually results in loading an assembly from the GAC.  Perscribed method of using 
                    // simply Assembly.Load(string/AssemblyName) wouldn't work despite various attempts
                    var assembly = Assembly.LoadWithPartialName("System.ComponentModel.Annotations");

                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    return assembly;
                }
                catch (Exception ex)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    MessageBox.Show("Failed to Load", ex.Message);
                    throw ex;
                }
            }
            else
            {
                var requestedAssembly = new AssemblyName(args.Name);
                Assembly assembly = null;
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                try
                {
                    assembly = Assembly.Load(requestedAssembly.Name);
                }
                catch { }
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                return assembly;
            }
        }
    }
}
