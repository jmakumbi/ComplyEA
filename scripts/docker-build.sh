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

echo "Building ComplyEA Docker image..."
docker build \
    --build-arg DEVEXPRESS_NUGET_KEY="${DEVEXPRESS_NUGET_KEY}" \
    --build-arg DEVEXPRESS_NUGET_USER="${DEVEXPRESS_NUGET_USER:-DevExpress}" \
    -t complyea:latest \
    .

echo "Build complete: complyea:latest"
