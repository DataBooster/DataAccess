@ECHO OFF
COPY SqlServer\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /y
NuGet pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release
MOVE /Y *.nupkg nupkg
