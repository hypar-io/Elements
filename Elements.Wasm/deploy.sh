#!/usr/bin/env bash

configuration='debug'

while getopts c: flag
do
    case "${flag}" in
        c) configuration=${OPTARG};;
    esac
done

if [ $configuration == 'release' ]
then
    source='./bin/Release/net6.0/publish/wwwroot/_framework'
elif [ $configuration == 'debug'  ]
then
    source='./bin/Release/net6.0/wwwroot/_framework'
else
    echo "The configuration, $configuration, is invalid. Only 'release' and 'debug' are supported."
    exit
fi

# echo "Building the app in $configuration configuration."
# if [ $configuration == 'release' ]
# then
#     dotnet publish -c release
# elif [ $configuration == 'debug'  ]
# then
#     dotnet build -c release
# fi

echo 'Echo creating deploy directory.'
rm -r deploy
mkdir deploy
mkdir deploy/elements
mkdir deploy/elements/_framework

echo "Copying assets from $source"
rsync -av  --exclude=*.gz --exclude=*.br $source deploy/elements
cp ./wwwroot/elements.js deploy/elements/elements.js 
cp ./wwwroot/index.html deploy/index.html

# echo 'Running the test application.'
cd deploy
python3 -m http.server