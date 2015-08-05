@ECHO OFF
CD /D %~dp0

IF NOT EXIST nupkg\Oracle.ODP MKDIR nupkg\Oracle.ODP

COPY Oracle\ODP.NET\DataAccess.nuspec ..\DataAccess\DataAccess.Oracle.ODP.nuspec /Y

..\.nuget\NuGet.exe pack ..\DataAccess\DataAccess.Oracle.ODP.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Oracle.ODP
