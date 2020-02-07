using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using NetGrpcGen.Infra.Impl;

namespace NetGrpcGen.Tests.Gen
{
    public class TestObject
    {

    }

    class Program
    {
        static void Exp(Expression<Action<TestObject>> sd)
        {
            
        }

        static void Method(object sdf)
        {
            
        }

        static void Main(string[] args)
        {
            Exp(x => Method(x));
            

            var outputFile = "/home/pknopf/git/net-grpc-gen/tests/NetGrpcGen.Tests/Objects/gen.proto";
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
