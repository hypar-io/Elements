#!/usr/bin/env bash

BASE=$1

dotnet run -p ./CoreTypeGenerator/CoreTypeGenerator.csproj ./Schemas ./Elements/src/Generate $1