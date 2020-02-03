dotnet publish "$(dirname $0)\..\HyparDynamo.sln"

packagePath="$APPDATA\Dynamo\Dynamo Revit\2.3\packages"
rm -r -f "$packagePath\HyparDyn"
cp -r "$(dirname $0)\HyparDyn" "$packagePath"

echo "Done copying package"