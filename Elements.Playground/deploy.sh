#!/usr/bin/env bash

source='./bin/Release/net6.0/wwwroot/_framework'

echo "Building the app..."
dotnet publish -c release

echo "Uploading assets to s3..."
aws s3 sync "$source" s3://elements-wasm/ --exclude "*.gz" --exclude "*.br"

echo "Creating an invalidation..."
aws cloudfront create-invalidation --distribution-id E19YQ16H2KTDNU --paths "/*"