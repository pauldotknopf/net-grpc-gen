using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetGrpcGen.Model;

namespace NetGrpcGen.CodeGen
{
    public class ProtofileCodeGen
    {
        private List<string> GetAllImports(GrpcObject grpcObject)
        {
            var imports = new List<string>();

            foreach (var method in grpcObject.Methods)
            {
                imports.Add(method.ResponseType.Import);
                imports.Add(method.RequestType.Import);
            }
            
            foreach (var prop in grpcObject.Properties)
            {
                imports.Add(prop.DataType.Import);
            }

            foreach (var even in grpcObject.Events)
            {
                if (even.DataType != null)
                {
                    imports.Add(even.DataType.Import);
                }
            }

            return imports.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        }
        
        public void Generate(List<GrpcObject> objects, string packageName, Stream outputStream)
        {
            using (var writer = new StreamWriter(outputStream))
            {
                writer.WriteLine("syntax = \"proto3\";");
                if (!string.IsNullOrEmpty(packageName))
                {
                    writer.WriteLine($"package {packageName};");
                }

                var imports = objects.SelectMany(GetAllImports).Distinct().ToList();
                if (!imports.Contains("google/protobuf/any.proto"))
                {
                    imports.Add("google/protobuf/any.proto");
                }
                foreach (var import in imports.OrderBy(x => x))
                {
                    writer.WriteLine($"import \"{import}\";");
                }
                
                foreach (var o in objects)
                {
                    var serviceName = $"{o.Name}ObjectService";

                    if (o.Events.Count > 0)
                    {
                        writer.WriteLine($"message {o.Name}ListenEventStream {{");
                        writer.WriteLine("\tuint64 objectId = 1;");
                        writer.WriteLine("}");
                    }
                    
                    writer.WriteLine($"message {o.Name}CreateResponse {{");
                    writer.WriteLine("\tuint64 objectId = 1;");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}StopRequest {{");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}StopResponse {{");
                    writer.WriteLine("}");

                    foreach (var property in o.Properties)
                    {
                        if (property.CanWrite)
                        {
                            writer.WriteLine($"message {o.Name}{property.Name}SetRequest {{");
                            writer.WriteLine("\tuint64 objectId = 1;");
                            writer.WriteLine($"\t{property.DataType.TypeName} value = 2;");
                            writer.WriteLine("}");
                            
                            writer.WriteLine($"message {o.Name}{property.Name}SetResponse {{");
                            writer.WriteLine("}");
                        }
                        writer.WriteLine($"message {o.Name}{property.Name}GetRequest {{");
                        writer.WriteLine("\tuint64 objectId = 1;");
                        writer.WriteLine("}");
                        
                        writer.WriteLine($"message {o.Name}{property.Name}GetResponse {{");
                        writer.WriteLine($"\t{property.DataType.TypeName} value = 1;");
                        writer.WriteLine("}");
                        
                        if (o.ImplementedINotify)
                        {
                            writer.WriteLine($"message {o.Name}{property.Name}PropertyChanged {{");
                            writer.WriteLine("\tuint64 objectId = 1;");
                            writer.WriteLine($"\t{property.DataType.TypeName} value = 2;");
                            writer.WriteLine("}");
                        }
                    }

                    foreach (var even in o.Events)
                    {
                        writer.WriteLine($"message {o.Name}{even.Name}Event {{");
                        writer.WriteLine("\tuint64 objectId = 1;");
                        if (even.DataType != null)
                        {
                            writer.WriteLine($"\t{even.DataType.TypeName} value = 2;");
                        }
                        writer.WriteLine("}");
                    }
                    
                    writer.WriteLine($"service {serviceName} {{");
                    writer.WriteLine("\trpc Create (stream google.protobuf.Any) returns (stream google.protobuf.Any);");
                    if (o.Events.Count > 0)
                    {
                        writer.WriteLine($"\trpc ListenEvents ({o.Name}ListenEventStream) returns (stream google.protobuf.Any);");
                    }
                    foreach (var method in o.Methods)
                    {
                        writer.WriteLine($"\trpc {method.Name} ({method.RequestType.TypeName}) returns ({method.ResponseType.TypeName});");
                    }
                    foreach (var property in o.Properties)
                    {
                        if (property.CanWrite)
                        {
                            writer.WriteLine($"\trpc SetProperty{property.Name} ({o.Name}{property.Name}SetRequest) returns ({o.Name}{property.Name}SetResponse);");
                        }
                        writer.WriteLine($"\trpc GetProperty{property.Name} ({o.Name}{property.Name}GetRequest) returns ({o.Name}{property.Name}GetResponse);");
                    }
                    writer.WriteLine("}");
                }
            }
        }

        private string ToGrpcType(GrpcDataType dataType)
        {
            switch (dataType)
            {
                case GrpcDataType.Complex:
                    return "string";
                case GrpcDataType.String:
                    return "string";
                case GrpcDataType.Int32:
                    return "sint32";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}