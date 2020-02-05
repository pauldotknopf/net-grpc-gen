using System;
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
                    
                    writer.WriteLine($"message {o.Name}CreateResponse {{");
                    writer.WriteLine("\tuint64 objectId = 1;");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}StopRequest {{");
                    writer.WriteLine("}");
                    
                    writer.WriteLine($"message {o.Name}StopResponse {{");
                    writer.WriteLine("}");
                    
                    // foreach (var method in o.Methods)
                    // {
                    //     writer.WriteLine($"message {method.Name}Request {{");
                    //     writer.WriteLine("\tuint64 objectId = 1;");
                    //     index = 1;
                    //     foreach (var arg in method.Arguments)
                    //     {
                    //         index++;
                    //         writer.WriteLine($"\t{ToGrpcType(arg.DataType)} {arg.Name} = {index};");
                    //     }
                    //     writer.WriteLine("}");
                    //     writer.WriteLine($"message {method.Name}Response {{");
                    //     if (method.ReturnType != null)
                    //     {
                    //         writer.WriteLine($"\t{ToGrpcType(method.ReturnType.Value)} response = 1;");
                    //     }
                    //     writer.WriteLine("}");
                    // }
                    
                    writer.WriteLine($"service {serviceName} {{");
                    writer.WriteLine("\trpc Create (stream google.protobuf.Any) returns (stream google.protobuf.Any);");
                    foreach (var method in o.Methods)
                    {
                        writer.WriteLine($"\trpc {method.Name} ({method.RequestType.TypeName}) returns ({method.ResponseType.TypeName});");
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