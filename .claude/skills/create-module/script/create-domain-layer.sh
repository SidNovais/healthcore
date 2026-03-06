#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
  echo "Use: ./create-domain-layer.sh [ModuleName] [RootNamespace] [BaseModulesDir]"
  exit 1
fi

MODULE_NAME=$1
ROOT_NS=$2
BASE_MODULES_DIR=$3
BASE_MODULE_DIR=${BASE_MODULES_DIR}/${MODULE_NAME}

dotnet new classlib -n ${ROOT_NS}.Modules.${MODULE_NAME}.Domain -o ${BASE_MODULE_DIR}/Domain

cat > "${BASE_MODULE_DIR}/Domain/DomainAssemblyInfo.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Domain;

public class DomainAssemblyInfo
{
}
EOF
