using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using NetGrpcGen.ProtoModel.Impl;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace NetGrpcGen.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();
            
            Log.Information("Running rpc generator...");

            try
            {
                using (var stdin = Console.OpenStandardInput())
                using (var stdout = Console.OpenStandardOutput())
                {
                    var request = CodeGeneratorRequest.Parser.ParseFrom(stdin);

                    var generator = new Generator(new ProtoModelBuilder());
                    var response = generator.Generate(request);
                    
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
