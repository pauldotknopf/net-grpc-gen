using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using NetGrpcGen.ProtoModel;
using Serilog;

namespace NetGrpcGen.Generator
{
    public class Generator
    {
        private readonly IProtoModelBuilder _protoModelBuilder;

        public Generator(IProtoModelBuilder protoModelBuilder)
        {
            _protoModelBuilder = protoModelBuilder;
        }
        
        public CodeGeneratorResponse Generate(CodeGeneratorRequest request)
        {
            var response = new CodeGeneratorResponse();

            var objectModels = new List<ProtoObjectModel>();
            
            foreach (var protoFile in Google.Protobuf.Reflection.FileDescriptor.BuildFromByteStrings(
                request.ProtoFile.Select(x => x.ToByteString())))
            {
                objectModels.AddRange(_protoModelBuilder.BuildObjectModels(protoFile));
            }

            foreach (var objectModel in objectModels)
            {
                string implFile = null;
                var headerFile = BuildContent(header =>
                { 
                    implFile = BuildContent(impl =>
                    {
                        header.WriteLine($"#ifndef {objectModel.HeaderPragma()}");
                        header.WriteLine($"#define {objectModel.HeaderPragma()}");
                        header.WriteLine("#include <QObject>");
                        foreach (var nameSpace in objectModel.NamespaceComponents())
                        {
                            header.WriteLine($"namespace {nameSpace} {{");
                        }
                        
                        header.WriteLine($"class {objectModel.CppTypeName()} : public QObject {{");
                        header.WriteLineIndented("Q_OBJECT");
                        header.WriteLine("public:");
                        objectModel.WriteCtorDeclarations(header);
                        header.WriteLine("};");
                        foreach (var nameSpace in objectModel.NamespaceComponents())
                        {
                            header.WriteLine("}");
                        }
                        header.WriteLine($"#endif // {objectModel.HeaderPragma()}");
                        
                        
                        impl.WriteLine($"#include \"{objectModel.HeaderFile()}\"");
                        if (!string.IsNullOrEmpty(objectModel.CppNamespace()))
                        {
                            impl.WriteLine($"using namespace {objectModel.CppNamespace()};");
                        }
                        objectModel.WriteCtorImplementation(impl);
                        
                    });
                });
                response.File.Add(new CodeGeneratorResponse.Types.File
                {
                    Name = objectModel.HeaderFile(),
                    Content = headerFile
                });
                response.File.Add(new CodeGeneratorResponse.Types.File
                {
                    Name = objectModel.ImplFile(),
                    Content = implFile
                });
            }
            
            response.File.Add(new CodeGeneratorResponse.Types.File
            {
                Name = "roc.pri",
                Content = BuildContent(pri =>
                {
                    var headers = new List<string>();
                    var sources = new List<string>();
                    foreach (var file in response.File)
                    {
                        if (file.Name.EndsWith(".h"))
                        {
                            headers.Add(file.Name);   
                        }
                        if (file.Name.EndsWith(".cpp"))
                        {
                            sources.Add(file.Name);
                        }
                    }
                    pri.WriteLine("INCLUDEPATH += $$PWD");
                    pri.WriteLine("HEADERS += \\");
                    using (pri.Indent())
                    {
                        for (var x = 0; x < headers.Count; x++)
                        {
                            pri.WriteLine($"$$PWD/{headers[x]}{((x == headers.Count - 1) ? "" : " \\")}");
                        }
                    }
                    pri.WriteLine("SOURCES += \\");
                    using (pri.Indent())
                    {
                        for (var x = 0; x < sources.Count; x++)
                        {
                            pri.WriteLine($"$$PWD/{sources[x]}{((x == sources.Count - 1) ? "" : " \\")}");
                        }
                    }
                })
            });
            
            return response;
        }
        
        private string BuildContent(Action<CodeWriter> action)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, -1, true))
                {
                    action(new CodeWriter(streamWriter));
                }

                memoryStream.Position = 0;

                using (var streamReader = new StreamReader(memoryStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}