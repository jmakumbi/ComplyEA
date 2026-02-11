#!/bin/bash
set -euo pipefail

# Load environment variables
if [ -f .env ]; then
    export $(grep -v '^#' .env | xargs)
fi

if [ -z "${DEVEXPRESS_NUGET_KEY:-}" ]; then
    echo "ERROR: DEVEXPRESS_NUGET_KEY is not set."
    echo "Copy .env.example to .env and fill in your DevExpress NuGet API key."
    exit 1
fi

if [ -z "${DB_PASSWORD:-}" ]; then
    echo "ERROR: DB_PASSWORD is not set."
    echo "Set DB_PASSWORD in your .env file."
    exit 1
fi

echo "Starting ComplyEA with Docker Compose..."
docker-compose up --build -d

echo ""
echo "ComplyEA is starting at http://localhost:${APP_PORT:-5000}"
echo "PostgreSQL is available at localhost:${DB_PORT:-5432}"
echo ""
echo "View logs: docker-compose logs -f complyea"
echo "Stop:      docker-compose down"
