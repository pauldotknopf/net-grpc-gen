using System;
using Google.Protobuf.Reflection;
using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class PropertyModelWorkerExtensions
    {
        public class WorkerWrapper
        {
            public WorkerWrapper(ProtoPropertyModel model)
            {
                Model = model;
            }
            
            public ProtoPropertyModel Model { get; }
        }
        
        public static WorkerWrapper Worker(this ProtoPropertyModel model)
        {
            return new WorkerWrapper(model);
        }
        
        public static void WriteEventSignal(this WorkerWrapper model, CodeWriter writer)
        {
            model.Model.WriteEventSignal(writer);
        }
        
        public static void WriteGetterSetterDecl(this WorkerWrapper model, CodeWriter writer)
        {
            var valueField = model.Model.UpdatedEvent.Fields["value"];
            
            writer.WriteLine($"{valueField.NativeType()} {model.Model.GetGetterName()}();");

            if (model.Model.Setter != null)
            {
                writer.WriteLine($"void {model.Model.GetSetterName()}({valueField.NativeType()} val);");
            }
        }
        
        public static void WriteGetterSetterImpl(this WorkerWrapper model, CodeWriter writer)
        {
            var valueField = model.Model.UpdatedEvent.Fields["value"];
            
            writer.WriteLine($"{valueField.NativeType()} {model.Model.ObjectModel.Worker().CppTypeName()}::{model.Model.GetGetterName()}()");
            using (writer.Indent(true))
            {
                writer.WriteLine($"{model.Model.Getter.InputType.File.CppNamespacePrefix()}{model.Model.Getter.InputType.Name} request;");
                writer.WriteLine($"{model.Model.Getter.OutputType.File.CppNamespacePrefix()}{model.Model.Getter.OutputType.Name} response;");
                writer.WriteLine("request.set_objectid(d_priv->objectId);");
                writer.WriteLine("grpc::ClientContext context;");
                writer.WriteLine($"auto result = d_priv->service->{model.Model.Getter.Name}(&context, request, &response);");
                writer.WriteLine($"if(!result.ok()) {{ qCritical(\"couldn't read property {model.Model.PropertyName}: %s\", result.error_message().c_str()); return {valueField.DefaultValue()}; }}");
                valueField.MarshalMessagePropertyToField(writer, "propValue", "response");
                writer.WriteLine("return propValue;");
            }

            if (model.Model.Setter != null)
            {
                writer.WriteLine($"void {model.Model.ObjectModel.Worker().CppTypeName()}::{model.Model.GetSetterName()}({valueField.NativeType()} val)");
                using (writer.Indent(true))
                {
                    writer.WriteLine($"{model.Model.Setter.InputType.File.CppNamespacePrefix()}{model.Model.Setter.InputType.Name} request;");
                    writer.WriteLine($"{model.Model.Setter.OutputType.File.CppNamespacePrefix()}{model.Model.Setter.OutputType.Name} response;");
                    writer.WriteLine("request.set_objectid(d_priv->objectId);");
                    valueField.MarshalValueToMessageProperty(writer, "val", "request");
                    writer.WriteLine("grpc::ClientContext context;");
                    writer.WriteLine($"auto result = d_priv->service->{model.Model.Setter.Name}(&context, request, &response);");
                    writer.WriteLine($"if(!result.ok()) {{ qCritical(\"couldn't set property {model.Model.GetPropertyName()}: %s\", result.error_message().c_str()); }}");
                }
            }
        }
    }
}