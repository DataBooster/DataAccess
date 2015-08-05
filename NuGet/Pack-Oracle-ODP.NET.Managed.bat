@ECHO OFF
CD /D %~dp0

IF NOT EXIST nupkg\Oracle.Managed MKDIR nupkg\Oracle.Managed

COPY Oracle\ODP.NET.Managed\DataAccess.nuspec ..\DataAccess\DataAccess.Oracle.Managed.nuspec /Y

..\.nuget\NuGet.exe pack ..\DataAccess\DataAccess.Oracle.Managed.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Oracle.Managed
