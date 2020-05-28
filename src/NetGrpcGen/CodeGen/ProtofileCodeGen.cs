using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetGrpcGen.Infra;
using NetGrpcGen.Model;

namespace NetGrpcGen.CodeGen
{
    public static class ProtofileCodeGen
    {
        private static List<string> GetAllImports(GrpcObject grpcObject)
        {
            var imports = new List<string>();

            foreach (var method in grpcObject.Methods)
            {
                if (method.ResponseType != null)
                {
                    imports.Add(method.ResponseType.Import);
                }
                if (method.RequestType != null)
                {
                    imports.Add(method.RequestType.Import);
                }
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

        public static string Generate(List<GrpcObject> objects, string package)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
                {
                    Generate(objects, package, writer);
                }

                memoryStream.Position = 0;
                using (var reader = new StreamReader(memoryStream, Encoding.UTF8, true, 1024, true))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        
        public static void Generate(List<GrpcObject> objects, string packageName, StreamWriter writer)
        {
            var codeWriter = new CodeWriter(writer);
            codeWriter.WriteLine("syntax = \"proto3\";");
            if (!string.IsNullOrEmpty(packageName))
            {
                codeWriter.WriteLine($"package {packageName};");
            }

            var imports = objects.SelectMany(GetAllImports).Distinct().ToList();
            if (!imports.Contains("google/protobuf/any.proto"))
            {
                imports.Add("google/protobuf/any.proto");
            }
            if(!imports.Contains("google/protobuf/descriptor.proto"))
            {
                imports.Add("google/protobuf/descriptor.proto");
            }
    
            foreach (var import in imports.OrderBy(x => x))
            {
                codeWriter.WriteLine($"import \"{import}\";");
            }

            codeWriter.WriteLine(@"extend google.protobuf.MethodOptions {
    // The method used to create the object.
    bool create = 1000;
    // The method used to listen for events.
    bool eventListener = 1001;
    // The method name
    string methodName = 1002;
    // Is this method sync or async?
    bool sync = 1003;
    // The name of the property.
    string propName = 1004;
    // The property setter.
    bool propSet = 1005;
    // The property getter.
    bool propGet = 1006;
}");
            
            codeWriter.WriteLine(@"extend google.protobuf.MessageOptions {
    // The name of the object that this message is for.
    string messageObjectName = 1000;
    // The name of the event this message is for.
    string eventName = 1001;
    // This event represents a ""property changed"" event for the given property.
    string forProp = 1002;
}");
            
            codeWriter.WriteLine(@"extend google.protobuf.ServiceOptions {
    // The name of the object that this service is for.
    string serviceObjectName = 1000;
}");
            
            foreach (var o in objects)
            {
                var serviceName = $"{o.Name}ObjectService";

                if (o.Events.Count > 0 || o.Properties.Count > 0)
                {
                    codeWriter.WriteLine($"message {o.Name}ListenEventStream {{");
                    using (codeWriter.Indent())
                    {
                        codeWriter.WriteLine("uint64 objectId = 1;");
                    }
                    codeWriter.WriteLine("}");
                }
                
                codeWriter.WriteLine($"message {o.Name}CreateRequest {{");
                codeWriter.WriteLine("}");
                
                codeWriter.WriteLine($"message {o.Name}CreateResponse {{");
                using (codeWriter.Indent())
                {
                    codeWriter.WriteLine($"option(messageObjectName) = \"{o.Name}\";");
                    codeWriter.WriteLine("uint64 objectId = 1;");
                }
                codeWriter.WriteLine("}");

                foreach (var property in o.Properties)
                {
                    if (property.CanWrite)
                    {
                        codeWriter.WriteLine($"message {o.Name}{property.Name}SetRequest {{");
                        using (codeWriter.Indent())
                        {
                            codeWriter.WriteLine("uint64 objectId = 1;");
                            codeWriter.WriteLine($"{property.DataType.TypeName} value = 2;");
                        }
                        codeWriter.WriteLine("}");
                        
                        codeWriter.WriteLine($"message {o.Name}{property.Name}SetResponse {{");
                        codeWriter.WriteLine("}");
                    }
                    codeWriter.WriteLine($"message {o.Name}{property.Name}GetRequest {{");
                    using (codeWriter.Indent())
                    {
                        codeWriter.WriteLine("uint64 objectId = 1;");
                    }
                    codeWriter.WriteLine("}");
                    
                    codeWriter.WriteLine($"message {o.Name}{property.Name}GetResponse {{");
                    using (codeWriter.Indent())
                    {
                        codeWriter.WriteLine($"{property.DataType.TypeName} value = 1;");
                    }
                    codeWriter.WriteLine("}");
                    
                    if (o.ImplementedINotify)
                    {
                        codeWriter.WriteLine($"message {o.Name}{property.Name}PropertyChanged {{");
                        using (codeWriter.Indent())
                        {
                            codeWriter.WriteLine($"option(messageObjectName) = \"{o.Name}\";");
                            codeWriter.WriteLine($"option(eventName) = \"{property.Name}\";");
                            codeWriter.WriteLine("uint64 objectId = 1;");
                            codeWriter.WriteLine($"{property.DataType.TypeName} value = 2;");
                        }
                        codeWriter.WriteLine("}");
                    }
                }

                foreach (var even in o.Events)
                {
                    codeWriter.WriteLine($"message {o.Name}{even.Name}Event {{");
                    using (codeWriter.Indent())
                    {
                        codeWriter.WriteLine($"option(messageObjectName) = \"{o.Name}\";");
                        codeWriter.WriteLine($"option(eventName) = \"{even.Name}\";");
                        codeWriter.WriteLine("uint64 objectId = 1;");
                        if (even.DataType != null)
                        {
                            codeWriter.WriteLine($"{even.DataType.TypeName} value = 2;");
                        }
                    }
                    codeWriter.WriteLine("}");
                }

                foreach (var method in o.Methods)
                {
                    codeWriter.WriteLine($"message {o.Name}{method.Name}MethodRequest {{");
                    using (codeWriter.Indent())
                    {
                        codeWriter.WriteLine("uint64 objectId = 1;");
                        if (method.RequestType != null)
                        {
                            codeWriter.WriteLine($"{method.RequestType.TypeName} value = 2;");
                        }
                    }
                    codeWriter.WriteLine("}");
                    codeWriter.WriteLine($"message {o.Name}{method.Name}MethodResponse {{");
                    using (codeWriter.Indent())
                    {
                        if (method.ResponseType != null)
                        {
                            codeWriter.WriteLine($"{method.ResponseType.TypeName} value = 1;");
                        }
                    }
                    codeWriter.WriteLine("}");
                }
                
                codeWriter.WriteLine($"service {serviceName} {{");
                using (codeWriter.Indent())
                {
                    codeWriter.WriteLine($"option(serviceObjectName) = \"{o.Name}\";");
                    codeWriter.WriteLine($"rpc Create ({o.Name}CreateRequest) returns (stream {o.Name}CreateResponse) {{");
                    codeWriter.WriteLineIndented("option(create) = true;");
                    codeWriter.WriteLine("}");
                    if (o.Events.Count > 0 || o.Properties.Count > 0)
                    {
                        codeWriter.WriteLine($"rpc ListenEvents ({o.Name}ListenEventStream) returns (stream google.protobuf.Any) {{");
                        codeWriter.WriteLineIndented("option(eventListener) = true;");
                        codeWriter.WriteLine("};");
                    }
                    foreach (var method in o.Methods)
                    {
                        codeWriter.WriteLine($"rpc Invoke{method.Name} ({o.Name}{method.Name}MethodRequest) returns ({o.Name}{method.Name}MethodResponse) {{");
                        codeWriter.WriteLineIndented($"option(methodName) = \"{method.Name}\";");
                        if (!method.IsAsync)
                        {
                            codeWriter.WriteLineIndented("option(sync) = true;");
                        }
                        codeWriter.WriteLine("};");
                    }
                    foreach (var property in o.Properties)
                    {
                        if (property.CanWrite)
                        {
                            codeWriter.WriteLine($"rpc SetProperty{property.Name} ({o.Name}{property.Name}SetRequest) returns ({o.Name}{property.Name}SetResponse) {{");
                            codeWriter.WriteLineIndented($"option(propName) = \"{property.Name}\";");
                            codeWriter.WriteLineIndented("option(propSet) = true;");
                            codeWriter.WriteLine("};");
                        }
                        codeWriter.WriteLine($"rpc GetProperty{property.Name} ({o.Name}{property.Name}GetRequest) returns ({o.Name}{property.Name}GetResponse) {{");
                        codeWriter.WriteLineIndented($"option(propName) = \"{property.Name}\";");
                        codeWriter.WriteLineIndented("option(propGet) = true;");
                        codeWriter.WriteLine("};");
                    }
                }
                codeWriter.WriteLine("}");
            }
        }
    }
}