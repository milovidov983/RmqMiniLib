set -e;
rm -rf bin
dotnet restore
dotnet build -c Release
dotnet pack -c Release


dotnet nuget push `ls ./bin/Release/*.nupkg`  --api-key SECRET_KEY --source https://api.nuget.org/v3/index.json