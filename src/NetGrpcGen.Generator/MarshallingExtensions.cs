using System;
using Google.Protobuf.Reflection;
using NetGrpcGen.ProtoModel;
using Serilog;

namespace NetGrpcGen.Generator
{
    public static class MarshallingExtensions
    {
        public static string NativeType(this FieldDescriptor fieldDescriptor)
        {
            switch (fieldDescriptor.FieldType)
            {
                case FieldType.Message:
                    switch (fieldDescriptor.MessageType.FullName)
                    {
                        case "google.protobuf.StringValue":
                            return "QVariant";
                        default:
                            return "QJsonValue";
                    }
                case FieldType.String:
                    return "QString";
                case FieldType.Bytes:
                    return "QByteArray";
                default:
                    return "bool";
                    throw new Exception($"Unsupported type: {fieldDescriptor.FieldType}");
            }
        }

        public static string DefaultValue(this FieldDescriptor fieldDescriptor)
        {
            switch (fieldDescriptor.FieldType)
            {
                case FieldType.Message:
                    switch (fieldDescriptor.MessageType.FullName)
                    {
                        case "google.protobuf.StringValue":
                            return "QVariant()";
                        default:
                            return "QJsonValue::Undefined";
                    }
                case FieldType.String:
                    return "QString()";
                case FieldType.Bool:
                    return "false";
                default:
                    return "false";
                    throw new Exception($"Unsupported type: {fieldDescriptor.FieldType}");
            }
        }

        public static void MarshalToVariable(this FieldDescriptor fieldDescriptor)
        {
            
        }

        public static void MarshalMessagePropertyToField(this FieldDescriptor fieldDescriptor,
            CodeWriter writer,
            string valueFieldName,
            string messageFieldName)
        {
            switch (fieldDescriptor.FieldType)
            {
                case FieldType.Message:
                    switch (fieldDescriptor.MessageType.FullName)
                    {
                        case "google.protobuf.StringValue":
                            writer.WriteLine($"QVariant {valueFieldName} = QVariant::fromValue(nullptr);");
                            writer.WriteLine($"if({messageFieldName}.has_value())");
                            using (writer.Indent(true))
                            {
                                writer.WriteLine($"{valueFieldName} = QString::fromStdString({messageFieldName}.value().value());");
                            }
                            break;
                        default:
                            writer.WriteLine($"QJsonValue {valueFieldName};");
                            writer.WriteLine($"if({messageFieldName}.has_value())");
                            using (writer.Indent(true))
                            {
                                // TODO: Check return type.
                                writer.WriteLine($"auto {messageFieldName}MessageValue = {messageFieldName}.value();");
                                writer.WriteLine(
                                    $"ProtobufJsonConverter::messageToJsonValue(&{messageFieldName}MessageValue, {valueFieldName});");
                            }
                            break;
                    }
                    break;
                default:
                    writer.WriteLine($"auto {valueFieldName} = {messageFieldName}.value();");
                    break;
            }
        }
        
        public static void MarshalValueToMessageProperty(this FieldDescriptor fieldDescriptor,
            CodeWriter writer, 
            string valueFieldName,
            string messageFieldName)
        {
            switch (fieldDescriptor.FieldType)
            {
                case FieldType.Message:
                    switch (fieldDescriptor.MessageType.FullName)
                    {
                        case "google.protobuf.StringValue":
                            writer.WriteLine("if(val.userType() == QMetaType::QString)");
                            using (writer.Indent(true))
                            {
                                writer.WriteLine($"auto messageVal = new {fieldDescriptor.MessageType.File.CppNamespacePrefix()}{fieldDescriptor.MessageType.Name}();");
                                writer.WriteLine($"messageVal->set_value({valueFieldName}.toString().toStdString());");
                                writer.WriteLine($"{messageFieldName}.set_allocated_value(messageVal);");
                            }
                            break;
                        default:
                            writer.WriteLine("if(!val.isNull())");
                            using (writer.Indent(true))
                            {
                                // TODO: Check response.
                                writer.WriteLine(
                                    $"auto messageVal = new {fieldDescriptor.MessageType.File.CppNamespacePrefix()}{fieldDescriptor.MessageType.Name}();");
                                writer.WriteLine(
                                    $"ProtobufJsonConverter::jsonValueToMessage({valueFieldName}, messageVal);");
                                writer.WriteLine($"{messageFieldName}.set_allocated_value(messageVal);");
                            }

                            break;
                    }
                    break;
                default:
                    writer.WriteLine($"request.set_value(val);");
                    break;
            }
        }
    }
}