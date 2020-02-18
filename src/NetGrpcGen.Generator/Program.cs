using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace NetGrpcGen.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(10);
            }
            Debugger.Break();
            
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();
            
            Log.Information("Running rpc generator...");

            try
            {
                using (var stdin = Console.OpenStandardInput())
                using (var stdout = Console.OpenStandardOutput())
                {
                    var registry = new ExtensionRegistry();
                    registry.Add(Extensions.ServiceObjectName);
                    registry.Add(Extensions.MethodCreate);
                    registry.Add(Extensions.MethodEventListener);
                    registry.Add(Extensions.MethodName);
                    registry.Add(Extensions.MethodSync);
                    registry.Add(Extensions.MethodPropName);
                    registry.Add(Extensions.MethodPropGet);
                    registry.Add(Extensions.MethodPropSet);
                    registry.Add(Extensions.MessageObjectName);
                    registry.Add(Extensions.MessageEventName);
                    registry.Add(Extensions.MessageForProp);
      
                    var parser = CodeGeneratorRequest.Parser.WithExtensionRegistry(registry);
                    var request = parser.ParseFrom(stdin);

                    var response = Generator.Generate(request);
                    
                    using (var output = new CodedOutputStream(stdout, true))
                    {
                        response.WriteTo(output);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Environment.Exit(1);
            }
        }
    }
}
