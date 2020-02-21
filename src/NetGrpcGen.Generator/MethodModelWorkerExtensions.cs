using System;
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
                throw new NotSupportedException();
            }
        }
        
        public static void WriteSignals(this WorkerWrapper val, CodeWriter writer)
        {
            var value = val.Model.MethodDescriptor.InputType.FindFieldByName("value");
            if (value == null)
            {
                writer.WriteLine($"void {val.Model.MethodName()}Done(int requestId, QString error);");
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static void WriteImpl(this WorkerWrapper val, CodeWriter writer)
        {
            var value = val.Model.MethodDescriptor.InputType.FindFieldByName("value");
            if (value == null)
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
                        writer.WriteLine("if(!invokeResult.ok()) {");
                        writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(requestId, QString::fromStdString(invokeResult.error_message()));");
                        writer.WriteLine("} else {");
                        writer.WriteLineIndented($"emit {val.Model.MethodName()}Done(requestId, QString());");
                        writer.WriteLine("}");
                    }
                    writer.WriteLine("});");
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}