using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetGrpcGen.Model;

namespace NetGrpcGen.CodeGen
{
    public class ProtofileCodeGen
    {
        public void Generate(List<GrpcObject> objects, string packageName, Stream outputStream)
        {
            using (var writer = new StreamWriter(outputStream))
            {
                writer.WriteLine("syntax = \"proto3\";");
                if (!string.IsNullOrEmpty(packageName))
                {
                    writer.WriteLine($"package {packageName};");
                }
                writer.WriteLine("import \"google/protobuf/any.proto\";");
                foreach (var o in objects)
                {
                    var serviceName = $"{o.Name}ObjectService";
                    
                    writer.WriteLine($"enum {serviceName}Property {{");
                    int index = 0;
                    foreach (var prop in o.Properties.OrderBy(x => x.Name))
                    {
                        writer.WriteLine($"\t{prop.Name} = {index};");
                        index++;
                    }
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}SetPropRequest {{");
                    writer.WriteLine("\tuint64 objectId = 1;");
                    writer.WriteLine($"\t{serviceName}Property prop = 2;");
                    writer.WriteLine("\toneof value {");
                    writer.WriteLine("\t\tstring str = 3;");
                    writer.WriteLine("\t}");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}SetPropResponse {{");
                    writer.WriteLine("\tuint64 objectId = 1;");
                    writer.WriteLine($"\t{serviceName}Property prop = 2;");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}GetPropRequest {{");
                    writer.WriteLine("\tuint64 objectId = 1;");
                    writer.WriteLine($"\t{serviceName}Property prop = 2;");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}GetPropResponse {{");
                    writer.WriteLine("\tuint64 objectId = 1;");
                    writer.WriteLine($"\t{serviceName}Property prop = 2;");
                    writer.WriteLine("\toneof value {");
                    writer.WriteLine("\t\tstring str = 3;");
                    writer.WriteLine("\t}");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}CreateResponse {{");
                    writer.WriteLine("\tuint64 objectId = 1;");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}StopRequest {{");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}StopResponse {{");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}PropChanged {{");
                    writer.WriteLine("\tuint64 objectId = 1;");
                    writer.WriteLine($"\t{serviceName}Property prop = 2;");
                    writer.WriteLine("\toneof value {");
                    writer.WriteLine("\t\tstring str = 3;");
                    writer.WriteLine("\t}");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"service {serviceName} {{");
                    writer.WriteLine("\trpc Create (stream google.protobuf.Any) returns (stream google.protobuf.Any);");
                    writer.WriteLine($"\trpc GetProperty ({o.Name}GetPropRequest) returns ({o.Name}GetPropResponse);");
                    writer.WriteLine($"\trpc SetProperty ({o.Name}SetPropRequest) returns ({o.Name}SetPropResponse);");
                    writer.WriteLine("}");
                }
            }
        }
    }
}