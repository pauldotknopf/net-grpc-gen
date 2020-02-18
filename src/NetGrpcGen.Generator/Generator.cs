using System.Linq;
using Google.Protobuf.Compiler;
using Serilog;

namespace NetGrpcGen.Generator
{
    public static class Generator
    {
        public static CodeGeneratorResponse Generate(CodeGeneratorRequest request)
        {
            var response = new CodeGeneratorResponse();

            var objectModels = request.ProtoFile.SelectMany(Model.ModelBuilder.BuildObjectModels).ToList();

            foreach (var objectModel in objectModels)
            {
                foreach (var property in objectModel.Properties)
                {
                    Log.Warning(property.PropertyName);
                }
            }
            
            return response;
        }
    }
}