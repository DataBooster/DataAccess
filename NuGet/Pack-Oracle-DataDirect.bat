@ECHO OFF
CD /D %~dp0

IF NOT EXIST nupkg\Oracle.DataDirect MKDIR nupkg\Oracle.DataDirect

COPY Oracle\DataDirect\DataAccess.nuspec ..\DataAccess\DataAccess.Oracle.DataDirect.nuspec /Y

..\.nuget\NuGet.exe pack ..\DataAccess\DataAccess.Oracle.DataDirect.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Oracle.DataDirect
