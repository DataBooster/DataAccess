@ECHO OFF
COPY Oracle\ODP.NET\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /Y
NuGet pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release
MOVE /Y *.nupkg nupkg
