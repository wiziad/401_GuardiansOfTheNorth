#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

if [[ -f "${PROJECT_ROOT}/.env" ]]; then
  set -a
  source "${PROJECT_ROOT}/.env"
  set +a
fi

if [[ -z "${DATABASE_URL:-}" ]]; then
  echo "DATABASE_URL is not set. Put it in backend/.env or export it before running this script."
  exit 1
fi

command -v pg_dump >/dev/null 2>&1 || {
  echo "pg_dump is required but not installed."
  exit 1
}

command -v psql >/dev/null 2>&1 || {
  echo "psql is required but not installed."
  exit 1
}
