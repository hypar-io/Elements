#!/usr/bin/env bash

dotnet publish ./src/cli/hypar-cli.csproj -c Release -r linux-x64 &&
dotnet publish ./src/cli/hypar-cli.csproj -c Release -r osx.10.12-x64 &&
dotnet publish ./src/cli/hypar-cli.csproj -c Release -r win-x64 &&

zip -r hypar-linux-x64.zip -j ./src/cli/bin/Release/netcoreapp2.0/linux-x64/publish &&
zip -r hypar-osx.10.12-x64.zip -j ./src/cli/bin/Release/netcoreapp2.0/osx.10.12-x64/publish &&
zip -r hypar-win-x64.zip -j ./src/cli/bin/Release/netcoreapp2.0/win-x64/publish &&

aws s3 cp hypar-linux-x64.zip s3://hypar-cli
aws s3 cp hypar-osx.10.12-x64.zip s3://hypar-cli
aws s3 cp hypar-win-x64.zip s3://hypar-cli

rm hypar-linux-x64.zip
rm hypar-osx.10.12-x64.zip
rm hypar-win-x64.zip