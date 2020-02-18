# Generate the protofiles.

```bash
> ./examples
sudo apt-get install libprotobuf-dev libgrpc++-dev protobuf-compiler-grpc
protoc --cpp_out=./QtClient/proto \
    --grpc_out=./QtClient/proto \
    --plugin=protoc-gen-grpc=`which grpc_cpp_plugin` \
    -I../tests/NetGrpcGen.Tests/Objects \
    types.proto
protoc --cpp_out=./QtClient/proto \
    --grpc_out=./QtClient/proto \
    --plugin=protoc-gen-grpc=`which grpc_cpp_plugin` \
    -I../tests/NetGrpcGen.Tests/Objects \
    gen.proto
```