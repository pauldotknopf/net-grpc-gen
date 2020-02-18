#!/usr/bin/env bash
set -e
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd $DIR

echo "Generating types.proto..."
protoc --cpp_out=./qt-client/proto \
    --grpc_out=./qt-client/proto \
    --roc_out=./qt-client/proto-roc \
    --plugin=protoc-gen-grpc=`which grpc_cpp_plugin` \
    -I../tests/NetGrpcGen.Tests/Objects \
    types.proto
echo "Generating gen.proto..."
protoc --cpp_out=./qt-client/proto \
    --grpc_out=./qt-client/proto \
    --roc_out=./qt-client/proto-roc \
    --plugin=protoc-gen-grpc=`which grpc_cpp_plugin` \
    -I../tests/NetGrpcGen.Tests/Objects \
    gen.proto