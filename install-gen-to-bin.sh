#!/usr/bin/env bash

echo "Installing to /usr/bin/protoc-gen-roc"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

sudo rm -rf /usr/bin/protoc-gen-roc

echo "#!/usr/bin/env bash" | sudo tee -a /usr/bin/protoc-gen-roc > /dev/null
echo "exec dotnet exec $DIR/src/NetGrpcGen.Generator/bin/Debug/netcoreapp3.1/NetGrpcGen.Generator.dll \$*" | sudo tee -a /usr/bin/protoc-gen-roc > /dev/null
sudo chmod +x /usr/bin/protoc-gen-roc

echo "Done!"