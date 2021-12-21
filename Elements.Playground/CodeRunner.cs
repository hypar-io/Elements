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
        public static string Output = "<p class=\"message\">Nothing to see yet. Try adding and connecting nodes.</p>";
        public static string Code = "// No code available yet. Try adding and connecting nodes.";
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
            OnCompilationComplete();
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
        public static async Task Compile()
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
                    Output += $"<p class=\"success\">Compilation successful in {sw.ElapsedMilliseconds} ms</p>";
                }
                Output += $"<p class=\"message\">{writer.ToString()}</p>";
                OnCompilationComplete();
            }
            catch (Exception ex)
            {
                Console.Write(exception);
                exception = ex;
                Output += $"<p class=\"error\">{ex.ToString()}</p>";
            }
            finally
            {
                Console.SetOut(currentOut);
            }
            sw.Stop();
        }

        static async Task RunInternal()
        {


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
                    Output = string.Empty;
                    var glb = ((Elements.Model)model).ToGlTF();
                    Output += $"<p class=\"success\">Execution: {sw.ElapsedMilliseconds} ms</p>";
                    sw.Reset();
                    sw.Start();
                    Runtime.InvokeUnmarshalled<byte[], bool>("model.loadModel", glb);
                    Output += $"<p class=\"success\">Drawing: {sw.ElapsedMilliseconds} ms</p>";
                });

                Output += $"<p class=\"message\">{writer.ToString()}</p>";
                OnExecutionComplete();
            }
            catch (Exception ex)
            {
                Output += $"<p class=\"error\">{ex.ToString()}</p>";
            }
            finally
            {
                Console.SetOut(currentOut);
            }
            sw.Stop();
        }
    }
}