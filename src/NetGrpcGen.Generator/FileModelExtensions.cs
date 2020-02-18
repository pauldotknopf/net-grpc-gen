using System.Collections.Generic;
using System.Linq;
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

        public static List<string> NamespaceComponents(this ProtoObjectModel objectModel)
        {
            return objectModel.CppNamespace().Split("::").ToList();
        }
        
        public static void WriteCtorDeclarations(this ProtoObjectModel val, CodeWriter writer)
        {
            using (writer.Indent())
            {
                writer.WriteLine($"{val.CppTypeName()}(QObject* parent = nullptr);");
                writer.WriteLine($"~{val.CppTypeName()}();");
            }
        }

        public static void WriteCtorImplementation(this ProtoObjectModel val, CodeWriter writer)
        {
            writer.WriteLine($"{val.CppTypeName()}::{val.CppTypeName()}(QObject* parent) : QObject(parent)");
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine($"{val.CppTypeName()}::~{val.CppTypeName()}()");
            writer.WriteLine("{");
            writer.WriteLine("}");
        }
    }
}