#!/usr/bin/env bash

set -euo pipefail

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"

echo "Listing public tables for the current database..."
psql "${DATABASE_URL}" -c "\dt public.*"
