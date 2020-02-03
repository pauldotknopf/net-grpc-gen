using System;
using System.Collections.Generic;
using System.IO;
using NetGrpcGen.Infra.Impl;

namespace NetGrpcGen.Tests.Gen
{
    class Program
    {
        static void Main(string[] args)
        {
            var outputFile = "/home/pknopf/git/net-grpc-gen/tests/NetGrpcGen.Tests/Proto/gen.proto";
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            var discovery = new Discovery.Impl.DiscoveryService(new AttributeFinder(new FixedTypeFinder(
                new List<Type>
                {
                    typeof(Objects.Test1)
                })));
            
            using (var file = File.OpenWrite(outputFile))
            {
                new NetGrpcGen.CodeGen.ProtofileCodeGen().Generate(discovery.DiscoverObjects(), "Tests", file);
            }
        }
    }
}
