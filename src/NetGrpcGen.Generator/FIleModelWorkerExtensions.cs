using System;
using Google.Protobuf.Reflection;
using Humanizer;
using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class FileModelWorkerExtensions
    {
        public class WorkerWrapper
        {
            public WorkerWrapper(ProtoObjectModel protoObjectModel)
            {
                ProtoObjectModel = protoObjectModel;
            }
            
            public ProtoObjectModel ProtoObjectModel { get; }
        }

        public static WorkerWrapper Worker(this ProtoObjectModel protoObjectModel)
        {
            return new WorkerWrapper(protoObjectModel);
        }
        
        public static string HeaderPragma(this WorkerWrapper objectModel)
        {
            return $"{objectModel.ProtoObjectModel.ServiceDescriptor.File.Name.ToUpper().Replace(".", "").Replace("/", "_")}_{objectModel.ProtoObjectModel.ObjectName.ToUpper()}_WORKER_H";
        }
        
        public static string HeaderFile(this WorkerWrapper objectModel)
        {
            return $"q{objectModel.ProtoObjectModel.ObjectName.ToLower()}worker.h";
        }

        public static string ImplFile(this WorkerWrapper objectModel)
        {
            return $"q{objectModel.ProtoObjectModel.ObjectName.ToLower()}worker.cpp";
        }
        
        public static string CppTypeName(this WorkerWrapper objectModel)
        {
            return $"Q{objectModel.ProtoObjectModel.ObjectName}Worker";
        }

        public static void WriteEventProcessDecl(this WorkerWrapper objectModel, CodeWriter writer)
        {
            writer.WriteLine("void processEvent(void* event);");
        }

        public static void WriteEventSignals(this WorkerWrapper objectModel, CodeWriter writer)
        {
            foreach (var even in objectModel.ProtoObjectModel.Events)
            {
                var valueField = even.MessageDescriptor.FindFieldByName("value");
                if (valueField == null)
                {
                    writer.WriteLine($"void {even.GetEventName()}Raised();");
                }
                else
                {
                    switch (valueField.FieldType)
                    {
                        case FieldType.String:
                            writer.WriteLine($"void {even.GetEventName()}Raised(QString val);");
                            break;
                        case FieldType.Message:
                            writer.WriteLine($"void {even.GetEventName()}Raised(QJsonValue val);");
                            break;
                        default:
                            throw new Exception($"Unsupported event type: {valueField.FieldType}");
                    }
                }
            }
        }
        
        public static void WriteEventProcessImpl(this WorkerWrapper objectModel, CodeWriter writer)
        {
            writer.WriteLine($"void {objectModel.CppTypeName()}::processEvent(void* _event)");
            using (writer.Indent(true))
            {
                writer.WriteLine("auto event = reinterpret_cast<google::protobuf::Any*>(_event);");
                foreach (var ev in objectModel.ProtoObjectModel.Events)
                {
                    writer.WriteLine($"if(event->Is<{objectModel.ProtoObjectModel.CppNamespacePrefix()}{ev.MessageDescriptor.Name}>())");
                    using (writer.Indent(true))
                    {
                        var valueField = ev.MessageDescriptor.FindFieldByName("value");
                        if (valueField == null)
                        {
                            writer.WriteLine($"emit {ev.GetEventName()}Raised();");
                        }
                        else
                        {
                            writer.WriteLine($"{objectModel.ProtoObjectModel.CppNamespacePrefix()}{ev.MessageDescriptor.Name} eventMessage;");
                            writer.WriteLine("event->UnpackTo(&eventMessage);");
                            writer.WriteLine("auto eventValue = eventMessage.value();");
                            switch (valueField.FieldType)
                            {
                                case FieldType.String:
                                    writer.WriteLine($"emit {ev.GetEventName()}Raised(QString::fromStdString(eventValue.value()));");
                                    break;
                                case FieldType.Message:
                                    writer.WriteLine("QJsonValue jsonValue;");
                                    // TODO: Check return type.
                                    writer.WriteLine("ProtobufJsonConverter::messageToJsonValue(&eventValue, jsonValue);");
                                    writer.WriteLine($"emit {ev.GetEventName()}Raised(jsonValue);");
                                    break;
                                default:
                                    throw new Exception($"Unsupported event type: {valueField.FieldType}");
                            }
                        }
                    }
                }

                foreach (var property in objectModel.ProtoObjectModel.Properties)
                {
                    if (property.UpdatedEvent == null)
                    {
                        continue;
                    }
                    
                    writer.WriteLine($"if(event->Is<{objectModel.ProtoObjectModel.CppNamespacePrefix()}{property.UpdatedEvent.Name}>())");
                    using (writer.Indent(true))
                    {
                        writer.WriteLine($"{objectModel.ProtoObjectModel.CppNamespacePrefix()}{property.UpdatedEvent.Name} eventMessage;");
                        writer.WriteLine("event->UnpackTo(&eventMessage);");
                        writer.WriteLine("auto eventValue = eventMessage.value();");
                        var valueField = property.UpdatedEvent.Fields["value"];
                        switch (valueField.FieldType)
                        {
                            case FieldType.String:
                                writer.WriteLine($"emit {property.GetPropertyName()}Changed(QString::fromStdString(eventValue.value()));");
                                break;
                            case FieldType.Message:
                                writer.WriteLine("QJsonValue jsonValue;");
                                // TODO: Check return type.
                                writer.WriteLine("ProtobufJsonConverter::messageToJsonValue(&eventValue, jsonValue);");
                                writer.WriteLine($"emit {property.GetPropertyName()}Changed(jsonValue);");
                                break;
                            default:
                                throw new Exception($"Unsupported event type: {valueField.FieldType}");
                        }
                    }
                }
                writer.WriteLine("qDebug(\"got event: %s\", event->type_url().c_str());");
            }
        }
    }
}