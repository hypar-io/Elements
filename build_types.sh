#!/usr/bin/env bash

BASE=$1
BRANCH=$(git branch --show-current)

dotnet run -p ./CoreTypeGenerator/CoreTypeGenerator.csproj ./Schemas ./Elements/src/Generate $1 $BRANCH