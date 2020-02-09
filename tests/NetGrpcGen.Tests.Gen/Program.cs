using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using NetGrpcGen.Infra.Impl;

namespace NetGrpcGen.Tests.Gen
{
    class Program
    {
        static void Main(string[] args)
        {
            var discovery = new Discovery.Impl.DiscoveryService(new AttributeFinder(new FixedTypeFinder(
                new List<Type>
                {
                    typeof(Objects.Test1)
                })));

            var protoCode = CodeGen.ProtofileCodeGen.Generate(discovery.DiscoverObjects(), "Tests");
            var outputFile = "/home/pknopf/git/net-grpc-gen/tests/NetGrpcGen.Tests/Objects/gen.proto";
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
            File.WriteAllText(outputFile, protoCode);
        }
    }
}
