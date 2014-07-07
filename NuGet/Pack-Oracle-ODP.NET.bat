@ECHO OFF
CD /D %~dp0
COPY Oracle\ODP.NET\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /Y
..\.nuget\NuGet.exe pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release;Platform=AnyCPU
MOVE /Y *.nupkg nupkg
