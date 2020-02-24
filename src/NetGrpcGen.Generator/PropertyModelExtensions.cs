using System;
using Google.Protobuf.Reflection;
using Humanizer;
using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class PropertyModelExtensions
    {
        public static string GetPropertyName(this ProtoPropertyModel model)
        {
            return model.PropertyName.Camelize();
        }

        public static string GetGetterName(this ProtoPropertyModel model)
        {
            return $"get{model.PropertyName.Pascalize()}";
        }
        
        public static string GetSetterName(this ProtoPropertyModel model)
        {
            return $"set{model.PropertyName.Pascalize()}";
        }

        public static string GetSignalChangedEventName(this ProtoPropertyModel model)
        {
            return $"{model.GetPropertyName()}Changed";
        }
        
        public static void WritePropertyDef(this ProtoPropertyModel model, CodeWriter writer)
        {
            var valueField = model.Setter.InputType.Fields["value"];
           
            var decl = $"Q_PROPERTY({valueField.NativeType()} {model.GetPropertyName()} READ {model.GetGetterName()}";
            if (model.Setter != null)
            {
                decl += $" WRITE {model.GetSetterName()}";
            }

            if (model.UpdatedEvent != null)
            {
                decl += $" NOTIFY {model.GetSignalChangedEventName()}";
            }

            decl += ")";
            
            writer.WriteLine(decl);
        }

        public static void WriteGetterSetterDecl(this ProtoPropertyModel model, CodeWriter writer)
        {
            var valueField = model.UpdatedEvent.Fields["value"];
            writer.WriteLine($"{valueField.NativeType()} {model.GetGetterName()}();");
            if (model.Setter != null)
            {
                writer.WriteLine($"void {model.GetSetterName()}({valueField.NativeType()} val);");
            }
        }
        
        public static void WriteGetterSetterImpl(this ProtoPropertyModel model, CodeWriter writer)
        {
            var valueField = model.UpdatedEvent.Fields["value"];
           
            writer.WriteLine($"{valueField.NativeType()} {model.ObjectModel.CppTypeName()}::{model.GetGetterName()}()");
            using (writer.Indent(true))
            {
                writer.WriteLine($"return d_priv->worker->{model.GetGetterName()}();");
            }

            if (model.Setter != null)
            {
                writer.WriteLine($"void {model.ObjectModel.CppTypeName()}::{model.GetSetterName()}({valueField.NativeType()} val)");
                using (writer.Indent(true))
                {
                    writer.WriteLine($"d_priv->worker->{model.GetSetterName()}(val);");
                }
            }
        }
        
        public static void WriteEventSignal(this ProtoPropertyModel model, CodeWriter writer)
        {
            if (model.UpdatedEvent == null)
            {
                return;
            }

            var valueField = model.UpdatedEvent.Fields["value"];
            writer.WriteLine($"void {model.GetSignalChangedEventName()}({valueField.NativeType()} val);");
        }
    }
}