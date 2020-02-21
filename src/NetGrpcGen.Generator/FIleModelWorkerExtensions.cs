using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class FileModelWorkerExtensions
    {
        public class WorkerWrapper
        {
            public WorkerWrapper(ProtoObjectModel protoObjectModel)
            {
                ProtoObjectModel = protoObjectModel;
            }
            
            public ProtoObjectModel ProtoObjectModel { get; }
        }

        public static WorkerWrapper Worker(this ProtoObjectModel protoObjectModel)
        {
            return new WorkerWrapper(protoObjectModel);
        }
        
        public static string HeaderPragma(this WorkerWrapper objectModel)
        {
            return $"{objectModel.ProtoObjectModel.ServiceDescriptor.File.Name.ToUpper().Replace(".", "").Replace("/", "_")}_{objectModel.ProtoObjectModel.ObjectName.ToUpper()}_WORKER_H";
        }
        
        public static string HeaderFile(this WorkerWrapper objectModel)
        {
            return $"q{objectModel.ProtoObjectModel.ObjectName.ToLower()}worker.h";
        }

        public static string ImplFile(this WorkerWrapper objectModel)
        {
            return $"q{objectModel.ProtoObjectModel.ObjectName.ToLower()}worker.cpp";
        }
        
        public static string CppTypeName(this WorkerWrapper objectModel)
        {
            return $"Q{objectModel.ProtoObjectModel.ObjectName}Worker";
        }
    }
}