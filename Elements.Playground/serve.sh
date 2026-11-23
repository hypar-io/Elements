#!/usr/bin/env bash

echo "Building the app..."
dotnet publish -c release -p:RunAOTCompilation=false

echo "Serving the build assets at http://localhost:5001"
python3 -m serve.py