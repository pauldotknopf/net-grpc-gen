using System;
using Google.Protobuf.Reflection;
using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class MarshallingExtensions
    {
        public static string NativeType(this FieldDescriptor fieldDescriptor)
        {
            switch (fieldDescriptor.FieldType)
            {
                case FieldType.Message:
                    return "QJsonValue";
                case FieldType.String:
                    return "QString";
                case FieldType.Bytes:
                    return "QByteArray";
                default:
                    return "bool";
                    throw new Exception($"Unsupported type: {fieldDescriptor.FieldType}");
            }
        }

        public static void MarshalToVariable(this FieldDescriptor fieldDescriptor)
        {
            
        }

        public static void MarshalToReturn(this FieldDescriptor fieldDescriptor)
        {
            
        }

        public static void Marshal(this FieldDescriptor fieldDescriptor)
        {
            
        }

        public static void MarshalValueToMessageProperty(this FieldDescriptor fieldDescriptor,
            ProtoObjectModel objectModel,
            CodeWriter writer, 
            string valueFieldName,
            string messageFieldName)
        {
            if (fieldDescriptor.FieldType == FieldType.Message)
            {
                writer.WriteLine($"auto messageVal = new {objectModel.CppNamespacePrefix()}{fieldDescriptor.MessageType.Name}();");
                // TODO: Check response.
                writer.WriteLine($"ProtobufJsonConverter::jsonValueToMessage({valueFieldName}, messageVal);");
                writer.WriteLine($"{messageFieldName}.set_allocated_value(messageVal);");
            }
            else
            {
                writer.WriteLine($"request.set_value(val);");
            }
        }
    }
}