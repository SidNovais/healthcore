#!/bin/bash
if [ -z "$1" ]; then
  echo "Use: ./create-module.sh [ModuleName] [RootNamespace=HC.LIS] [BaseModulesDir=./src/HC.LIS/HC.LIS.Modules]"
  exit 1
fi

MODULE_NAME=$1
ROOT_NS=${2:-HC.LIS}
BASE_MODULES_DIR=${3:-./src/HC.LIS/HC.LIS.Modules}
BASE_MODULE_DIR=${BASE_MODULES_DIR}/${MODULE_NAME}

if [ -d "${BASE_MODULE_DIR}" ]; then
  echo "Error: Module directory '${BASE_MODULE_DIR}' already exists. Aborting."
  exit 1
fi

mkdir -p "${BASE_MODULE_DIR}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

bash "${SCRIPT_DIR}/create-domain-layer.sh" "$MODULE_NAME" "$ROOT_NS" "$BASE_MODULES_DIR"
bash "${SCRIPT_DIR}/create-application-layer.sh" "$MODULE_NAME" "$ROOT_NS" "$BASE_MODULES_DIR"
bash "${SCRIPT_DIR}/create-infrastructure-layer.sh" "$MODULE_NAME" "$ROOT_NS" "$BASE_MODULES_DIR"
bash "${SCRIPT_DIR}/create-integration-events-layer.sh" "$MODULE_NAME" "$ROOT_NS" "$BASE_MODULES_DIR"
bash "${SCRIPT_DIR}/create-arch-test-layer.sh" "$MODULE_NAME" "$ROOT_NS" "$BASE_MODULES_DIR"
bash "${SCRIPT_DIR}/create-unit-test-layer.sh" "$MODULE_NAME" "$ROOT_NS" "$BASE_MODULES_DIR"
bash "${SCRIPT_DIR}/create-integration-test-layer.sh" "$MODULE_NAME" "$ROOT_NS" "$BASE_MODULES_DIR"

find "${BASE_MODULE_DIR}" -type f -name "Class1.cs" -delete
