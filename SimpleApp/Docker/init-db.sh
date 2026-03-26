#!/bin/bash
set -e

# simple-db is already created by POSTGRES_DB env var.
# This script creates the second database for the log server.
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    SELECT 'CREATE DATABASE "simple-log-db"'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'simple-log-db')\gexec
EOSQL
