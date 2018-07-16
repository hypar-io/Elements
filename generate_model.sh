#!/usr/bin/env bash
dotnet build -c Release ./csharp/src/generate/generate.csproj
echo 'Generating c# classes in /csharp/src/sdk/Elements.g.cs...'
dotnet ./csharp/src/generate/bin/Release/netcoreapp2.1/generate.dll ./elements.json ./csharp/src/sdk/Elements.g.cs