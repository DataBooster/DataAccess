@ECHO OFF
COPY SqlServer\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /y
..\.nuget\NuGet.exe pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release
MOVE /Y *.nupkg nupkg
