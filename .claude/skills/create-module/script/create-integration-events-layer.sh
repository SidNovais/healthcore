#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
  echo "Use: ./create-integration-events-layer.sh [ModuleName] [RootNamespace] [BaseModulesDir]"
  exit 1
fi

MODULE_NAME=$1
ROOT_NS=$2
BASE_MODULES_DIR=$3
BASE_MODULE_DIR=${BASE_MODULES_DIR}/${MODULE_NAME}

dotnet new classlib -n ${ROOT_NS}.Modules.${MODULE_NAME}.IntegrationEvents -o ${BASE_MODULE_DIR}/IntegrationEvents

cat > "${BASE_MODULE_DIR}/IntegrationEvents/IntegrationEventsAssemblyInfo.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.IntegrationEvents;

public class IntegrationEventsAssemblyInfo { }
EOF
