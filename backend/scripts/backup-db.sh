#!/usr/bin/env bash

set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

BACKUP_DIR="${PROJECT_ROOT}/backups"
TIMESTAMP="$(date +"%Y%m%d-%H%M%S")"
BASE_NAME="guardians-backup-${TIMESTAMP}"
CUSTOM_DUMP_PATH="${BACKUP_DIR}/${BASE_NAME}.dump"
SCHEMA_DUMP_PATH="${BACKUP_DIR}/${BASE_NAME}-schema.sql"

mkdir -p "${BACKUP_DIR}"

echo "Creating full database backup at ${CUSTOM_DUMP_PATH}"
pg_dump \
  --dbname="${DATABASE_URL}" \
  --format=custom \
  --no-owner \
  --no-privileges \
  --file="${CUSTOM_DUMP_PATH}"

echo "Creating schema-only backup at ${SCHEMA_DUMP_PATH}"
pg_dump \
  --dbname="${DATABASE_URL}" \
  --schema-only \
  --no-owner \
  --no-privileges \
  --file="${SCHEMA_DUMP_PATH}"

echo "Backup complete."
echo "Full backup: ${CUSTOM_DUMP_PATH}"
echo "Schema backup: ${SCHEMA_DUMP_PATH}"
