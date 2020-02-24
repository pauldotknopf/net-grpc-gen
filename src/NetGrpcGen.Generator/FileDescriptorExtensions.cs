using Google.Protobuf.Reflection;

namespace NetGrpcGen.Generator
{
    public static class FileDescriptorExtensions
    {
        public static string CppNamespace(this FileDescriptor fileDescriptor)
        {
            return string.IsNullOrEmpty(fileDescriptor.Package) ? "" : fileDescriptor.Package.Replace(".", "::");
        }
        
        public static string CppNamespacePrefix(this FileDescriptor fileDescriptor)
        {
            var ns = fileDescriptor.CppNamespace();
            if (!string.IsNullOrEmpty(ns))
            {
                ns += "::";
            }

            return ns;
        }
    }
}