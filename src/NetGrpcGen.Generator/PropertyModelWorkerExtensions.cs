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
                writer.WriteLine($"{model.Model.ObjectModel.CppNamespacePrefix()}{model.Model.Getter.InputType.Name} request;");
                writer.WriteLine($"{model.Model.ObjectModel.CppNamespacePrefix()}{model.Model.Getter.OutputType.Name} response;");
                writer.WriteLine("request.set_objectid(d_priv->objectId);");
                writer.WriteLine("grpc::ClientContext context;");
                writer.WriteLine($"auto result = d_priv->service->{model.Model.Getter.Name}(&context, request, &response);");
                writer.WriteLine($"if(!result.ok()) {{ qCritical(\"couldn't read property {model.Model.PropertyName}: %s\", result.error_message().c_str()); return {valueField.NativeType()}(); }}");
                writer.WriteLine("auto propValue = response.value();");
                // Tests::Test1PropStringGetRequest request;
                // Tests::Test1PropStringGetResponse response;
                // request.set_objectid(d_priv->objectId);
                // grpc::ClientContext context;
                // auto result = d_priv->service->GetPropertyPropString(&context, request, &response);
                // if(!result.ok()) {
                //     qCritical("unable to get property PropString: %s", result.error_message().c_str());
                //     return QJsonValue::Undefined;
                // }
                // auto propValue = response.value();
                // QJsonValue propResult;
                // ProtobufJsonConverter::messageToJsonValue(&propValue, propResult);
                // return propResult;
            }

            if (model.Model.Setter != null)
            {
                writer.WriteLine($"void {model.Model.ObjectModel.Worker().CppTypeName()}::{model.Model.GetSetterName()}({valueField.NativeType()} val)");
                using (writer.Indent(true))
                {
                
                }
            }
        }
    }
}