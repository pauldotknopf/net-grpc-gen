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
                        BuildWorker(objectModel, header, impl);
                    });
                });
                response.File.Add(new CodeGeneratorResponse.Types.File
                {
                    Name = objectModel.Worker().HeaderFile(),
                    Content = headerFile
                });
                response.File.Add(new CodeGeneratorResponse.Types.File
                {
                    Name = objectModel.Worker().ImplFile(),
                    Content = implFile
                });
            }
            
            foreach (var objectModel in objectModels)
            {
                string implFile = null;
                var headerFile = BuildContent(header =>
                { 
                    implFile = BuildContent(impl =>
                    {
                        BuildClass(objectModel, header, impl);
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

        private void BuildClass(ProtoObjectModel objectModel, CodeWriter header, CodeWriter impl)
        {
            header.WriteLine($"#ifndef {objectModel.HeaderPragma()}");
            header.WriteLine($"#define {objectModel.HeaderPragma()}");
            header.WriteLine("#include <QObject>");
            header.WriteLine("#include <QScopedPointer>");
            header.WriteLine("#include <QJSValue>");
            header.WriteLine("#include <QJsonValue>");
            header.WriteLine("#include <QVariant>");
            foreach (var nameSpace in objectModel.NamespaceComponents())
            {
                header.WriteLine($"namespace {nameSpace} {{");
            }
            header.WriteLine($"class {objectModel.CppTypeName()}Private;");
            header.WriteLine($"class {objectModel.CppTypeName()} : public QObject {{");
            header.WriteLineIndented("Q_OBJECT");
            using (header.Indent())
            {
                foreach (var property in objectModel.Properties)
                {
                    property.WritePropertyDef(header);
                }
            }
            header.WriteLine("public:");
            using (header.Indent())
            {
                header.WriteLine($"{objectModel.CppTypeName()}(QObject* parent = nullptr);");
                header.WriteLine($"~{objectModel.CppTypeName()}();");
                foreach (var method in objectModel.Methods)
                {
                    method.WriteDecl(header);
                }

                foreach (var property in objectModel.Properties)
                {
                    property.WriteGetterSetterDecl(header);
                }
            }
            header.WriteLine("signals:");
            using (header.Indent())
            {
                objectModel.WriteEventSignals(header);
                foreach (var property in objectModel.Properties)
                {
                    property.WriteEventSignal(header);
                }
            }
            header.WriteLine("private slots:");
            using (header.Indent())
            {
                foreach (var method in objectModel.Methods)
                {
                    method.WriteSlotsDecl(header);
                }
            }
            header.WriteLine("private:");
            using (header.Indent())
            {
                header.WriteLine($"QScopedPointer<{objectModel.CppTypeName()}Private> const d_priv;");
            }
            header.WriteLine("};");
            foreach (var nameSpace in objectModel.NamespaceComponents())
            {
                header.WriteLine("}");
            }
            header.WriteLine($"#endif // {objectModel.HeaderPragma()}");
                        
                        
            impl.WriteLine($"#include \"{objectModel.HeaderFile()}\"");
            impl.WriteLine($"#include \"{objectModel.Worker().HeaderFile()}\"");
            impl.WriteLine("#include <QThread>");
            impl.WriteLine("#include <QMap>");
            impl.WriteLine("#include <QJSValue>");
            impl.WriteLine("#include <QSharedPointer>");
            impl.WriteLine("#include <QQmlEngine>");
            impl.WriteLine("#include <QQmlContext>");
            impl.WriteLine("#include \"roc-lib/qroccommon.h\"");
            if (!string.IsNullOrEmpty(objectModel.CppNamespace()))
            {
                impl.WriteLine($"using namespace {objectModel.CppNamespace()};");
            }
            impl.WriteLine("struct CallbackRequest");
            impl.WriteLine("{");
            using (impl.Indent())
            {
                impl.WriteLine("QJSValue state;");
                impl.WriteLine("QJSValue callback;");
            }
            impl.WriteLine("};");
            impl.WriteLine($"class {objectModel.CppNamespacePrefix()}{objectModel.CppTypeName()}Private");
            impl.WriteLine("{");
            impl.WriteLine("public:");
            using (impl.Indent())
            {
                impl.WriteLine($"{objectModel.CppTypeName()}Private() : currentRequestId(0) {{}}");
                impl.WriteLine("QThread workerThread;");
                impl.WriteLine($"{objectModel.Worker().CppTypeName()}* worker;");
                impl.WriteLine("QMap<int, QSharedPointer<CallbackRequest>> requests;");
                impl.WriteLine("int currentRequestId;");
            }
            impl.WriteLine("};");
            impl.WriteLine($"{objectModel.CppTypeName()}::{objectModel.CppTypeName()}(QObject* parent) : QObject(parent), d_priv(new {objectModel.CppNamespacePrefix()}{objectModel.CppTypeName()}Private())");
            using (impl.Indent(true))
            {
                impl.WriteLine($"d_priv->worker = new {objectModel.Worker().CppTypeName()}();");
                impl.WriteLine("d_priv->worker->moveToThread(&d_priv->workerThread);");
                impl.WriteLine("connect(&d_priv->workerThread, SIGNAL(finished()), d_priv->worker, SLOT(deleteLater()));");
                impl.WriteLine("d_priv->workerThread.start();");
                foreach (var method in objectModel.Methods)
                {
                    impl.WriteLine($"connect(d_priv->worker, &{objectModel.Worker().CppTypeName()}::{method.MethodName()}Done, this, &{objectModel.CppTypeName()}::{method.MethodName()}Handler);");
                }
                foreach (var even in objectModel.Events)
                {
                    impl.WriteLine($"connect(d_priv->worker, &{objectModel.Worker().CppTypeName()}::{even.GetEventName()}Raised, this, &{objectModel.CppTypeName()}::{even.GetEventName()});");
                }

                foreach (var prop in objectModel.Properties)
                {
                    if (prop.UpdatedEvent == null)
                    {
                        continue;
                    }
                    impl.WriteLine($"connect(d_priv->worker, &{objectModel.Worker().CppTypeName()}::{prop.GetPropertyName()}Changed, this, &{objectModel.CppTypeName()}::{prop.GetPropertyName()}Changed);");
                }

                foreach (var property in objectModel.Properties)
                {
                    if (property.UpdatedEvent == null)
                    {
                        return;
                    }
                }
            }
            impl.WriteLine($"{objectModel.CppTypeName()}::~{objectModel.CppTypeName()}()");
            using (impl.Indent(true))
            {
                impl.WriteLine("d_priv->workerThread.quit();");
                impl.WriteLine("d_priv->workerThread.wait();");
            }
            foreach (var method in objectModel.Methods)
            {
                method.WriteImpl(impl);
            }
            foreach (var method in objectModel.Methods)
            {
                method.WriteSlotsImpl(impl);
            }

            foreach (var property in objectModel.Properties)
            {
                property.WriteGetterSetterImpl(impl);
            }
        }

        private void BuildWorker(ProtoObjectModel objectModel, CodeWriter header, CodeWriter impl)
        {
            header.WriteLine($"#ifndef {objectModel.Worker().HeaderPragma()}");
            header.WriteLine($"#define {objectModel.Worker().HeaderPragma()}");
            header.WriteLine("#include <QObject>");
            header.WriteLine("#include <QScopedPointer>");
            header.WriteLine("#include <QJsonValue>");
            header.WriteLine("#include <QVariant>");
            foreach (var nameSpace in objectModel.NamespaceComponents())
            {
                header.WriteLine($"namespace {nameSpace} {{");
            }
            header.WriteLine($"class {objectModel.Worker().CppTypeName()}Private;");
            header.WriteLine($"class {objectModel.Worker().CppTypeName()} : public QObject {{");
            header.WriteLineIndented("Q_OBJECT");
            header.WriteLine("public:");
            using (header.Indent())
            {
                header.WriteLine($"{objectModel.Worker().CppTypeName()}();");
                header.WriteLine($"~{objectModel.Worker().CppTypeName()}();");
                foreach (var method in objectModel.Methods)
                {
                    method.Worker().WriteDecl(header);
                }
                objectModel.Worker().WriteEventProcessDecl(header);
                foreach (var property in objectModel.Properties)
                {
                    property.Worker().WriteGetterSetterDecl(header);
                }
            }
            header.WriteLine("signals:");
            using (header.Indent())
            {
                foreach (var method in objectModel.Methods)
                {
                    method.Worker().WriteSignals(header);
                }
                objectModel.Worker().WriteEventSignals(header);
                foreach (var property in objectModel.Properties)
                {
                    property.Worker().WriteEventSignal(header);
                }
            }
            header.WriteLine("private:");
            using (header.Indent())
            {
                header.WriteLine($"QScopedPointer<{objectModel.Worker().CppTypeName()}Private> const d_priv;");
            }
            header.WriteLine("};");
            foreach (var nameSpace in objectModel.NamespaceComponents())
            {
                header.WriteLine("}");
            }
            header.WriteLine($"#endif // {objectModel.Worker().HeaderPragma()}");
            
            impl.WriteLine($"#include \"{objectModel.Worker().HeaderFile()}\"");
            foreach (var protoInclude in objectModel.GetAllProtoIncludeFiles())
            {
                impl.WriteLine($"#include \"{protoInclude}\"");
            }
            impl.WriteLine("#include \"roc-lib/qrocobjectadapter.h\"");
            impl.WriteLine("#include \"protobuf-qjson/protobufjsonconverter.h\"");
            impl.WriteLine("#include <QThread>");
            if (!string.IsNullOrEmpty(objectModel.CppNamespace()))
            {
                impl.WriteLine($"using namespace {objectModel.CppNamespace()};");
            }
            impl.WriteLine("class EventThread : public QThread");
            impl.WriteLine("{");
            impl.WriteLine("public:");
            using (impl.Indent())
            {
                impl.WriteLine($"std::shared_ptr<{objectModel.ProtoBufServiceName()}::Stub> service;");
                impl.WriteLine("grpc::ClientContext context;");
                impl.WriteLine("google::protobuf::uint64 objectId;");
                impl.WriteLine($"{objectModel.Worker().CppTypeName()}* worker;");
                impl.WriteLine("void run() override");
                using (impl.Indent(true))
                {
                    impl.WriteLine("if(!service) { return; }");
                    impl.WriteLine($"{objectModel.CppNamespacePrefix()}{objectModel.ListenEventsDescriptor.InputType.Name} request;");
                    impl.WriteLine("request.set_objectid(objectId);");
                    impl.WriteLine("auto stream = service->ListenEvents(&context, request);");
                    impl.WriteLine("google::protobuf::Any any;");
                    impl.WriteLine("while(stream->Read(&any)) { worker->processEvent(&any); }");
                    impl.WriteLine("auto result = stream->Finish();");
                    impl.WriteLine("if(result.error_code() == grpc::StatusCode::CANCELLED) { return; }");
                    impl.WriteLine("if(result.ok()) { qCritical(\"unabled to stop stream: %s\", result.error_message().c_str()); return; }");
                }
            }
            impl.WriteLine("};");
            impl.WriteLine($"class {objectModel.CppNamespacePrefix()}{objectModel.Worker().CppTypeName()}Private");
            impl.WriteLine("{");
            impl.WriteLine("public:");
            using (impl.Indent())
            {
                impl.WriteLine($"{objectModel.Worker().CppTypeName()}Private({objectModel.Worker().CppTypeName()}* worker) : objectId(0), worker(worker) {{}}");
                impl.WriteLine($"{objectModel.Worker().CppTypeName()}* worker;");
                impl.WriteLine("google::protobuf::uint64 objectId;");
                impl.WriteLine($"std::shared_ptr<{objectModel.ProtoBufServiceName()}::Stub> service;");
                impl.WriteLine("grpc::ClientContext objectRequestContext;");
                impl.WriteLine($"std::unique_ptr<grpc::ClientReader<{objectModel.CppNamespacePrefix()}{objectModel.CreateDescriptor.OutputType.Name}>> objectRequest;");
                impl.WriteLine("EventThread eventThread;");
                impl.WriteLine("void createObject()");
                using (impl.Indent(true))
                {
                    impl.WriteLine($"{objectModel.CppNamespacePrefix()}{objectModel.CreateDescriptor.InputType.Name} request;");
                    impl.WriteLine("objectRequest = service->Create(&objectRequestContext, request);");
                    impl.WriteLine($"{objectModel.CppNamespacePrefix()}{objectModel.CreateDescriptor.OutputType.Name} response;");
                    impl.WriteLine("if(!objectRequest->Read(&response)) { qCritical(\"\"); objectRequest.release(); return; }");
                    impl.WriteLine("objectId = response.objectid();");
                    impl.WriteLine("eventThread.objectId = objectId;");
                    impl.WriteLine("eventThread.service = service;");
                    impl.WriteLine("eventThread.worker = worker;");
                    impl.WriteLine("eventThread.start();");
                }
                impl.WriteLine("void releaseObject()");
                using (impl.Indent(true))
                {
                    impl.WriteLine("if(objectRequest != nullptr)");
                    using (impl.Indent(true))
                    {
                        impl.WriteLine("eventThread.context.TryCancel();");
                        impl.WriteLine("eventThread.quit();");
                        impl.WriteLine("eventThread.wait();");
                        impl.WriteLine("objectRequestContext.TryCancel();");
                        impl.WriteLine("auto result = objectRequest->Finish();");
                        impl.WriteLine("if(result.error_code() == grpc::StatusCode::CANCELLED) { return; }");
                        impl.WriteLine("if(!result.ok()) { qCritical(\"Couldn't release object: %s\", result.error_message().c_str()); }");
                    }
                }
                impl.WriteLine("bool isValid()");
                using (impl.Indent(true))
                {
                    impl.WriteLine("return objectRequest != nullptr;");
                }
            }
            impl.WriteLine("};");
            impl.WriteLine($"{objectModel.Worker().CppTypeName()}::{objectModel.Worker().CppTypeName()}() : QObject(nullptr), d_priv(new {objectModel.Worker().CppTypeName()}Private(this))");
            using (impl.Indent(true))
            {
                impl.WriteLine($"auto channel = QRocObjectAdapter::getSharedChannel();");
                impl.WriteLine("if(channel == nullptr) { qWarning(\"Set the channel to use via QRocObjectAdapter::setSharedChannel(...)\"); return; }");
                impl.WriteLine($"d_priv->service = {objectModel.ProtoBufServiceName()}::NewStub(channel);");
                impl.WriteLine("d_priv->createObject();");
            }
            impl.WriteLine($"{objectModel.Worker().CppTypeName()}::~{objectModel.Worker().CppTypeName()}()");
            using (impl.Indent(true))
            {
                impl.WriteLine("d_priv->releaseObject();");
            }
            
            foreach (var method in objectModel.Methods)
            {
                method.Worker().WriteImpl(impl);
            }
            objectModel.Worker().WriteEventProcessImpl(impl);
            foreach (var property in objectModel.Properties)
            {
                property.Worker().WriteGetterSetterImpl(impl);
            }
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