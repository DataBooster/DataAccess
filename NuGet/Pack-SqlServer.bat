@ECHO OFF
CD /D %~dp0
COPY SqlServer\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /y
..\.nuget\NuGet.exe pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release;Platform=AnyCPU
MOVE /Y *.nupkg nupkg
