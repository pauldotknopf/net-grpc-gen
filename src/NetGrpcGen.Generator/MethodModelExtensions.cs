using System;
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
                throw new NotSupportedException();
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
                throw new NotSupportedException();
            }
        }
        
        public static void WriteSlotsDecl(this ProtoMethodModel val, CodeWriter writer)
        {
            var value = val.MethodDescriptor.InputType.FindFieldByName("value");
            if (value == null)
            {
                writer.WriteLine($"void {val.MethodName()}Handler(int requestId, QString error);");
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        
        public static void WriteSlotsImpl(this ProtoMethodModel val, CodeWriter writer)
        {
            var value = val.MethodDescriptor.InputType.FindFieldByName("value");
            if (value == null)
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
                throw new NotSupportedException();
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
    }
}