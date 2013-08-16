@ECHO OFF
COPY Oracle\DataDirect\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /Y
NuGet pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release
MOVE /Y *.nupkg nupkg
