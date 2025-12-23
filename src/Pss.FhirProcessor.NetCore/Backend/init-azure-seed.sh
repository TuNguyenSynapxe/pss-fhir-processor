#!/bin/bash
# Azure Startup Script to initialize seed files
# This runs on first deployment to copy seed files to /home/data/seed

SEED_DIR="/home/data/seed"
SOURCE_DIR="/home/site/wwwroot/seed"

echo "Initializing Azure seed files..."

# Create the directory if it doesn't exist
if [ ! -d "$SEED_DIR" ]; then
    echo "Creating $SEED_DIR directory..."
    mkdir -p "$SEED_DIR"
fi

# Copy seed files if they don't exist (preserve existing files)
if [ ! -f "$SEED_DIR/happy-sample-full.json" ] && [ -f "$SOURCE_DIR/happy-sample-full.json" ]; then
    echo "Copying happy-sample-full.json..."
    cp "$SOURCE_DIR/happy-sample-full.json" "$SEED_DIR/"
fi

if [ ! -f "$SEED_DIR/validation-metadata.json" ] && [ -f "$SOURCE_DIR/validation-metadata.json" ]; then
    echo "Copying validation-metadata.json..."
    cp "$SOURCE_DIR/validation-metadata.json" "$SEED_DIR/"
fi

echo "Seed files initialization complete!"
ls -lh "$SEED_DIR/"
