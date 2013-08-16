@ECHO OFF
COPY Oracle\DataDirect\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /Y
..\.nuget\NuGet.exe pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release
MOVE /Y *.nupkg nupkg
