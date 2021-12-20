using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.JSInterop;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.IO;
using Elements.Serialization.glTF;

namespace Elements.Playground
{
    public static class CodeRunner
    {
        public static string Output = "This is a test";
        public static string Code = string.Empty;
        public static IJSUnmarshalledRuntime Runtime;
        private static Dictionary<string, double> context;
        private static Assembly asm;
        private static Compilation compilation;

        public static event Action ExecutionComplete;
        public static void OnExecutionComplete()
        {
            if (ExecutionComplete != null)
            {
                ExecutionComplete();
            }
        }

        public static event Action CompilationComplete;
        public static void OnCompilationComplete()
        {
            if (CompilationComplete != null)
            {
                CompilationComplete();
            }
        }

        [JSInvokable]
        public static void SetCodeValue(string code)
        {
            Console.WriteLine($"Setting code value to \n {code}");
            Code = code;
        }

        [JSInvokable]
        public static void SetCodeContext(Dictionary<string, double> value)
        {
            context = value;
        }

        [JSInvokable]
        public static Task Run()
        {
            return Compiler.WhenReady(RunInternal);
        }

        [JSInvokable]
        public static void Compile()
        {
            Output = string.Empty;

            var currentOut = Console.Out;
            var writer = new StringWriter();
            Console.SetOut(writer);
            var sw = Stopwatch.StartNew();
            Exception exception = null;
            try
            {
                asm = null;
                compilation = null;
                var (success, newAsm, newCompilation) = Compiler.LoadSource(Code);
                if (success)
                {
                    asm = newAsm;
                    compilation = newCompilation;
                    Console.WriteLine($"\r\nCompilation successful in {sw.ElapsedMilliseconds} ms");
                }
                Output += writer.ToString();
                OnCompilationComplete();
            }
            catch (Exception ex)
            {
                exception = ex;
                Output += "\r\n" + exception.ToString();
            }
            finally
            {
                Console.SetOut(currentOut);
            }
            sw.Stop();
        }

        static async Task RunInternal()
        {
            Output = string.Empty;

            var globals = new Globals(context);

            var currentOut = Console.Out;
            var writer = new StringWriter();
            Console.SetOut(writer);
            var sw = Stopwatch.StartNew();
            try
            {
                var entryPoint = compilation.GetEntryPoint(CancellationToken.None);
                var type = asm.GetType($"{entryPoint.ContainingNamespace.MetadataName}.{entryPoint.ContainingType.MetadataName}");
                var entryPointMethod = type.GetMethod(entryPoint.MetadataName);

                var submission = (Func<object[], Task>)entryPointMethod.CreateDelegate(typeof(Func<object[], Task>));
                var model = await (Task<object>)submission(new object[] { globals, null });

                await Task.Run(() =>
                {
                    var glb = ((Elements.Model)model).ToGlTF();
                    Console.WriteLine($"\r\n Exceution completed in {sw.ElapsedMilliseconds} ms");
                    Runtime.InvokeUnmarshalled<byte[], bool>("model.loadModel", glb);
                });

                Output += writer.ToString();
                OnExecutionComplete();
            }
            catch (Exception ex)
            {
                Output += "\r\n" + ex.ToString();
            }
            finally
            {
                Console.SetOut(currentOut);
            }
            sw.Stop();
        }
    }
}