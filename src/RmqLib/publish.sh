set -e;
rm -rf bin
dotnet restore
dotnet build -c Release
dotnet pack -c Release


dotnet nuget push `ls ./bin/Release/*.nupkg`  --api-key oy2khrgn2jjzu6ukoybqtdjxeixpmjrurj4fu6yraot2aq --source https://api.nuget.org/v3/index.json