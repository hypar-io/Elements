#!/usr/bin/env bash

echo 'Building the app.'
dotnet build -c Release

echo 'Echo creating deploy directory.'
rm -r deploy
mkdir deploy
mkdir deploy/elements
mkdir deploy/elements/_framework

echo 'Copying assets.'
rsync -av  --exclude=*.gz --exclude=*.br ./bin/Release/net6.0/wwwroot/_framework deploy/elements
cp ./wwwroot/elements.js deploy/elements/elements.js 
cp ./wwwroot/index.html deploy/index.html

echo 'Running the test application.'
cd deploy
python3 -m http.server