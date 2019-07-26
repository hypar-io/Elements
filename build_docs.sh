#!/usr/bin/env bash

echo 'Deleting the existing docs.'
rm -rf ./docs

echo 'Running example tests to generate updated sample data.'
dotnet test --filter "FullyQualifiedName~Elements.Tests.Examples"

echo 'Building the Elements docs.'
docfx ./doc/docfx.json -f