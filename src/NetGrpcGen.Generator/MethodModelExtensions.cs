using System;
using Google.Protobuf.Reflection;
using Humanizer;
using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class MethodModelExtensions
    {
        public static string MethodName(this ProtoMethodModel model)
        {
            return model.MethodName.Camelize();
        }
        
        public static void WriteDecl(this ProtoMethodModel val, CodeWriter writer)
        {
            var value = val.MethodDescriptor.InputType.FindFieldByName("value");
            if (value == null)
            {
                writer.WriteLine($"Q_INVOKABLE void {val.MethodName()}(QJSValue state, QJSValue callback);");
            }
            else
            {
                writer.WriteLine($"Q_INVOKABLE void {val.MethodName()}({val.RequestQtType()} val, QJSValue state, QJSValue callback);");
            }
        }
        
        public static void WriteImpl(this ProtoMethodModel val, CodeWriter writer)
        {
            var value = val.MethodDescriptor.InputType.FindFieldByName("value");
            if (value == null)
            {
                writer.WriteLine($"void {val.ObjectModel.CppTypeName()}::{val.MethodName()}(QJSValue state, QJSValue callback)");
                using (writer.Indent(true))
                {
                    writer.WriteLine("auto requestId = d_priv->currentRequestId++;");
                    writer.WriteLine("d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));");
                    writer.WriteLine($"d_priv->worker->{val.MethodName()}(requestId);");
                }
            }
            else
            {
                writer.WriteLine($"void {val.ObjectModel.CppTypeName()}::{val.MethodName()}({val.RequestQtType()} val, QJSValue state, QJSValue callback)");
                using (writer.Indent(true))
                {
                    writer.WriteLine("auto requestId = d_priv->currentRequestId++;");
                    writer.WriteLine("d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));");
                    writer.WriteLine($"d_priv->worker->{val.MethodName()}(val, requestId);");
                }
            }
        }
        
        public static void WriteSlotsDecl(this ProtoMethodModel val, CodeWriter writer)
        {
            var value = val.MethodDescriptor.OutputType.FindFieldByName("value");
            if (value == null)
            {
                writer.WriteLine($"void {val.MethodName()}Handler(int requestId, QString error);");
            }
            else
            {
                writer.WriteLine($"void {val.MethodName()}Handler({val.ResponseQtType()} result, int requestId, QString error);");
            }
        }
        
        public static void WriteSlotsImpl(this ProtoMethodModel val, CodeWriter writer)
        {
            var inputField = val.MethodDescriptor.OutputType.FindFieldByName("value");
            var outputField = val.MethodDescriptor.OutputType.FindFieldByName("value");
            if (outputField == null)
            {
                writer.WriteLine($"void {val.ObjectModel.CppTypeName()}::{val.MethodName()}Handler(int requestId, QString error)");
                using (writer.Indent(true))
                {
                    writer.WriteLine("if(!d_priv->requests.contains(requestId)) { qCritical(\"Couldn't find the given request id.\"); return; }");
                    writer.WriteLine("auto request = d_priv->requests.value(requestId);");
                    writer.WriteLine("d_priv->requests.remove(requestId);");
                    writer.WriteLine("if(request->callback.isCallable())");
                    using (writer.Indent(true))
                    {
                        writer.WriteLine("QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();");
                        writer.WriteLine("e.setProperty(\"state\", request->state);");
                        writer.WriteLine("e.setProperty(\"result\", QJSValue::NullValue);");
                        writer.WriteLine("e.setProperty(\"error\", error);");
                        writer.WriteLine("QJSValueList args;");
                        writer.WriteLine("args.push_back(e);");
                        writer.WriteLine("request->callback.call(args);");
                    }
                }
            }
            else
            {
                writer.WriteLine($"void {val.ObjectModel.CppTypeName()}::{val.MethodName()}Handler({val.ResponseQtType()} val, int requestId, QString error)");
                using (writer.Indent(true))
                {
                    writer.WriteLine("if(!d_priv->requests.contains(requestId)) { qCritical(\"Couldn't find the given request id.\"); return; }");
                    writer.WriteLine("auto request = d_priv->requests.value(requestId);");
                    writer.WriteLine("d_priv->requests.remove(requestId);");
                    writer.WriteLine("if(request->callback.isCallable())");
                    using (writer.Indent(true))
                    {
                        writer.WriteLine("auto engine = QQmlEngine::contextForObject(this)->engine();");
                        writer.WriteLine("QJSValue e = engine->newObject();");
                        writer.WriteLine("e.setProperty(\"state\", request->state);");
                        if (val.ResponseQtType() == "QJsonValue")
                        {
                            writer.WriteLine("e.setProperty(\"result\", convertJsonValueToJsValue(engine, val));");
                        }
                        else
                        {
                            writer.WriteLine("e.setProperty(\"result\", val);");   
                        }
                        writer.WriteLine("e.setProperty(\"error\", error);");
                        writer.WriteLine("QJSValueList args;");
                        writer.WriteLine("args.push_back(e);");
                        writer.WriteLine("request->callback.call(args);");
                    }
                }
            }
        }

        public static string ProtobufRequestCppType(this ProtoMethodModel val)
        {
            return $"{val.ObjectModel.CppNamespacePrefix()}{val.MethodDescriptor.InputType.Name}";
        }
        
        public static string ProtobufResponseCppType(this ProtoMethodModel val)
        {
            return $"{val.ObjectModel.CppNamespacePrefix()}{val.MethodDescriptor.OutputType.Name}";
        }
        
        public static string ProtobufRequestInnerCppType(this ProtoMethodModel val)
        {
            var valueField = val.MethodDescriptor.InputType.FindFieldByName("value");
            if (valueField == null)
            {
                return null;
            }
            var ns = valueField.MessageType.File.Package.Replace(".", "::");
            if (!string.IsNullOrEmpty(ns))
            {
                ns += "::";
            }
            return $"{ns}{valueField.MessageType.Name}";
        }

        public static string RequestQtType(this ProtoMethodModel val)
        {
            var field = val.MethodDescriptor.InputType.FindFieldByName("value");
            if (field == null)
            {
                return null;
            }

            switch (field.FieldType)
            {
                case FieldType.Int32:
                    return "int";
                case FieldType.Message:
                    return "QJsonValue";
                default:
                    throw new Exception($"Not supported: {field.FieldType}");
            }
        }

        public static string ResponseQtType(this ProtoMethodModel val)
        {
            var field = val.MethodDescriptor.OutputType.FindFieldByName("value");
            if (field == null)
            {
                return null;
            }

            switch (field.FieldType)
            {
                case FieldType.Int32:
                    return "int";
                case FieldType.Message:
                    return "QJsonValue";
                default:
                    throw new Exception($"Not supported: {field.FieldType}");
            }
        }
    }
}