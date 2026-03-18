#!/usr/bin/env bash

set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

command -v pg_restore >/dev/null 2>&1 || {
  echo "pg_restore is required but not installed."
  exit 1
}

BACKUP_FILE="${1:-}"

if [[ -z "${BACKUP_FILE}" ]]; then
  echo "Usage: npm run db:restore -- ./backups/<file.dump|file.sql>"
  exit 1
fi

if [[ ! -f "${BACKUP_FILE}" ]]; then
  echo "Backup file not found: ${BACKUP_FILE}"
  exit 1
fi

case "${BACKUP_FILE}" in
  *.dump)
    echo "Restoring custom dump from ${BACKUP_FILE}"
    pg_restore \
      --clean \
      --if-exists \
      --no-owner \
      --no-privileges \
      --dbname="${DATABASE_URL}" \
      "${BACKUP_FILE}"
    ;;
  *.sql)
    echo "Restoring SQL file from ${BACKUP_FILE}"
    psql "${DATABASE_URL}" < "${BACKUP_FILE}"
    ;;
  *)
    echo "Unsupported backup format. Use a .dump or .sql file."
    exit 1
    ;;
esac

echo "Restore complete."
