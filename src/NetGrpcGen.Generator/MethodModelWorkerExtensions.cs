using System;
using Google.Protobuf.Reflection;
using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class MethodModelWorkerExtensions
    {
        public class WorkerWrapper
        {
            public WorkerWrapper(ProtoMethodModel model)
            {
                Model = model;
            }
            
            public ProtoMethodModel Model { get; }
        }
        
        public static WorkerWrapper Worker(this ProtoMethodModel model)
        {
            return new WorkerWrapper(model);
        }

        public static void WriteDecl(this WorkerWrapper val, CodeWriter writer)
        {
            var value = val.Model.MethodDescriptor.InputType.FindFieldByName("value");
            if (value == null)
            {
                writer.WriteLine($"void {val.Model.MethodName()}(int requestId);");
            }
            else
            {
                writer.WriteLine($"void {val.Model.MethodName()}({val.Model.RequestQtType()} request, int requestId);");
            }
        }
        
        public static void WriteSignals(this WorkerWrapper val, CodeWriter writer)
        {
            var value = val.Model.MethodDescriptor.OutputType.FindFieldByName("value");
            if (value == null)
            {
                writer.WriteLine($"void {val.Model.MethodName()}Done(int requestId, QString error);");
            }
            else
            {
                writer.WriteLine($"void {val.Model.MethodName()}Done({val.Model.ResponseQtType()} val, int requestId, QString error);");
            }
        }

        public static void WriteImpl(this WorkerWrapper val, CodeWriter writer)
        {
            var inputField = val.Model.MethodDescriptor.InputType.FindFieldByName("value");
            var outputField = val.Model.MethodDescriptor.OutputType.FindFieldByName("value");
            if (inputField == null)
            {
                writer.WriteLine($"void {val.Model.ObjectModel.Worker().CppTypeName()}::{val.Model.MethodName()}(int requestId)");
                using (writer.Indent(true))
                {
                    writer.WriteLine("QMetaObject::invokeMethod(this, [this, requestId] {");
                    using (writer.Indent())
                    {
                        writer.WriteLine($"{val.Model.ProtobufRequestCppType()} request;");
                        writer.WriteLine($"{val.Model.ProtobufResponseCppType()} response;");
                        writer.WriteLine("request.set_objectid(d_priv->objectId);");
                        writer.WriteLine("grpc::ClientContext context;");
                        writer.WriteLine($"auto invokeResult = d_priv->service->{val.Model.MethodDescriptor.Name}(&context, request, &response);");
                        if (outputField != null)
                        {
                            writer.WriteLine("auto responseValue = response.value();");
                            writer.WriteLine("if(!invokeResult.ok()) {");
                            writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));");
                            writer.WriteLine("} else {");
                            writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(responseValue, requestId, QString());");
                            writer.WriteLine("}");
                        }
                        else
                        {
                            writer.WriteLine("if(!invokeResult.ok()) {");
                            writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(requestId, QString::fromStdString(invokeResult.error_message()));");
                            writer.WriteLine("} else {");
                            writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(requestId, QString());");
                            writer.WriteLine("}");
                        }
                        
                    }
                    writer.WriteLine("});");
                }
            }
            else
            {
                writer.WriteLine($"void {val.Model.ObjectModel.Worker().CppTypeName()}::{val.Model.MethodName()}({val.Model.RequestQtType()} val, int requestId)");
                using (writer.Indent(true))
                {
                    writer.WriteLine("QMetaObject::invokeMethod(this, [this, val, requestId] {");
                    using (writer.Indent())
                    {
                        writer.WriteLine($"{val.Model.ProtobufRequestCppType()} request;");
                        writer.WriteLine($"{val.Model.ProtobufResponseCppType()} response;");
                        writer.WriteLine("request.set_objectid(d_priv->objectId);");
                        if (inputField.FieldType == FieldType.Message)
                        {
                            writer.WriteLine($"auto messageVal = new {val.Model.ProtobufRequestInnerCppType()}();");
                            // TODO: Check response.
                            writer.WriteLine("ProtobufJsonConverter::jsonValueToMessage(val, messageVal);");
                            writer.WriteLine("request.set_allocated_value(messageVal);");
                        }
                        else
                        {
                            writer.WriteLine($"request.set_value(val);");
                        }

                        writer.WriteLine("grpc::ClientContext context;");
                        writer.WriteLine($"auto invokeResult = d_priv->service->{val.Model.MethodDescriptor.Name}(&context, request, &response);");
                       
                        if (outputField == null)
                        {
                            writer.WriteLine("if(!invokeResult.ok()) {");
                            writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(requestId, QString::fromStdString(invokeResult.error_message()));");
                            writer.WriteLine("} else {");
                            writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(requestId, QString());");
                            writer.WriteLine("}");
                        }
                        else
                        {
                            writer.WriteLine("auto responseValue = response.value();");
                            if (outputField.FieldType == FieldType.Message)
                            {
                                writer.WriteLine("QJsonValue messageJson;");
                                // TODO: Check response
                                writer.WriteLine("ProtobufJsonConverter::messageToJsonValue(&responseValue, messageJson);");
                                writer.WriteLine("if(!invokeResult.ok()) {");
                                writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(messageJson, requestId, QString::fromStdString(invokeResult.error_message()));");
                                writer.WriteLine("} else {");
                                writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(messageJson, requestId, QString());");
                                writer.WriteLine("}");
                            }
                            else
                            {
                                writer.WriteLine("if(!invokeResult.ok()) {");
                                writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));");
                                writer.WriteLine("} else {");
                                writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(responseValue, requestId, QString());");
                                writer.WriteLine("}");
                            }
                        }
                    }
                    writer.WriteLine("});");
                }
            }
        }
    }
}