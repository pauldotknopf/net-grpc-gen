using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Newtonsoft.Json;

namespace NetGrpcGen.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(1000);
                }
                Debugger.Break();

                var t = "/home/pknopf/test.json";
                if (File.Exists(t))
                {
                    File.Delete(t);
                }
                
                using (var stdin = Console.OpenStandardInput())
                using (var stdout = Console.OpenStandardOutput())
                {
                    var request = CodeGeneratorRequest.Parser.ParseFrom(stdin);

                    Console.WriteLine(request.Parameter);
                    
                    foreach (var file in request.ProtoFile)
                    {
                        foreach (var message in file.MessageType)
                        {
                            foreach (var field in message.Field)
                            {
                                Console.WriteLine(field.Name);s
                            }
                        }
                    }
                    
                    File.WriteAllText(t, JsonConvert.SerializeObject(request, Formatting.Indented));
                    
                    var response = new CodeGeneratorResponse();
                    response.File.Add(new CodeGeneratorResponse.Types.File
                    {
                        Content = "SDFSDF",
                        Name = "Name.cs"
                    });
                    response.WriteTo(new CodedOutputStream(stdout, true));
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
