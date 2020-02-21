using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Google.Protobuf.Reflection;
using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class FileModelExtensions
    {
        public static string HeaderPragma(this ProtoObjectModel objectModel)
        {
            return $"{objectModel.ServiceDescriptor.File.Name.ToUpper().Replace(".", "").Replace("/", "_")}_{objectModel.ObjectName.ToUpper()}_H";
        }
        
        public static string CppTypeName(this ProtoObjectModel objectModel)
        {
            return $"Q{objectModel.ObjectName}";
        }

        public static string ProtoBufServiceName(this ProtoObjectModel objectModel)
        {
            return $"{objectModel.CppNamespacePrefix()}{objectModel.ServiceDescriptor.Name}";
        }

        public static List<string> GetAllProtoIncludeFiles(this ProtoObjectModel objectModel)
        {
            var all = new List<FileDescriptor>();
            all.Add(objectModel.ServiceDescriptor.File);
            all.AddRange(objectModel.ServiceDescriptor.File.Dependencies);

            return all.Select(x =>
            {
                var name = objectModel.ServiceDescriptor.File.Name;
                if (name.EndsWith(".proto"))
                {
                    name = name.Substring(0, name.Length - ".proto".Length);
                }

                return name.ToLower();
            }).Distinct().SelectMany(x => new List<string>
            {
                $"{x}.pb.h",
                $"{x}.grpc.pb.h"
            }).ToList();
        }
        
        public static string HeaderFile(this ProtoObjectModel objectModel)
        {
            return $"q{objectModel.ObjectName.ToLower()}.h";
        }

        public static string ImplFile(this ProtoObjectModel objectModel)
        {
            return $"q{objectModel.ObjectName.ToLower()}.cpp";
        }
    
        public static string CppNamespace(this ProtoObjectModel objectModel)
        {
            return string.IsNullOrEmpty(objectModel.ServiceDescriptor.File.Package) ? "" : objectModel.ServiceDescriptor.File.Package.Replace(".", "::");
        }
        
        public static string CppNamespacePrefix(this ProtoObjectModel objectModel)
        {
            var ns = objectModel.CppNamespace();
            if (!string.IsNullOrEmpty(ns))
            {
                ns += "::";
            }

            return ns;
        }

        public static List<string> NamespaceComponents(this ProtoObjectModel objectModel)
        {
            return objectModel.CppNamespace().Split("::").ToList();
        }
    }
}